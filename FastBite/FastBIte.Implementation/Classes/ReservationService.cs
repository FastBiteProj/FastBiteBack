using System.Security.Claims;
using AutoMapper;
using FastBite.Implementation.Configs;
using FastBite.Infrastructure.Contexts;
using FastBite.Shared.DTOS;
using FastBite.Core.Models;
using FastBite.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FastBite.Implementation.Classes;

public class ReservationService : IReservationService
{
    public FastBiteContext _context;
    public ITokenService _tokenService;
    public IOrderService _orderService;
    private readonly IMapper mapper;

    public ReservationService(FastBiteContext context, ITokenService tokenService, IOrderService orderService) 
    {
        _context = context;
        mapper = MappingConfiguration.InitializeConfig();
        _tokenService = tokenService;
        _orderService = orderService;
    }

    public async Task<ReservationDTO> CreateReservationAsync(ReservationDTO reservation) 
    {
        Table table = null;
        var requestedStart = TimeOnly.Parse(reservation.ReservationStartTime);
        var requestedEnd = TimeOnly.Parse(reservation.ReservationEndTime);
        var requestedDate = DateOnly.Parse(reservation.ReservationDate);

        if (reservation.TableNumber == null) { 
            table = await _context.Tables
                .Include(t => t.Reservations)
                .FirstOrDefaultAsync(t => t.Capacity >= reservation.GuestCount);
        }
        else {
            table = await _context.Tables
                .Include(t => t.Reservations)
                .FirstOrDefaultAsync(t => t.Capacity >= reservation.GuestCount && t.Number == reservation.TableNumber);
        }

        if (table == null)
        {
            throw new Exception("Table not found with given capacity");
        }

        var existingReservations = table.Reservations?
            .Where(r => r.ReservationDate == requestedDate)
            .ToList() ?? new List<Reservation>();

        if (existingReservations.Count >= 5)
        {
            throw new Exception("Table has reached maximum reservations for this day");
        }

        bool hasTimeConflict = existingReservations.Any(r =>
            (requestedStart >= r.ReservationStart && requestedStart < r.ReservationEnd) ||
            (requestedEnd > r.ReservationStart && requestedEnd <= r.ReservationEnd) ||
            (requestedStart <= r.ReservationStart && requestedEnd >= r.ReservationEnd)
        );

        if (hasTimeConflict)
        {
            throw new Exception("Table is already reserved for this time period");
        }

        Guid? orderId = null;
        if (reservation.Order != null && reservation.Order.ProductNames != null && reservation.Order.ProductNames.Any(p => !string.IsNullOrEmpty(p.ProductName) && p.ProductName != "string" && p.Quantity > 0))  
        {
            var order = await _orderService.CreateOrderAsync(reservation.Order);
            Console.WriteLine("-------------_________-------------____________-------------reservation");
            orderId = order?.Id;
        }

        var newReservation = new Reservation
        {
            ReservationStart = requestedStart,
            ReservationEnd = requestedEnd,
            ReservationDate = requestedDate,
            GuestCount = reservation.GuestCount,
            ConfirmationDate = DateTime.Now, 
            UserId = reservation.UserId,
            TableId = table.Id,
            OrderId = orderId
        };

        table.Reservations ??= new List<Reservation>();
        table.Reservations.Add(newReservation);

        _context.Reservations.Add(newReservation); 
        _context.Tables.Update(table);

        await _context.SaveChangesAsync(); 
        return mapper.Map<ReservationDTO>(newReservation);
    }

    public async Task DeleteReservationAsync(Guid Id)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Table)
            .Include(r => r.Order)
                .ThenInclude(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Translations)
            .FirstOrDefaultAsync(r => r.Id == Id);

        if (reservation == null) // throw new Exception("Reservation not found"); if there is null
        {
            throw new KeyNotFoundException("Reservation not found");
        }

        _context.Reservations.Remove(reservation);
        await _context.SaveChangesAsync();
    }

    public async Task<ReservationDTO> EditReservation(Guid Id, ReservationDTO reservation) 
    {
        var currentReservation = await _context.Reservations
            .Include(r => r.Table)
            .Include(r => r.Order)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == Id);

        if (currentReservation == null) 
        {
            throw new Exception("Reservation not found");
        }

        var table = await _context.Tables
            .FirstOrDefaultAsync(t => t.Capacity >= reservation.GuestCount);

        if (table == null)
        {
            throw new Exception("Table not found with given capacity");
        }

        currentReservation.ReservationStart = TimeOnly.Parse(reservation.ReservationStartTime);
        currentReservation.ReservationEnd = TimeOnly.Parse(reservation.ReservationEndTime);
        currentReservation.ReservationDate = DateOnly.Parse(reservation.ReservationDate);
        currentReservation.TableId = table.Id;

        _context.Reservations.Update(currentReservation);
        await _context.SaveChangesAsync();

        return mapper.Map<ReservationDTO>(currentReservation);
    }

    public async Task<List<ReservationDTO>> GetAllReservationsAsync(string token) // Get All reservations without id and for each user
    {
        IQueryable<Reservation> query = _context.Reservations;
        var principal = _tokenService.GetPrincipalFromToken(token, validateLifetime: true);

        var email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        query = await Functions.GetFilteredDataByUserRoleAsync(user, query, _context);

        var reservations = await query
            .Include(r => r.Table)
            .Include(r => r.Order)
                .ThenInclude(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Translations)
            .ToListAsync();

        return mapper.Map<List<ReservationDTO>>(reservations);
    }
}