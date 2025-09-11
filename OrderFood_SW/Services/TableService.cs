using OrderFood_SW.Models;
using OrderFood_SW.Repositories;

namespace OrderFood_SW.Services
{
    public class TableService
    {
        private readonly TableRepository _repo;

        public TableService(TableRepository repo)
        {
            _repo = repo;
        }

        public Task<(List<Table> Tables, int TotalRows)> GetPagedAsync(string keyword, int page, int pageSize) =>
            _repo.GetPagedAsync(keyword, page, pageSize);

        public async Task<Table?> GetByIdAsync(int id) =>
            await _repo.GetByIdAsync(id);

        public Task AddAsync(Table table) =>
            _repo.AddAsync(table);

        public Task UpdateAsync(Table table) =>
            _repo.UpdateAsync(table);

        public async Task DeleteAsync(int id)
        {
            var table = await _repo.GetByIdAsync(id);
            if (table != null)
            {
                await _repo.DeleteAsync(table);
            }
        }
    }
}
