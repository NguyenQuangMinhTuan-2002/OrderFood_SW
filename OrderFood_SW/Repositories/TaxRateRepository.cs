using Microsoft.EntityFrameworkCore;
using OrderFood_SW.Helper;
using OrderFood_SW.Models;

namespace OrderFood_SW.Repositories
{
    public class TaxRateRepository
    {
        private readonly DatabaseHelperEF _db;

        public TaxRateRepository(DatabaseHelperEF db)
        {
            _db = db;
        }

        public List<TaxRate> GetAllTaxRates()
        {
            return _db.TaxRates
                .AsNoTracking()
                .OrderByDescending(tr => tr.CreatedDate)
                .ToList();
        }

        public TaxRate? GetActiveTaxRate()
        {
            return _db.TaxRates
                .AsNoTracking()
                .FirstOrDefault(tr => tr.IsActive);
        }

        public TaxRate? GetById(int id)
        {
            return _db.TaxRates.AsNoTracking().FirstOrDefault(tr => tr.Id == id);
        }

        public void Add(TaxRate taxRate)
        {
            _db.TaxRates.Add(taxRate);
        }

        public void Update(TaxRate taxRate)
        {
            // Use AsNoTracking to avoid tracking conflicts
            var existingTaxRate = _db.TaxRates.AsNoTracking().FirstOrDefault(tr => tr.Id == taxRate.Id);
            if (existingTaxRate != null)
            {
                // Detach any existing tracked entities
                var trackedEntity = _db.ChangeTracker.Entries<TaxRate>()
                    .FirstOrDefault(e => e.Entity.Id == taxRate.Id);
                if (trackedEntity != null)
                {
                    trackedEntity.State = EntityState.Detached;
                }

                // Update the entity
                taxRate.UpdatedDate = DateTime.Now;
                _db.TaxRates.Update(taxRate);
            }
        }

        public void Delete(int id)
        {
            var taxRate = _db.TaxRates.FirstOrDefault(tr => tr.Id == id);
            if (taxRate != null)
            {
                _db.TaxRates.Remove(taxRate);
            }
        }

        public async Task SetActiveTaxRateAsync(int id)
        {
            // Deactivate all tax rates first using raw SQL to avoid tracking issues
            await _db.Database.ExecuteSqlRawAsync("UPDATE TaxRates SET IsActive = 0, UpdatedDate = GETDATE()");

            // Activate the selected one using raw SQL
            if (id > 0)
            {
                await _db.Database.ExecuteSqlRawAsync("UPDATE TaxRates SET IsActive = 1, UpdatedDate = GETDATE() WHERE Id = {0}", id);
            }
        }

        public void SaveChanges()
        {
            _db.SaveChanges();
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
