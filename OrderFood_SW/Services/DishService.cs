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

        public async Task<string?> SaveImageAsync(IFormFile? imageFile)
        {
            if (imageFile == null || imageFile.Length == 0) return null;

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Products");
            Directory.CreateDirectory(uploadFolder);
            string filePath = Path.Combine(uploadFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return fileName;
        }
    }
}
