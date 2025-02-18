using Azure.Storage.Blobs.Models;

namespace FastBite.Data.DTOS;

public record TableDTO (
    int TableNumber,
    int TableCapacity,
    List<ReservationDTO> ReservationsOnDate
);