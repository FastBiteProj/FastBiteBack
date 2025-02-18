using FastBite.Data.DTOS;
using FastBite.Data.Models;

namespace FastBite.Services.Interfaces;

public interface IReservationService {
    public Task<List<ReservationDTO>> GetAllReservationsAsync(string token);
    public Task<ReservationDTO> CreateReservationAsync(ReservationDTO reservation);
    public Task<ReservationDTO> EditReservation(Guid Id, ReservationDTO reservation);
    public Task DeleteReservationAsync(Guid Id);
}