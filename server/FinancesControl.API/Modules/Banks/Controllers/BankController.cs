using System.Security.Claims;
using FinancesControl.API.Banks.DTOs;
using FinancesControl.API.Banks.Services;
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
            var id = _bankService.CreateBank(request);
            return CreatedAtAction(nameof(GetById), new { bankId = id }, null);
        }

        [HttpGet("{bankId}")]
        public async Task<ActionResult<BankDetailResponse>> GetById(long bankId)
        {
            var userId = GetCurrentUserId();
            var response = _bankService.GetById(bankId, userId);
            if (response == null)
                return NotFound();
            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult<List<BankListItemResponse>>> Get()
        {
            var userId = GetCurrentUserId();
            var response = _bankService.GetAll(userId);
            return Ok(response);
        }

        [HttpPut("{bankId}")]
        public async Task<IActionResult> Update(long bankId, UpdateBankRequest request)
        {
            var userId = GetCurrentUserId();
            var updated = _bankService.Update(bankId, request, userId);
            if (!updated)
                return NotFound();
            return NoContent();
        }

        [HttpPost("{bankId}/activate")]
        public async Task<IActionResult> Activate(long bankId)
        {
            var userId = GetCurrentUserId();
            var activated = _bankService.Activate(bankId, userId);
            if (!activated)
                return NotFound();
            return NoContent();
        }

        [HttpPost("{bankId}/deactivate")]
        public async Task<IActionResult> Deactivate(long bankId)
        {
            var userId = GetCurrentUserId();
            var deactivated = _bankService.Deactivate(bankId, userId);
            if (!deactivated)
                return NotFound();
            return NoContent();
        }
    }
}