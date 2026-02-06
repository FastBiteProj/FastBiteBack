using AutoMapper;
using FastBite.Implementation.Configs;
using FastBite.Infrastructure.Contexts;
using FastBite.Shared.DTOS;
using FastBite.Core.Models;
using FastBite.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using FastBite.Shared.Enum;

namespace FastBite.Implementation.Classes
{
    public class OrderService : IOrderService
    {
        public readonly FastBiteContext _context;
        public readonly ITokenService _tokenService;
        public readonly IMapper _mapper;

        public OrderService(FastBiteContext context, ITokenService tokenService)
        {
            _context = context;
            _mapper = MappingConfiguration.InitializeConfig();
            _tokenService = tokenService;
        }


       public async Task<CreateOrderDTO> CreateOrderAsync(CreateOrderDTO orderDTO)
        {
            var existingOrder = await _context.Orders
                .FirstOrDefaultAsync(o => 
                    o.TableNumber == orderDTO.TableNumber && 
                    o.Status != OrderStatus.Paid && 
                    o.Status != OrderStatus.Cancelled);
            
            if (existingOrder != null)
            {
                throw new InvalidOperationException($"Table {orderDTO.TableNumber} already has an active order (ID: {existingOrder.Id})");
            }
            
            if (orderDTO.Products == null || !orderDTO.Products.Any())
            {
                throw new ArgumentException("You need to add at least one product.");
            }
            
            var productNames = orderDTO.Products
                .Select(p => p.ProductName)
                .Distinct()
                .ToList();
            
            var products = await _context.Products
                .Include(p => p.Translations)
                .Where(p => p.Translations.Any(t => productNames.Contains(t.Name)))
                .ToListAsync();
            
            if (products.Count != productNames.Count)
            {
                throw new ArgumentException("One or more products not found.");
            }
            
            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = orderDTO.UserId,
                TotalPrice = 0,
                OrderItems = new List<OrderItem>(),
                ConfirmationDate = DateTime.Now,
                TableNumber = orderDTO.TableNumber,
                Status = OrderStatus.Created 
            };
            
            foreach (var item in orderDTO.Products)
            {
                var product = products.FirstOrDefault(p => 
                    p.Translations.Any(t => t.Name == item.ProductName));
                
                if (product == null)
                {
                    throw new ArgumentException($"Product '{item.ProductName}' not found.");
                }
                
                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    Order = order
                };
                
                order.OrderItems.Add(orderItem);
                order.TotalPrice += product.Price * item.Quantity;
            }
            
            try
            {
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Could not create order.", ex);
            }
            
            var orderDTOResult = _mapper.Map<CreateOrderDTO>(order);
            return orderDTOResult;
        }

        public async Task<OrderReceiptDTO> TryLockAndPayOrderAsync(Guid orderId, OrderStatus newStatus, string languageCode)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Translations)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            
            if (order == null)
            {
                throw new Exception("Order not found.");
            }
            
            if (order.Status == OrderStatus.Paid)
            {
                return BuildReceipt(order);
            }

            if (order.Status == OrderStatus.Cancelled)
            {
                throw new InvalidOperationException("Order is cancelled.");
            }
            
            if (order.Status == OrderStatus.Cancelled)
            {
                throw new InvalidOperationException("Order is cancelled and cannot be paid.");
            }
            
            var originalRowVersion = order.RowVersion;
            
            try
            {
                order.Status = newStatus;
                _context.Entry(order).Property(x => x.RowVersion).OriginalValue = originalRowVersion;
                
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new InvalidOperationException("Order was modified by another user. Please try again.");
            }
            
            var receipt = new OrderReceiptDTO
            {
                OrderId = order.Id,
                TableNumber = order.TableNumber,
                ConfirmationDate = order.ConfirmationDate,
                TotalPrice = order.TotalPrice,
                Items = order.OrderItems.Select(oi =>
                {
                    var translation = oi.Product.Translations
                        .FirstOrDefault(t => t.LanguageCode == languageCode)
                        ?? oi.Product.Translations.FirstOrDefault();

                    return new OrderReceiptItemDTO
                    {
                        ProductName = translation?.Name ?? "Unknown",
                        Quantity = oi.Quantity,
                        Price = oi.Product.Price,
                    };
                }).ToList()
            };

            return receipt;
        }

        public async Task<CreateOrderDTO> CancelOrderAsync(Guid orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Translations)
                .FirstOrDefaultAsync(o => o.Id == orderId)
                ?? throw new Exception("Order not found.");

            if (order.Status == OrderStatus.Paid)
                throw new InvalidOperationException("Paid order cannot be cancelled.");

            order.Status = OrderStatus.PaymentCancelled;

            await _context.SaveChangesAsync();
            return _mapper.Map<CreateOrderDTO>(order);
        }


        public async Task<CreateOrderDTO> GetOrderByIdAsync(Guid orderId)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(op => op.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                throw new Exception("Order not found.");
            }

            var orderDTO = _mapper.Map<CreateOrderDTO>(order);
            return orderDTO;
        }

        public async Task<List<CreateOrderDTO>> GetAllOrdersAsync(string token)
        {
            IQueryable<Order> query = _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.User);
            var principal = _tokenService.GetPrincipalFromToken(token, validateLifetime: true);

            var email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            query = await Functions.GetFilteredDataByUserRoleAsync(user, query, _context);

            var orders = await query
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Translations)
                .Include(o => o.User)
                .ToListAsync();
            return _mapper.Map<List<CreateOrderDTO>>(orders);
        }


        public async Task<List<CreateOrderDTO>> GetUserOrdersAsync(Guid userId)
        {
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                    .ThenInclude(op => op.Product)
                .ToListAsync();

            var orderDTOs = _mapper.Map<List<CreateOrderDTO>>(orders);
            return orderDTOs;
        }

        public async Task<CreateOrderDTO> EditOrderAsync(Guid orderId, int tableNumber)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                throw new Exception("Order not found.");
            }

            try
            {
                order.TableNumber = tableNumber;
                
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
                return _mapper.Map<CreateOrderDTO>(order);

            }
            catch (Exception ex)
            {
                throw new Exception("Could not update order.", ex);
            }
        }

        public async Task DeleteOrderAsync(Guid orderId)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
            {
                throw new Exception("Order not found.");
            }

            try
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Could not delete order.", ex);
            }
        }

        private static OrderReceiptDTO BuildReceipt(Order order)
        {
            return new OrderReceiptDTO
            {
                OrderId = order.Id,
                TableNumber = order.TableNumber,
                ConfirmationDate = order.ConfirmationDate,
                TotalPrice = order.TotalPrice,
                Items = order.OrderItems.Select(oi => new OrderReceiptItemDTO
                {
                    ProductName =
                        oi.Product.Translations.FirstOrDefault()?.Name ?? "Unknown",
                    Quantity = oi.Quantity,
                    Price = oi.Product.Price
                }).ToList()
            };
        }
    }
}
