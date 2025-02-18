namespace FastBite.Data.DTOS
{
    public record CreateOrderDTO
    (
        Guid Id,
        ICollection<OrderProductDTO> ProductNames,
        Guid UserId,
        int TotalPrice,
        int TableNumber,
        DateTime ConfirmationDate
    );
}
