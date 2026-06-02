using System.Security.Claims;
using FinancesControl.API.Transactions.DTOs;
using FinancesControl.API.Transactions.Services;
using FinancesControl.API.Common;
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

		[HttpPost("{accountId}")]
		public async Task<IActionResult> Create(long accountId, CreateTransactionRequest request)
		{
			try
			{
				var userId = GetUserId();
				var id = await _service.CreateTransaction(request, accountId, userId);
				return CreatedAtAction(nameof(GetById), new { accountId = accountId, transactionId = id }, null);
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ex.Message);
			}
		}

		[HttpGet("{accountId}/{transactionId}")]
		public async Task<ActionResult<SingleResponse<TransactionDetailResponse>>> GetById(long accountId, long transactionId)
		{
			var userId = GetUserId();
			var resp = await _service.GetById(transactionId, accountId, userId);
			return StatusCode(resp.HttpStatusCode, resp);
		}

		[HttpGet("{accountId}")]
		public async Task<ActionResult<PaginatedResponse<TransactionListItemResponse>>> Get(long accountId)
		{
			var userId = GetUserId();
			var resp = await _service.GetAll(accountId, userId);
			return Ok(resp);
		}

		[HttpDelete("{accountId}/{transactionId}")]
		public async Task<IActionResult> Delete(long accountId, long transactionId)
		{
			var userId = GetUserId();
			var deleted = await _service.Delete(transactionId, accountId, userId);
			if (!deleted) return NotFound();
			return NoContent();
		}

		[HttpPatch("{accountId}/{transactionId}")]
		public async Task<IActionResult> Update(long accountId, long transactionId, UpdateTransactionRequest request)
		{
			var userId = GetUserId();
			var updated = await _service.Update(transactionId, accountId, request, userId);
			if (!updated) return NotFound();
			return NoContent();
		}
	}
}
