using FastBite.Data.DTOS;
using FastBite.Data.Models;

namespace FastBite.Services.Interfaces;

public interface IOrderService {
    public Task<List<CreateOrderDTO>> GetAllOrdersAsync(string token);
    public Task<CreateOrderDTO> GetOrderByIdAsync(Guid orderId);
    public Task<CreateOrderDTO> CreateOrderAsync(CreateOrderDTO orderDTO);
    public Task<CreateOrderDTO> EditOrderAsync(Guid orderId, CreateOrderDTO orderDTO);
    public Task DeleteOrderAsync(Guid Id);
}