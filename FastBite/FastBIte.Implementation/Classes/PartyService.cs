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
        var allKeys = _redis.Multiplexer.GetServer(_redis.Multiplexer.GetEndPoints()[0]).Keys();
        foreach (var key in allKeys)
        {
            var strKey = key.ToString();
            if (Guid.TryParse(strKey, out var parsedGuid))
            {
                var partyData = await _redisService.GetAsync<PartyDTO>(parsedGuid);
                if (partyData != null && partyData.TableId == tableId)
                {
                    throw new Exception($"Table {tableId} already has an active party");
                }
            }
        }

        var partyId = Guid.NewGuid();
        var newPartyData = new PartyDTO
        {
            PartyId = partyId,
            TableId = tableId,
            OwnerId = ownerId,
            MemberIds = new List<Guid> { ownerId }
        };

        var jsonData = JsonSerializer.Serialize(newPartyData);
        await _redis.StringSetAsync(partyId.ToString(), jsonData); // ✅ Ключ с дефисами

        return partyId;
    }

    public async Task<string> JoinPartyAsync(string partyCode, Guid userId)
    {
        var keys = _redis.Multiplexer.GetServer(_redis.Multiplexer.GetEndPoints()[0]).Keys();
        var matchingKey = keys.FirstOrDefault(key => key.ToString().Contains(partyCode));

        if (matchingKey.ToString() == null)
            throw new Exception("Invalid party code");

        if (!Guid.TryParse(matchingKey.ToString(), out var partyId))
            throw new Exception("Party ID format is invalid");

        var partyData = await _redisService.GetAsync<PartyDTO>(partyId);
        if (partyData == null)
            throw new Exception("Party not found");

        if (partyData.MemberIds.Contains(userId))
            throw new Exception("You are already in this party");

        partyData.MemberIds.Add(userId);
        await _redis.StringSetAsync(partyId.ToString(), JsonSerializer.Serialize(partyData));

        return partyCode;
    }
    
    public async Task<bool> LeavePartyAsync(Guid partyId, Guid userId)
    {
        var exists = await _redis.KeyExistsAsync(partyId.ToString());
        Console.WriteLine($"Key exists before retrieval: {exists}");

        if (!exists) 
        {
            Console.WriteLine($"Party with ID {partyId} does not exist in Redis.");
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
        var party = await _redisService.GetAsync<PartyDTO>(partyId);
        if (party == null)
        {
            return null;
        }

        var cartKey = $"party_cart:{partyId}";
        var productIds = await _redis.ListRangeAsync(cartKey);

        party.OrderItems = productIds.Select(id => Guid.Parse(id.ToString())).ToList();

        return party;
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
    
    public async Task RemoveProductFromPartyCartAsync(Guid partyId, Guid productId)
    {
        var cartKey = $"party_cart:{partyId}";

        var exists = await _redis.KeyExistsAsync(cartKey);
        if (!exists)
        {
            throw new Exception("Party cart not found.");
        }

        await _redis.ListRemoveAsync(cartKey, productId.ToString(), 1);

        var remainingItems = await _redis.ListLengthAsync(cartKey);
        if (remainingItems > 0)
        {
            await _redis.KeyExpireAsync(cartKey, TimeSpan.FromMinutes(15));
        }
    }

    public async Task ClearPartyCartAsync(Guid partyId)
    {
        var cartKey = $"party_cart:{partyId}";

        await _redis.KeyDeleteAsync(cartKey);
    }
}