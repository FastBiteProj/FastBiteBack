using FastBite.Core.Interfaces;
using FastBite.Shared.DTOS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FastBite.Presentation.Controllers;

[Route("api/v1/party")]
[ApiController]
public class PartyController : ControllerBase
{
    public readonly IPartyService _partyService;

    public PartyController(IPartyService partyService)
    {
        _partyService = partyService;
    }
    
    [Authorize]
    [HttpPost("createParty")]
    public async Task<IActionResult> CreateParty([FromBody] PartyRequestDTO partyRequest)
    {
        if (partyRequest.OwnerId == null || partyRequest.TableId <= 0)
        {
            return BadRequest("Owner id is required and cannot be empty or Table Id is invalid");
        }

        var partyId = await _partyService.CreatePartyAsync(partyRequest.OwnerId, partyRequest.TableId);
        
        return Ok(partyId);
    }


    [HttpPost("joinParty")]
    public async Task<IActionResult> JoinParty([FromBody] JoinPartyRequestDTO joinPartyRequest)
    {
        if (joinPartyRequest.PartyCode == null || joinPartyRequest.UserId == null)
        {
            return BadRequest("PartyId и UserId обязательны");
        }

        var result = await _partyService.JoinPartyAsync(joinPartyRequest.PartyCode, joinPartyRequest.UserId);
        if (result == null)
        {
            return BadRequest("Не удалось присоединиться к пати");
        }

        return Ok(result);
    }
    
    [HttpPost("leave")]
    public async Task<IActionResult> LeaveParty([FromBody] LeavePartyDTO leavePartyDto)
    {
        bool result = await _partyService.LeavePartyAsync(leavePartyDto.PartyId, leavePartyDto.UserId);
        return result ? Ok("Successfully left the party.") : BadRequest("Failed to leave the party.");
    }
    
    [HttpGet("getParty")]
    public async Task<IActionResult> GetParty([FromQuery] Guid partyId)
    {
        var party = await _partyService.GetPartyAsync(partyId);
        if (party == null)
        {
            return NotFound(new { message = "Пати не найдена" });
        }
        return Ok(party);
    }
    
    [HttpPost("addToPartyCart")]
    public async Task<IActionResult> AddToPartyCart([FromBody] AddToPartyCartRequestDTO request)
    {
        await _partyService.AddProductToPartyCartAsync(request.PartyId, request.ProductId);
        return Ok(new { message = "Продукт добавлен в общую корзину" });
    }

    [HttpGet("getPartyCart")]
    public async Task<IActionResult> GetPartyCart([FromQuery] Guid partyId)
    {
        var products = await _partyService.GetPartyCartAsync(partyId);
        return Ok(products);
    }
    [HttpPost("removeFromPartyCart")]
    public async Task<IActionResult> RemoveFromPartyCart([FromBody] RemoveFromPartyCartDTO request)
    {
        await _partyService.RemoveProductFromPartyCartAsync(request.PartyId, request.ProductId);
        return Ok(new { message = "Продукт удален из корзины пати" });
    }

    [HttpPost("clearPartyCart")]
    public async Task<IActionResult> ClearPartyCart([FromBody] ClearPartyCartDTO request)
    {
        await _partyService.ClearPartyCartAsync(request.PartyId);
        return Ok(new { message = "Корзина пати очищена" });
    }

}