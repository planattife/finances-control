using System.Security.Claims;
using FinancesControl.API.Cards.DTOs;
using FinancesControl.API.Cards.Services;
using FinancesControl.API.Common;
using FinancesControl.API.Common.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FinancesControl.API.Cards.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CardController : ControllerBase
    {
        private readonly ICardService _cardService;

        public CardController(ICardService cardService)
        {
            _cardService = cardService;
        }

        private string GetCurrentUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new AuthenticationContextException("User context is not available.");
            return userId;
        }

        [HttpPost("{accountId}/debit")]
        [EnableRateLimiting(RateLimitPolicyNames.FinanceWrite)]
        public async Task<IActionResult> CreateDebitCard(long accountId, CreateDebitCardRequest request)
        {
            var userId = GetCurrentUserId();
            long id = await _cardService.CreateDebitCard(request, accountId, userId);
            return CreatedAtAction(nameof(GetById), new { accountId = accountId, cardId = id }, null);
        }

        [HttpPost("{accountId}/credit")]
        [EnableRateLimiting(RateLimitPolicyNames.FinanceWrite)]
        public async Task<IActionResult> CreateCreditCard(long accountId, CreateCreditCardRequest request)
        {
            var userId = GetCurrentUserId();
            long id = await _cardService.CreateCreditCard(request, accountId, userId);
            return CreatedAtAction(nameof(GetCreditById), new { accountId = accountId, cardId = id }, null);
        }

        [HttpGet("{accountId}/{cardId}")]
        public async Task<ActionResult<SingleResponse<CardDetailResponse>>> GetById(long accountId, long cardId)
        {
            var userId = GetCurrentUserId();
            var response = await _cardService.GetById(cardId, accountId, userId);
            return StatusCode(response.HttpStatusCode, response);
        }

        [HttpGet("{accountId}/credit/{cardId}")]
        public async Task<ActionResult<SingleResponse<CreditCardDetailResponse>>> GetCreditById(long accountId, long cardId)
        {
            var userId = GetCurrentUserId();
            var response = await _cardService.GetCreditById(cardId, accountId, userId);
            return StatusCode(response.HttpStatusCode, response);
        }

        [HttpGet("{accountId}")]
        public async Task<ActionResult<PaginatedResponse<CardListItemResponse>>> Get(long accountId, [FromQuery] PaginationParams paginationParams)
        {
            var userId = GetCurrentUserId();
            var response = await _cardService.GetAll(accountId, userId, paginationParams);
            return Ok(response);
        }

        [HttpPut("{accountId}/{cardId}")]
        [EnableRateLimiting(RateLimitPolicyNames.FinanceWrite)]
        public async Task<IActionResult> UpdateCardName(long accountId, long cardId, [FromQuery] string cardName)
        {
            var userId = GetCurrentUserId();
            var updated = await _cardService.UpdateCardName(cardId, accountId, userId, cardName);
            if (!updated)
                return NotFound();
            return NoContent();
        }

        [HttpPut("{accountId}/{cardId}/credit-card-dates")]
        [EnableRateLimiting(RateLimitPolicyNames.FinanceWrite)]
        public async Task<IActionResult> UpdateCreditCardDates(long accountId, long cardId, UpdateCreditCardRequest request)
        {
            var userId = GetCurrentUserId();
            var updated = await _cardService.UpdateCreditCardDates(cardId, accountId, userId, request);
            if (!updated)
                return NotFound();
            return NoContent();
        }

        [HttpPost("{accountId}/{cardId}/activate")]
        [EnableRateLimiting(RateLimitPolicyNames.FinanceWrite)]
        public async Task<IActionResult> Activate(long accountId, long cardId)
        {
            var userId = GetCurrentUserId();
            var activated = await _cardService.Activate(cardId, accountId, userId);
            if (!activated)
                return NotFound();
            return NoContent();
        }

        [HttpPost("{accountId}/{cardId}/deactivate")]
        [EnableRateLimiting(RateLimitPolicyNames.FinanceWrite)]
        public async Task<IActionResult> Deactivate(long accountId, long cardId)
        {
            var userId = GetCurrentUserId();
            var deactivated = await _cardService.Deactivate(cardId, accountId, userId);
            if (!deactivated)
                return NotFound();
            return NoContent();
        }
    }
}
