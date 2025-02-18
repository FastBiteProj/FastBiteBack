using AutoMapper;
using FastBite.Data.Configs;
using FastBite.Data.Contexts;
using FastBite.Data.DTOS;
using FastBite.Data.Models;
using FastBite.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace FastBite.Services.Classes
{
    public class OrderService : IOrderService
    {
        public readonly FastBiteContext _context;
        public readonly ITokenService _tokenService;
        public readonly Mapper _mapper;

        public OrderService(FastBiteContext context, ITokenService tokenService)
        {
            _context = context;
            _mapper = MappingConfiguration.InitializeConfig();
            _tokenService = tokenService;
        }


       public async Task<CreateOrderDTO> CreateOrderAsync(CreateOrderDTO orderDTO)
        {
            if (orderDTO.ProductNames == null || !orderDTO.ProductNames.Any())
            {
                throw new ArgumentException("You need to add at least one product.");
            }

            var productNames = orderDTO.ProductNames.Select(p => p.ProductName).ToList();

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
                UserId = orderDTO.UserId,
                TotalPrice = 0,
                OrderItems = new List<OrderItem>(),
                ConfirmationDate = DateTime.Now,
                TableNumber = orderDTO.TableNumber
            };

            foreach (var item in orderDTO.ProductNames)
            {
                var product = products.FirstOrDefault(p => p.Translations.Any(t => t.Name == item.ProductName));
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

        public async Task<CreateOrderDTO> EditOrderAsync(Guid orderId, CreateOrderDTO orderDTO)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                throw new Exception("Order not found.");
            }

            if (orderDTO.ProductNames == null || !orderDTO.ProductNames.Any())
            {
                throw new Exception("You need to add at least one product.");
            }

            var productNames = orderDTO.ProductNames.Select(i => i.ProductName).ToList();
            var products = await _context.Products
                .Include(p => p.Translations)
                .Where(p => p.Translations.Any(t => productNames.Contains(t.Name)))
                .ToListAsync();

            if (products.Count != productNames.Count)
            {
                throw new ArgumentException("One or more products not found.");
            }

            _context.OrderItems.RemoveRange(order.OrderItems);
            order.OrderItems.Clear();

            order.TotalPrice = 0;
            foreach (var item in orderDTO.ProductNames)
            {
                var product = products.FirstOrDefault(p => p.Translations.Any(t => t.Name == item.ProductName));
                if (product == null)
                {
                    throw new Exception($"Product '{item.ProductName}' not found.");
                }

                var orderProduct = new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    OrderId = order.Id
                };

                order.OrderItems.Add(orderProduct);

                order.TotalPrice += product.Price * item.Quantity;
            }

            try
            {
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                var updatedOrderDTO = _mapper.Map<CreateOrderDTO>(order);
                return updatedOrderDTO;
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
    }
}
