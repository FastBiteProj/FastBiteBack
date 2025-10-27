using FastBite.Infrastructure.Contexts;
using FastBite.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastBite.Presentation.Controllers;
[ApiController]
[Route("api/v1/[controller]")]
public class RestaurantController : ControllerBase
{
    private readonly FastBiteContext _context;

    public RestaurantController(FastBiteContext context) {
        _context = context;
    }

    [HttpGet("GetRestaurants")]
    public async Task<ActionResult<IEnumerable<Restaurant>>> GetRestaurants() {
        return await _context.Restaurants.ToListAsync();
    }

    [HttpPost("AddRestaurant")]
    public async Task<ActionResult<Restaurant>> CreateRestaurant([FromBody] Restaurant restaurant) {
        if (restaurant is null)
        {
            
            throw new ArgumentNullException(nameof(restaurant));
        }
        
        await _context.SaveChangesAsync();
        return Ok(restaurant);
    }

}