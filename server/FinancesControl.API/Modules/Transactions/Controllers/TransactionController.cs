using System.Security.Claims;
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

		private string? GetUserId()
		{
			return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		}

		[HttpPost]
		public async Task<IActionResult> Create(CreateTransactionRequest request)
		{
			var userId = GetUserId();
			var id = _service.CreateTransaction(request, userId);
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

		[HttpPatch("{transactionId}")]
		public async Task<IActionResult> Update(long transactionId, UpdateTransactionRequest request)
		{
			var userId = GetUserId();
			var updated = _service.Update(transactionId, request, userId);
			if (!updated) return NotFound();
			return NoContent();
		}
	}
}
