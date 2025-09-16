using OrderFood_SW.Models;
using OrderFood_SW.Repositories;

namespace OrderFood_SW.Services
{
    public class DishService
    {
        private readonly DishRepository _repo;

        public DishService(DishRepository repo)
        {
            _repo = repo;
        }

        public async Task<(List<Dish>, int totalRows)> GetPagedAsync(string keyword, int page, int pageSize)
        {
            return await _repo.GetPagedAsync(keyword, page, pageSize);
        }

        public async Task<Dish?> GetByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task AddAsync(Dish dish)
        {
            await _repo.AddAsync(dish);
        }

        public async Task UpdateAsync(Dish dish)
        {
            await _repo.UpdateAsync(dish);
        }

        public async Task DeleteAsync(int id)
        {
            var dish = await _repo.GetByIdAsync(id);
            if (dish != null)
            {
                await _repo.DeleteAsync(dish);
            }
        }

        public async Task<string?> SaveImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Products");
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }

        public void DeleteImage(string? fileName)
        {
            if (string.IsNullOrEmpty(fileName) || fileName == "nophoto1.png") return;

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Products", fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
