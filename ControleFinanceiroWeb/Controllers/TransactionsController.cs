using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ControleFinanceiroWeb.Services.Transactions;
using ControleFinanceiroWeb.Services.Categories;
using ControleFinanceiroWeb.Models.ViewModels;

namespace ControleFinanceiroWeb.Controllers
{
    // Controller responsible for managing account transactions and Excel copy-paste imports.
    public class TransactionsController : Controller
    {
        private readonly ITransactionService _transactionService;
        private readonly ICategoryIdentificationService _categoryIdentificationService;

        public TransactionsController(ITransactionService transactionService, ICategoryIdentificationService categoryIdentificationService)
        {
            _transactionService = transactionService;
            _categoryIdentificationService = categoryIdentificationService;
        }

        // Displays the main transaction list page filtered by account and date range.
        public async Task<IActionResult> Index(string? statementType, int? statementTypeId, DateTime? startDate, DateTime? endDate)
        {
            int id = statementTypeId ?? 0;
        
            DateTime start = startDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DateTime end = endDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));

            ViewBag.CurrentStatementType = string.IsNullOrEmpty(statementType) ? "Extrato" : statementType;
            ViewBag.CurrentStatementTypeId = id;
            ViewBag.StartDate = start.ToString("yyyy-MM-dd");
            ViewBag.EndDate = end.ToString("yyyy-MM-dd");

            var viewModel = await _transactionService.GetTransactionsAsync(id, start, end);

            return View(viewModel);
        }

        // Returns the insertion/editing transaction form partial view.
        [HttpGet]
        public async Task<IActionResult> GetForm(int id, int statementTypeId)
        {
            var model = await _transactionService.GetTransactionFormAsync(id, statementTypeId);
        
            return PartialView("_TransactionForm", model);
        }

        // Saves or updates a transaction.
        [HttpPost]
        public async Task<IActionResult> Save(TransactionFormViewModel model)
        {
            var result = await _transactionService.SaveTransactionAsync(model);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        // Deletes a transaction by id.
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _transactionService.DeleteTransactionAsync(id);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        // Triggers automatic category identification for transactions in the specified date range.
        [HttpPost]
        public async Task<IActionResult> Identify(int statementTypeId, string startDate, string endDate)
        {
            var start = Helpers.ConversionHelper.ToNullableDateTime(startDate) ?? DateTime.Today;
            var end = Helpers.ConversionHelper.ToNullableDateTime(endDate) ?? DateTime.Today;

            var result = await _categoryIdentificationService.IdentifyTransactionsAsync(statementTypeId, start, end);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        // Parses raw clipboard data and returns a structured preview list.
        [HttpPost]
        public async Task<IActionResult> PreviewImport(string rawText)
        {
            var result = await _transactionService.PreviewImportAsync(rawText);

            return Ok(new { success = true, items = result });
        }

        // Saves a list of imported transactions in bulk.
        [HttpPost]
        public async Task<IActionResult> SaveImport([FromBody] List<TransactionImportSaveModel> items, int statementTypeId)
        {
            var result = await _transactionService.SaveImportAsync(items, statementTypeId);

            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}