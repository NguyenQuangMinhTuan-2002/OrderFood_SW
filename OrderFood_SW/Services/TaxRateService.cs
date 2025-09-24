using OrderFood_SW.Models;
using OrderFood_SW.Repositories;

namespace OrderFood_SW.Services
{
    public class TaxRateService
    {
        private readonly TaxRateRepository _repo;

        public TaxRateService(TaxRateRepository repo)
        {
            _repo = repo;
        }

        public List<TaxRate> GetAllTaxRates()
        {
            return _repo.GetAllTaxRates();
        }

        public TaxRate? GetActiveTaxRate()
        {
            return _repo.GetActiveTaxRate();
        }

        public decimal GetCurrentTaxRate()
        {
            var activeTaxRate = _repo.GetActiveTaxRate();
            return activeTaxRate?.Rate ?? 0.1m; // Default to 10% if no active rate found
        }

        public TaxRate? GetById(int id)
        {
            return _repo.GetById(id);
        }

        public async Task<(bool Success, string Message)> CreateTaxRateAsync(TaxRate taxRate)
        {
            try
            {
                // If this is set as active, deactivate others first
                if (taxRate.IsActive)
                {
                    await _repo.SetActiveTaxRateAsync(0); // This will deactivate all
                }

                _repo.Add(taxRate);
                _repo.SaveChanges();
                return (true, "Tax rate created successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Error creating tax rate: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> UpdateTaxRateAsync(TaxRate taxRate)
        {
            try
            {
                var existingTaxRate = _repo.GetById(taxRate.Id);
                if (existingTaxRate == null)
                    return (false, "Tax rate not found.");

                // If this is set as active, deactivate others first
                if (taxRate.IsActive)
                {
                    await _repo.SetActiveTaxRateAsync(0); // This will deactivate all
                }

                _repo.Update(taxRate);
                _repo.SaveChanges();
                return (true, "Tax rate updated successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating tax rate: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteTaxRateAsync(int id)
        {
            try
            {
                var taxRate = _repo.GetById(id);
                if (taxRate == null)
                    return (false, "Tax rate not found.");

                if (taxRate.IsActive)
                    return (false, "Cannot delete active tax rate. Please activate another rate first.");

                _repo.Delete(id);
                _repo.SaveChanges();
                return (true, "Tax rate deleted successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting tax rate: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> SetActiveTaxRateAsync(int id)
        {
            try
            {
                var taxRate = _repo.GetById(id);
                if (taxRate == null)
                    return (false, "Tax rate not found.");

                await _repo.SetActiveTaxRateAsync(id);
                return (true, "Tax rate activated successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Error activating tax rate: {ex.Message}");
            }
        }
    }
}
