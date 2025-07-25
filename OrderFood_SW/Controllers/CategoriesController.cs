using Dapper;
using Microsoft.AspNetCore.Mvc;
using OrderFood_SW.Helper;
using OrderFood_SW.Models;

namespace OrderFood_SW.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly DatabaseHelper _db;

        public CategoriesController(DatabaseHelper db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(String keyword = "", int page = 1)
        {
            int pageSize = 8;
            int offset = (page - 1) * pageSize;

            string sql = @"
                    SELECT * FROM Categories
                    WHERE (@Keyword = '' OR 
                           CAST(CategoryName AS NVARCHAR) LIKE '%' + @Keyword + '%' OR 
                           CategoryDescription LIKE '%' + @Keyword + '%')
                    ORDER BY CategoryId
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                    SELECT COUNT(*) FROM Categories
                    WHERE (@Keyword = '' OR 
                           CAST(CategoryName AS NVARCHAR) LIKE '%' + @Keyword + '%' OR 
                           CategoryDescription LIKE '%' + @Keyword + '%');
                ";

            using (var connection = DatabaseHelper.GetConnection())
            {
                using (var multi = await connection.QueryMultipleAsync(sql, new { Keyword = keyword, Offset = offset, PageSize = pageSize }))
                {
                    var Categories = (await multi.ReadAsync<Categories>()).ToList();
                    int totalRows = await multi.ReadFirstAsync<int>();

                    ViewBag.TotalPages = (int)Math.Ceiling((double)totalRows / pageSize);
                    ViewBag.CurrentPage = page;
                    ViewBag.Keyword = keyword;

                    return View(Categories);
                }
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Categories table)
        {
            if (!ModelState.IsValid)
                return View(table);

            var sql = @"INSERT INTO Categories (CategoryName, CategoryDescription)
                        VALUES (@CategoryName, @CategoryDescription)";
            using var conn = _db.CreateConnection();
            await conn.ExecuteAsync(sql, table);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var sql = "SELECT * FROM Categories WHERE CategoryId = @id";
            using var conn = _db.CreateConnection();
            var table = await conn.QuerySingleOrDefaultAsync<Categories>(sql, new { id });

            if (table == null) return NotFound();
            return View(table);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Categories table)
        {
            if (!ModelState.IsValid)
                return View(table);

            var sql = @"UPDATE Categories SET CategoryName = @CategoryName, CategoryDescription = @CategoryDescription
                        WHERE CategoryId = @CategoryId";

            using var conn = _db.CreateConnection();
            await conn.ExecuteAsync(sql, table);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var sql = "SELECT * FROM Categories WHERE CategoryId = @id";
            using var conn = _db.CreateConnection();
            var table = await conn.QuerySingleOrDefaultAsync<Categories>(sql, new { id });

            if (table == null) return NotFound();
            return View(table);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var sql = "SELECT * FROM Categories WHERE CategoryId = @id";
            using var conn = _db.CreateConnection();
            var table = await conn.QuerySingleOrDefaultAsync<Categories>(sql, new { id });

            if (table == null) return NotFound();
            return View(table);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sql = "DELETE FROM Categories WHERE CategoryId = @id";
            using var conn = _db.CreateConnection();
            await conn.ExecuteAsync(sql, new { id });

            return RedirectToAction(nameof(Index));
        }
    }
}
