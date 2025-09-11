using OrderFood_SW.Helper;

namespace OrderFood_SW.Repositories
{
    public class CartRepository
    {
        private readonly DatabaseHelperEF _db;

        public CartRepository(DatabaseHelperEF db)
        {
            _db = db;
        }

        public int? GetTableNumberById(int? tableId)
        {
            if (tableId == null || tableId == 0) return null;

            return _db.Tables
                .Where(t => t.TableId == tableId)
                .Select(t => t.TableNumber)
                .FirstOrDefault();
        }
    }
}
