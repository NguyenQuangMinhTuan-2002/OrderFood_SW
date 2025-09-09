using Microsoft.EntityFrameworkCore;
using OrderFood_SW.Helper;
using OrderFood_SW.Models;

namespace OrderFood_SW.Repositories
{
    public class TableRepository
    {
        private readonly DatabaseHelperEF _db;

        public TableRepository(DatabaseHelperEF db)
        {
            _db = db;
        }

        public async Task<(List<Table> Tables, int TotalRows)> GetPagedAsync(string keyword, int page, int pageSize)
        {
            var query = _db.Tables.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(c =>
                    c.TableNumber.Equals(keyword) ||
                    c.Description.Contains(keyword));
            }

            int totalRows = await query.CountAsync();
            var tables = await query
                .OrderBy(c => c.TableId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (tables, totalRows);
        }

        public async Task<Table?> GetByIdAsync(int id) =>
            await _db.Tables.FindAsync(id);

        public async Task AddAsync(Table table)
        {
            _db.Tables.Add(table);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Table table)
        {
            _db.Tables.Update(table);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Table table)
        {
            _db.Tables.Remove(table);
            await _db.SaveChangesAsync();
        }

        public async Task<Table?> GetTableByIdAsync(int tableId) =>
        await _db.Tables.FirstOrDefaultAsync(t => t.TableId == tableId);
    }
}
