using FastBite.Shared.DTOS;

namespace FastBite.Core.Interfaces;

public interface IOrderService {
    public Task<List<CreateOrderDTO>> GetAllOrdersAsync(string token);
    public Task<CreateOrderDTO> GetOrderByIdAsync(Guid orderId);
    public Task<CreateOrderDTO> CreateOrderAsync(CreateOrderDTO orderDTO);
    public Task<CreateOrderDTO> EditOrderAsync(Guid orderId, int tableNumber);
    public Task DeleteOrderAsync(Guid Id);
}