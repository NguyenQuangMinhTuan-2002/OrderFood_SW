using Dapper;
using Microsoft.AspNetCore.Mvc;
using OrderFood_SW.Helper;
using OrderFood_SW.Models;

namespace OrderFood_SW.Controllers
{
    public class DishesController : Controller
    {
        private readonly DatabaseHelper _db;

        public DishesController(DatabaseHelper db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(String keyword = "", int page = 1)
        {
            int pageSize = 8;
            int offset = (page - 1) * pageSize;

            string sql = @"
                SELECT * FROM Dishes
                WHERE (@Keyword = '' OR 
                       CAST(DishPrice AS NVARCHAR) LIKE '%' + @Keyword + '%' OR 
                       DishName LIKE '%' + @Keyword + '%' OR
                       DishDescription LIKE '%' + @Keyword + '%')
                ORDER BY DishId
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT COUNT(*) FROM Dishes
                WHERE (@Keyword = '' OR 
                       CAST(DishPrice AS NVARCHAR) LIKE '%' + @Keyword + '%' OR 
                       DishName LIKE '%' + @Keyword + '%' OR
                       DishDescription LIKE '%' + @Keyword + '%');
            ";

            using (var connection = DatabaseHelper.GetConnection())
            {
                using (var multi = await connection.QueryMultipleAsync(sql, new { Keyword = keyword, Offset = offset, PageSize = pageSize }))
                {
                    var Dishes = (await multi.ReadAsync<Dish>()).ToList();
                    int totalRows = await multi.ReadFirstAsync<int>();

                    ViewBag.TotalPages = (int)Math.Ceiling((double)totalRows / pageSize);
                    ViewBag.CurrentPage = page;
                    ViewBag.Keyword = keyword;

                    return View(Dishes);
                }
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Dish table)
        {
            if (!ModelState.IsValid)
                return View(table);

            var sql = @"INSERT INTO Dishes (DishName, DishDescription, DishPrice, ImageUrl, CategoryId, IsAvailable)
                        VALUES (@DishName, @DishDescription, @DishPrice, @ImageUrl, @CategoryId, @IsAvailable)";
            using var conn = _db.CreateConnection();
            await conn.ExecuteAsync(sql, table);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var sql = "SELECT * FROM Dishes WHERE DishId = @id";
            using var conn = _db.CreateConnection();
            var table = await conn.QuerySingleOrDefaultAsync<Dish>(sql, new { id });

            if (table == null) return NotFound();
            return View(table);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Dish table)
        {
            if (!ModelState.IsValid)
                return View(table);

            var sql = @"UPDATE Dishes SET DishName = @DishName, DishDescription = @DishDescription,
                        DishPrice = @DishPrice, ImageUrl = @ImageUrl, CategoryId = @CategoryId, IsAvailable = @IsAvailable
                        WHERE DishId = @DishId";

            using var conn = _db.CreateConnection();
            await conn.ExecuteAsync(sql, table);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var sql = "SELECT * FROM Dishes WHERE DishId = @id";
            using var conn = _db.CreateConnection();
            var table = await conn.QuerySingleOrDefaultAsync<Dish>(sql, new { id });

            if (table == null) return NotFound();
            return View(table);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var sql = "SELECT * FROM Dishes WHERE DishId = @id";
            using var conn = _db.CreateConnection();
            var table = await conn.QuerySingleOrDefaultAsync<Dish>(sql, new { id });

            if (table == null) return NotFound();
            return View(table);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sql = "DELETE FROM Dishes WHERE DishId = @id";
            using var conn = _db.CreateConnection();
            await conn.ExecuteAsync(sql, new { id });

            return RedirectToAction(nameof(Index));
        }
    }
}
