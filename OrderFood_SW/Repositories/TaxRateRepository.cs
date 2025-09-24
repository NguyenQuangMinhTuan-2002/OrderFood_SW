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
                .OrderByDescending(tr => tr.CreatedDate)
                .ToList();
        }

        public TaxRate? GetActiveTaxRate()
        {
            return _db.TaxRates
                .FirstOrDefault(tr => tr.IsActive);
        }

        public TaxRate? GetById(int id)
        {
            return _db.TaxRates.FirstOrDefault(tr => tr.Id == id);
        }

        public void Add(TaxRate taxRate)
        {
            _db.TaxRates.Add(taxRate);
        }

        public void Update(TaxRate taxRate)
        {
            taxRate.UpdatedDate = DateTime.Now;
            _db.TaxRates.Update(taxRate);
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
            // Deactivate all tax rates first
            var allTaxRates = await _db.TaxRates.ToListAsync();
            foreach (var tr in allTaxRates)
            {
                tr.IsActive = false;
            }

            // Activate the selected one
            var selectedTaxRate = await _db.TaxRates.FirstOrDefaultAsync(tr => tr.Id == id);
            if (selectedTaxRate != null)
            {
                selectedTaxRate.IsActive = true;
                selectedTaxRate.UpdatedDate = DateTime.Now;
            }

            await _db.SaveChangesAsync();
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
