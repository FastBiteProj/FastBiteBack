namespace FastBite.Data.DTOS;
public record CheckTableAvailabilityDto
(
    DateTime ReservationDate,
    int GuestsCount
);