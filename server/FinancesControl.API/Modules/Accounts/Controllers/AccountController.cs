using System.Security.Claims;
using FinancesControl.API.Accounts.DTOs;
using FinancesControl.API.Accounts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> CreateAccount(CreateAccountRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var id = _accountService.CreateAccount(request, userId);
            return CreatedAtAction(nameof(GetById), new { accountId = id }, null);
        }

        [HttpGet("{accountId}")]
        public async Task<ActionResult<AccountDetailResponse>> GetById(long accountId)
        {
            var response = _accountService.GetById(accountId);
            if (response == null)
                return NotFound();
            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult<List<AccountListItemResponse>>> Get()
        {
            var response = _accountService.GetAll();
            return Ok(response);
        }

        [HttpPut("{accountId}")]
        public async Task<IActionResult> Update(long accountId, UpdateAccountRequest request)
        {
            var updated = _accountService.Update(accountId, request);
            if (!updated)
                return NotFound();
            return NoContent();
        }

        [HttpPost("{accountId}/activate")]
        public async Task<IActionResult> Activate(long accountId)
        {
            var activated = _accountService.Activate(accountId);
            if (!activated)
                return NotFound();
            return NoContent();
        }

        [HttpPost("{accountId}/deactivate")]
        public async Task<IActionResult> Deactivate(long accountId)
        {
            var deactivated = _accountService.Deactivate(accountId);
            if (!deactivated)
                return NotFound();
            return NoContent();
        }
    }
}
