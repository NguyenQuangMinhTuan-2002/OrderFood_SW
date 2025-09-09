using OrderFood_SW.Models;
using OrderFood_SW.Repositories;

namespace OrderFood_SW.Services
{
    public class CategoryService
    {
        private readonly CategoryRepository _repo;

        public CategoryService(CategoryRepository repo)
        {
            _repo = repo;
        }

        public async Task<(List<Category> Categories, int TotalRows)> GetPagedCategoriesAsync(
            string keyword, int page, int pageSize)
        {
            int skip = (page - 1) * pageSize;
            int totalRows = await _repo.CountAsync(keyword);
            var categories = await _repo.GetPagedAsync(keyword, skip, pageSize);
            return (categories, totalRows);
        }

        public async Task<Category?> GetByIdAsync(int id) => await _repo.GetByIdAsync(id);

        public async Task AddAsync(Category category) => await _repo.AddAsync(category);

        public async Task UpdateAsync(Category category) => await _repo.UpdateAsync(category);

        public async Task DeleteAsync(Category category) => await _repo.DeleteAsync(category);
    }
}
