using System.Text.Json;
using AutoMapper;
using FastBite.Core.Interfaces;
using FastBite.Implementation.Configs;
using FastBite.Infrastructure.Contexts;
using FastBite.Shared.DTOS;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace FastBite.Implementation.Classes;

public class PartyService : IPartyService
{
    private readonly IDatabase _redis;
    private FastBiteContext _context;
    private IMapper _mapper;
    public IRedisService _redisService;

    public PartyService(IConnectionMultiplexer redis, IRedisService redisService, FastBiteContext context)
    {
        _redis = redis.GetDatabase();
        _redisService = redisService;
        _context = context;
        _mapper = MappingConfiguration.InitializeConfig();
    }

    public async Task<Guid> CreatePartyAsync(Guid ownerId, int tableId)
    {
        Guid partyId = Guid.NewGuid();
        var partyData = new PartyDTO
        {
            PartyId = partyId,
            TableId = tableId,
            OwnerId = ownerId,
            MemberIds = new List<Guid> { ownerId }
        };

        string key = $"{partyId}";
        string jsonData = JsonSerializer.Serialize(partyData);
    
        await _redis.StringSetAsync(key, jsonData);
        
        return partyId;
    }

    public async Task<string> JoinPartyAsync(string partyCode, Guid userId)
    {
        var keys = _redis.Multiplexer.GetServer(_redis.Multiplexer.GetEndPoints()[0]).Keys();
        var matchingKey = keys.FirstOrDefault(key => key.ToString().Contains(partyCode));

        var partyId = Guid.Parse(matchingKey.ToString());

        var partyData = await _redisService.GetAsync<PartyDTO>(partyId);
        if (partyData == null)
        {
            throw new Exception("Party not found");
        }

        if (partyData.MemberIds.Contains(userId))
        {
            throw new Exception("You cannot join this party, You already there"); 
        }

        partyData.MemberIds.Add(userId);

        string jsonData = JsonSerializer.Serialize(partyData);
        await _redis.StringSetAsync(partyId.ToString(), jsonData);

        return partyCode;
    }
    
    public async Task<bool> LeavePartyAsync(Guid partyId, Guid userId)
    {
        var exists = await _redis.KeyExistsAsync(partyId.ToString());
        Console.WriteLine($"Key exists before retrieval: {exists}");

        if (!exists) 
        {
            Console.WriteLine($"Party with ID {partyId} does not exist in Redis.[[__[_[_[__[_[_[_[_[");
            return false;
        }

        var partyData = await _redisService.GetAsync<PartyDTO>(partyId);
        if (partyData == null)
        {
            Console.WriteLine($"Party with ID {partyId} not found in Redis.");
            return false;
        }

        partyData.MemberIds.Remove(userId);

        if (partyData.MemberIds.Count == 0)
        {
            await _redis.KeyDeleteAsync(partyId.ToString());
            Console.WriteLine($"Party {partyId} deleted from Redis.");
        }
        else
        {
            string jsonData = JsonSerializer.Serialize(partyData);
            await _redis.StringSetAsync(partyId.ToString(), jsonData);
            Console.WriteLine($"Updated party {partyId} in Redis.");
        }

        return true;
    }
    
    public async Task<PartyDTO?> GetPartyAsync(Guid partyId)
    {
        return await _redisService.GetAsync<PartyDTO>(partyId);
    }
    
    public async Task AddProductToPartyCartAsync(Guid partyId, Guid productId)
    {
        var cartKey = $"party_cart:{partyId}";
        await _redis.ListLeftPushAsync(cartKey, productId.ToString());
        await _redis.KeyExpireAsync(cartKey, TimeSpan.FromMinutes(15));
    }

    public async Task<List<ProductDTO>> GetPartyCartAsync(Guid partyId)
    {
        var cartKey = $"party_cart:{partyId}";
        var productIds = await _redis.ListRangeAsync(cartKey);

        var products = new List<ProductDTO>();
        foreach (var productId in productIds)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Translations)
                .FirstOrDefaultAsync(p => p.Id == Guid.Parse(productId));

            if (product != null)
            {
                products.Add(_mapper.Map<ProductDTO>(product));
            }
        }
        return products;
    }
}