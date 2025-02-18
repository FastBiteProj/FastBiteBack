using AutoMapper;
using FastBite.Data.Configs;
using FastBite.Data.Contexts;
using FastBite.Data.DTOS;
using FastBite.Data.Models;
using FastBite.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FastBite.Services.Classes;

public class TableService : ITableService
{
    private readonly FastBiteContext _context;
    private readonly IMapper _mapper;

    public TableService(FastBiteContext context)
    {
        _context = context;
        _mapper = MappingConfiguration.InitializeConfig();
    }

    public async Task<TableDTO> CreateTableAsync(TableDTO tableDto)
    {
        var table = new Table
        {
            Number = tableDto.TableNumber,
            Capacity = tableDto.TableCapacity
        };

        _context.Tables.Add(table);
        await _context.SaveChangesAsync();

        return _mapper.Map<TableDTO>(table);
    }

    public async Task<List<TableDTO>> GetAllTablesAsync(string dateString)
    {
        if (!DateTime.TryParseExact(dateString, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var date))
        {
            throw new ArgumentException("Invalid date format. Use yyyy-MM-dd.");
        }

        var tables = await _context.Tables
            .Include(t => t.Reservations)
            .ToListAsync();

        var result = tables.Select(t => new TableDTO(
            t.Number,
            t.Capacity,
            t.Reservations
                .Where(r => r.ReservationDate.ToString("yyyy-MM-dd") == date.ToString("yyyy-MM-dd"))
                .Select(r => _mapper.Map<ReservationDTO>(r)) 
                .ToList() 
        )).ToList();

        return result;
    }
    public async Task<TableDTO> EditTableAsync(int TableNumber, TableDTO tableDto)
    {
        var table = await _context.Tables.FirstOrDefaultAsync(t => t.Number == TableNumber);
        if (table == null) throw new Exception("Table not found");

        table.Number = tableDto.TableNumber;
        table.Capacity = tableDto.TableCapacity;

        _context.Tables.Update(table);
        await _context.SaveChangesAsync();

        return _mapper.Map<TableDTO>(table);
    }

    public async Task DeleteTableAsync(int TableNumber)
    {
        var table = await _context.Tables.FirstOrDefaultAsync(t => t.Number == TableNumber);
        if (table == null) throw new Exception("Table not found");

        _context.Tables.Remove(table);
        await _context.SaveChangesAsync();
    }
}