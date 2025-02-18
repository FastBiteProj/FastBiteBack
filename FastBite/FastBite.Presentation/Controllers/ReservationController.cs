using System.Security.Claims;
using FastBite.Infrastructure.Contexts;
using FastBite.Shared.DTOS;
using FastBite.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace FastBite.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ReservationController : ControllerBase {
    public FastBiteContext _context;
    public IReservationService _reservationService;


    public ReservationController(FastBiteContext context, IReservationService reservationService) {
        _context = context;
        _reservationService = reservationService;
    }


    [HttpGet("Get")]
    public async Task<IActionResult> GetAllReservations() {

        var token = HttpContext.Request.Cookies["accessToken"];
        token = token.ToString().Replace("Bearer ", "");
        
        var res = await _reservationService.GetAllReservationsAsync(token);

        return Ok(res);
    }

    [HttpPost("Create")]
    public async Task<IActionResult> CreateReservation([FromBody] ReservationDTO reservation) {
        
        if (reservation == null) {
            throw new ArgumentNullException(nameof(reservation));
        }
        
        var newReservation = await _reservationService.CreateReservationAsync(reservation);

        return Ok(newReservation);
    }

    [Authorize(Roles = "AppAdmin")]
    [HttpDelete("Delete")]
    public async Task<IActionResult> DeleteReservation(Guid Id) 
    {
        try 
        {
            await _reservationService.DeleteReservationAsync(Id);
            return Ok(new { message = "Reservation deleted successfully" });
        }
        catch (KeyNotFoundException ex) 
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex) 
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [Authorize(Roles = "AppAdmin")]
    [HttpPut("Edit")]
    public async Task<IActionResult> EditReservation(Guid Id, [FromBody] ReservationDTO reservation) {
        
        var res = await _reservationService.EditReservation(Id, reservation);
        
        return Ok(res);
    }
}