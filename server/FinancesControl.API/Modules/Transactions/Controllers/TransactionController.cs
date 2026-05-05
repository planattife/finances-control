using FinancesControl.API.Transactions.DTOs;
using FinancesControl.API.Transactions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancesControl.API.Transactions.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class TransactionController : ControllerBase
	{
		private readonly ITransactionService _service;

		public TransactionController(ITransactionService service)
		{
			_service = service;
		}

		[HttpPost]
		public async Task<IActionResult> Create(CreateTransactionRequest request)
		{
			var id = _service.CreateTransaction(request);
			return CreatedAtAction(nameof(GetById), new { transactionId = id }, null);
		}

		[HttpGet("{transactionId}")]
		public async Task<ActionResult<TransactionDetailResponse>> GetById(long transactionId)
		{
			var resp = _service.GetById(transactionId);
			if (resp == null) return NotFound();
			return Ok(resp);
		}

		[HttpGet]
		public async Task<ActionResult<List<TransactionListItemResponse>>> Get()
		{
			var resp = _service.GetAll();
			return Ok(resp);
		}

		[HttpDelete("{transactionId}")]
		public async Task<IActionResult> Delete(long transactionId)
		{
			var deleted = _service.Delete(transactionId);
			if (!deleted) return NotFound();
			return NoContent();
		}

		[HttpPatch("{transactionId}/amount")]
		public async Task<IActionResult> UpdateAmount(long transactionId, UpdateTransactionAmountRequest request)
		{
			var updated = _service.UpdateAmount(transactionId, request);
			if (!updated) return NotFound();
			return NoContent();
		}

		[HttpPatch("{transactionId}/category")]
		public async Task<IActionResult> UpdateCategory(long transactionId, UpdateTransactionCategoryRequest request)
		{
			var updated = _service.UpdateCategory(transactionId, request);
			if (!updated) return NotFound();
			return NoContent();
		}
	}
}
