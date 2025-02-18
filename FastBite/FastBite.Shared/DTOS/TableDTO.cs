namespace FastBite.Shared.DTOS;

public record TableDTO (
    int TableNumber,
    int TableCapacity,
    List<ReservationDTO> ReservationsOnDate
);