using Microsoft.AspNetCore.Mvc;
using OrderFood_SW.Helper;
using OrderFood_SW.Models;
using OrderFood_SW.Services;

namespace OrderFood_SW.Controllers
{
    [AuthorizeRole("Admin", "Staff")]
    public class TaxRateController : Controller
    {
        private readonly TaxRateService _service;

        public TaxRateController(TaxRateService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            var taxRates = _service.GetAllTaxRates();
            return View(taxRates);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaxRate taxRate)
        {
            if (ModelState.IsValid)
            {
                var result = await _service.CreateTaxRateAsync(taxRate);
                if (result.Success)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = result.Message;
                }
            }
            return View(taxRate);
        }

        public IActionResult Edit(int id)
        {
            var taxRate = _service.GetById(id);
            if (taxRate == null)
            {
                return NotFound();
            }
            return View(taxRate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaxRate taxRate)
        {
            if (id != taxRate.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var result = await _service.UpdateTaxRateAsync(taxRate);
                if (result.Success)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = result.Message;
                }
            }
            return View(taxRate);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteTaxRateAsync(id);
            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> SetActive(int id)
        {
            var result = await _service.SetActiveTaxRateAsync(id);
            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult GetCurrentTaxRate()
        {
            var rate = _service.GetCurrentTaxRate();
            return Json(new { rate = rate });
        }
    }
}
