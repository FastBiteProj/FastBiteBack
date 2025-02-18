using FastBite.Shared.DTOS;

namespace FastBite.Core.Interfaces;

public interface ITableService
{
    Task<TableDTO> CreateTableAsync(TableDTO tableDto);
    Task<List<TableDTO>> GetAllTablesAsync(string dateString);
    Task<TableDTO> EditTableAsync(int TableNumber, TableDTO tableDto);
    Task DeleteTableAsync(int TableNumber);
}