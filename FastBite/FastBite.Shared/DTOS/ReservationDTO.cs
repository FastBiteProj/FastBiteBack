namespace FastBite.Shared.DTOS;
public record ReservationDTO (
    Guid Id,
    string ReservationStartTime, 
    string ReservationEndTime,  
    string ReservationDate,
    int GuestCount,
    int? TableNumber,
    Guid UserId,
    CreateOrderDTO? Order
);