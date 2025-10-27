using FastBite.Shared.DTOS;
using FastBite.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FastBite.Presentation.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class TableController : ControllerBase
{
    private readonly ITableService _tableService;

    public TableController(ITableService tableService)
    {
        _tableService = tableService;
    }

    [HttpPost("Create")]
    public async Task<IActionResult> CreateTable([FromBody] TableDTO tableDto)
    {
        var table = await _tableService.CreateTableAsync(tableDto);
        return Ok(table);
    }

    [HttpGet("Get")]
    public async Task<IActionResult> GetAllTables([FromQuery] string dateString)
    {
        var tables = await _tableService.GetAllTablesAsync(dateString);
        return Ok(tables);
    }

    [HttpPut("Edit")]
    public async Task<IActionResult> EditTable([FromForm] int TableNumber, [FromBody] TableDTO tableDto)
    {
        var updatedTable = await _tableService.EditTableAsync(TableNumber, tableDto);
        return Ok(updatedTable);
    }

    [HttpDelete("Delete")]
    public async Task<IActionResult> DeleteTable([FromBody] int TableNumber)
    {
        await _tableService.DeleteTableAsync(TableNumber);
        return NoContent();
    }
}