using AutoMapper;
using FastBite.Implementation.Configs;
using FastBite.Infrastructure.Contexts;
using FastBite.Shared.DTOS;
using FastBite.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FastBite.Implementation.Classes;

public class RestaurantService
{
    public FastBiteContext _context;
    public readonly IMapper _mapper;

    public RestaurantService(FastBiteContext context)
    {
        _context = context;
        _mapper = MappingConfiguration.InitializeConfig();
    }
    //
    // public async Task<List<RestaurantDTO>> GetRestaurantsAsync()
    // {
    //     var restaurants = await _context.Restaurants
    //         .Include(r => r.MenuItems)
    //         .ToListAsync();
    //
    //     return _mapper.Map<List<RestaurantDTO>>(restaurants);
    // }

    // public async Task<RestaurantDTO> CreateRestaurantAsync(RestaurantDTO restaurant)
    // {
    //     var newRestaurant = new Restaurant()
    //     {
    //         Name = restaurant.Name,
    //         Address = restaurant.Address,
    //     };
        
    //     _context.Restaurants.Add(newRestaurant);
    //     await _context.SaveChangesAsync();
        
    //     return _mapper.Map<RestaurantDTO>(newRestaurant);
    // }
}