using System.Security.Claims;
using FinancesControl.API.Banks.DTOs;
using FinancesControl.API.Banks.Services;
using FinancesControl.API.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancesControl.API.Banks.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BankController : ControllerBase
    {
        private readonly IBankService _bankService;

        public BankController(IBankService bankService)
        {
            _bankService = bankService;
        }

        private string GetCurrentUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userId ?? throw new InvalidOperationException("User context is not available");
        }

        [HttpPost]
        public async Task<IActionResult> CreateBank(CreateBankRequest request)
        {
            var id = await _bankService.CreateBank(request);
            return CreatedAtAction(nameof(GetById), new { bankId = id }, null);
        }

        [HttpGet("{bankId}")]
        public async Task<ActionResult<SingleResponse<BankDetailResponse>>> GetById(long bankId)
        {
            var userId = GetCurrentUserId();
            var response = await _bankService.GetById(bankId, userId);
            return StatusCode(response.HttpStatusCode, response);
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<BankListItemResponse>>> Get([FromQuery] PaginationParams paginationParams)
        {
            var userId = GetCurrentUserId();
            var response = await _bankService.GetAll(userId, paginationParams);
            return Ok(response);
        }

        [HttpPut("{bankId}")]
        public async Task<IActionResult> Update(long bankId, UpdateBankRequest request)
        {
            var userId = GetCurrentUserId();
            var updated = await _bankService.Update(bankId, request, userId);
            if (!updated)
                return NotFound();
            return NoContent();
        }

        [HttpPost("{bankId}/activate")]
        public async Task<IActionResult> Activate(long bankId)
        {
            var userId = GetCurrentUserId();
            var activated = await _bankService.Activate(bankId, userId);
            if (!activated)
                return NotFound();
            return NoContent();
        }

        [HttpPost("{bankId}/deactivate")]
        public async Task<IActionResult> Deactivate(long bankId)
        {
            var userId = GetCurrentUserId();
            var deactivated = await _bankService.Deactivate(bankId, userId);
            if (!deactivated)
                return NotFound();
            return NoContent();
        }
    }
}
