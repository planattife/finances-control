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

        [HttpPost]
        public async Task<IActionResult> CreateBank(CreateBankRequest request)
        {
            var id = _bankService.CreateBank(request);
            return CreatedAtAction(nameof(GetById), new { bankId = id }, null);
        }

        [HttpGet("{bankId}")]
        public async Task<ActionResult<BankDetailResponse>> GetById(int bankId)
        {
            var response = _bankService.GetById(bankId);
            if (response == null)
                return NotFound();
            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult<List<BankListItemResponse>>> Get()
        {
            var response = _bankService.GetAll();
            return Ok(response);
        }

        [HttpPut("{bankId}")]
        public async Task<IActionResult> Update(int bankId, UpdateBankRequest request)
        {
            var updated = _bankService.Update(bankId, request);
            if (!updated)
                return NotFound();
            return NoContent();
        }

        [HttpPost("{bankId}/activate")]
        public async Task<IActionResult> Activate(int bankId)
        {
            var activated = _bankService.Activate(bankId);
            if (!activated)
                return NotFound();
            return NoContent();
        }

        [HttpPost("{bankId}/deactivate")]
        public async Task<IActionResult> Deactivate(int bankId)
        {
            var deactivated = _bankService.Deactivate(bankId);
            if (!deactivated)
                return NotFound();
            return NoContent();
        }
    }
}