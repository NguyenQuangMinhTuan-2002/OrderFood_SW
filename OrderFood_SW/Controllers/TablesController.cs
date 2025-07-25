using Dapper;
using Microsoft.AspNetCore.Mvc;
using OrderFood_SW.Helper;
using OrderFood_SW.Models;

namespace OrderFood_SW.Controllers
{
    public class TablesController : Controller
    {
        private readonly DatabaseHelper _db;

        public TablesController(DatabaseHelper db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(String keyword = "", int page = 1)
        {
            int pageSize = 8;
            int offset = (page - 1) * pageSize;

            string sql = @"
                SELECT * FROM Tables
                WHERE (@Keyword = '' OR 
                       CAST(TableNumber AS NVARCHAR) LIKE '%' + @Keyword + '%' OR 
                       Description LIKE '%' + @Keyword + '%')
                ORDER BY TableId
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT COUNT(*) FROM Tables
                WHERE (@Keyword = '' OR 
                       CAST(TableNumber AS NVARCHAR) LIKE '%' + @Keyword + '%' OR 
                       Description LIKE '%' + @Keyword + '%');
            ";

            using (var connection = DatabaseHelper.GetConnection())
            {
                using (var multi = await connection.QueryMultipleAsync(sql, new { Keyword = keyword, Offset = offset, PageSize = pageSize }))
                {
                    var tables = (await multi.ReadAsync<Table>()).ToList();
                    int totalRows = await multi.ReadFirstAsync<int>();

                    ViewBag.TotalPages = (int)Math.Ceiling((double)totalRows / pageSize);
                    ViewBag.CurrentPage = page;
                    ViewBag.Keyword = keyword;

                    return View(tables);
                }
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Table table)
        {
            if (!ModelState.IsValid)
                return View(table);

            var sql = @"INSERT INTO Tables (TableNumber, QRCode, Status, Description)
                        VALUES (@TableNumber, @QRCode, @Status, @Description)";
            using var conn = _db.CreateConnection();
            await conn.ExecuteAsync(sql, table);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var sql = "SELECT * FROM Tables WHERE TableId = @id";
            using var conn = _db.CreateConnection();
            var table = await conn.QuerySingleOrDefaultAsync<Table>(sql, new { id });

            if (table == null) return NotFound();
            return View(table);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Table table)
        {
            if (!ModelState.IsValid)
                return View(table);

            var sql = @"UPDATE Tables SET TableNumber = @TableNumber, QRCode = @QRCode,
                        Status = @Status, Description = @Description
                        WHERE TableId = @TableId";

            using var conn = _db.CreateConnection();
            await conn.ExecuteAsync(sql, table);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var sql = "SELECT * FROM Tables WHERE TableId = @id";
            using var conn = _db.CreateConnection();
            var table = await conn.QuerySingleOrDefaultAsync<Table>(sql, new { id });

            if (table == null) return NotFound();
            return View(table);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var sql = "SELECT * FROM Tables WHERE TableId = @id";
            using var conn = _db.CreateConnection();
            var table = await conn.QuerySingleOrDefaultAsync<Table>(sql, new { id });

            if (table == null) return NotFound();
            return View(table);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sql = "DELETE FROM Tables WHERE TableId = @id";
            using var conn = _db.CreateConnection();
            await conn.ExecuteAsync(sql, new { id });

            return RedirectToAction(nameof(Index));
        }
    }
}
