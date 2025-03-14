namespace FastBite.Shared.DTOS;
public record OrderItemDTO
(
     string ProductId,
     Guid UserId,
     int Quantity 
);