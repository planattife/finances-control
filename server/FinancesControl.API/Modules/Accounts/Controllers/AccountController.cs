using System.Security.Claims;
using FinancesControl.API.Accounts.DTOs;
using FinancesControl.API.Accounts.Services;
using FinancesControl.API.Common;
using FinancesControl.API.Common.Antiforgery;
using FinancesControl.API.Common.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FinancesControl.API.Accounts.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost]
        [EnableRateLimiting(RateLimitPolicyNames.FinanceWrite)]
        [ValidateAntiforgeryToken]
        public async Task<IActionResult> CreateAccount(CreateAccountRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var id = await _accountService.CreateAccount(request, userId);
            return CreatedAtAction(nameof(GetById), new { accountId = id }, null);
        }

        [HttpGet("{accountId}")]
        public async Task<ActionResult<SingleResponse<AccountDetailResponse>>> GetById(long accountId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var response = await _accountService.GetById(accountId, userId);
            return StatusCode(response.HttpStatusCode, response);
        }

        [HttpGet("{accountId}/account-kpi")]
        public async Task<ActionResult<SingleResponse<AccountKpiResponse>>> GetAccountKpi(long accountId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var response = await _accountService.GetAccountKpi(accountId, userId);
            return StatusCode(response.HttpStatusCode, response);
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<AccountListItemResponse>>> Get([FromQuery] PaginationParams paginationParams)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var response = await _accountService.GetAll(userId, paginationParams);
            return Ok(response);
        }

        [HttpPut("{accountId}")]
        [EnableRateLimiting(RateLimitPolicyNames.FinanceWrite)]
        [ValidateAntiforgeryToken]
        public async Task<IActionResult> Update(long accountId, UpdateAccountRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var updated = await _accountService.Update(accountId, request, userId);
            if (!updated)
                return NotFound();
            return NoContent();
        }

        [HttpPost("{accountId}/activate")]
        [EnableRateLimiting(RateLimitPolicyNames.FinanceWrite)]
        [ValidateAntiforgeryToken]
        public async Task<IActionResult> Activate(long accountId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var activated = await _accountService.Activate(accountId, userId);
            if (!activated)
                return NotFound();
            return NoContent();
        }

        [HttpPost("{accountId}/deactivate")]
        [EnableRateLimiting(RateLimitPolicyNames.FinanceWrite)]
        [ValidateAntiforgeryToken]
        public async Task<IActionResult> Deactivate(long accountId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var deactivated = await _accountService.Deactivate(accountId, userId);
            if (!deactivated)
                return NotFound();
            return NoContent();
        }
    }
}

