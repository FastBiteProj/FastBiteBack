using FastBite.Shared.DTOS;
using FastBite.Shared.Enum;

namespace FastBite.Core.Interfaces;

public interface IOrderService {
    public Task<List<CreateOrderDTO>> GetAllOrdersAsync(string token);
    public Task<OrderReceiptDTO> TryLockAndPayOrderAsync(Guid orderId, OrderStatus newStatus, string languageCode);
    public Task<CreateOrderDTO> CancelOrderAsync(Guid userId);
    public Task<CreateOrderDTO> GetOrderByIdAsync(Guid orderId);
    public Task<CreateOrderDTO> CreateOrderAsync(CreateOrderDTO orderDTO);
    public Task<CreateOrderDTO> EditOrderAsync(Guid orderId, int tableNumber);
    public Task DeleteOrderAsync(Guid Id);
}