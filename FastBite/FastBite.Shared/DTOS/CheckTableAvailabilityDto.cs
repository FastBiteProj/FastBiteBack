namespace FastBite.Shared.DTOS;
public record CheckTableAvailabilityDto
(
    DateTime ReservationDate,
    int GuestsCount
);