using System.Security.Claims;
using FinancesControl.API.Cards.DTOs;
using FinancesControl.API.Cards.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
                throw new InvalidOperationException("User context not available");
            return userId;
        }

        [HttpPost("{accountId}/debit")]
        public async Task<IActionResult> CreateDebitCard(long accountId, CreateDebitCardRequest request)
        {
            var userId = GetCurrentUserId();
            try
            {
                long id = _cardService.CreateDebitCard(request, accountId, userId);
                return CreatedAtAction(nameof(GetById), new { accountId = accountId, cardId = id }, null);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }

        [HttpPost("{accountId}/credit")]
        public async Task<IActionResult> CreateCreditCard(long accountId, CreateCreditCardRequest request)
        {
            var userId = GetCurrentUserId();
            try
            {
                long id = _cardService.CreateCreditCard(request, accountId, userId);
                return CreatedAtAction(nameof(GetCreditById), new { accountId = accountId, cardId = id }, null);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }

        [HttpGet("{accountId}/{cardId}")]
        public async Task<ActionResult<CardDetailResponse>> GetById(long accountId, long cardId)
        {
            var userId = GetCurrentUserId();
            var response = _cardService.GetById(cardId, accountId, userId);
            if (response == null)
                return NotFound();
            return Ok(response);
        }

        [HttpGet("{accountId}/credit/{cardId}")]
        public async Task<ActionResult<CreditCardDetailResponse>> GetCreditById(long accountId, long cardId)
        {
            var userId = GetCurrentUserId();
            var response = _cardService.GetCreditById(cardId, accountId, userId);
            if (response == null)
                return NotFound();
            return Ok(response);
        }

        [HttpGet("{accountId}")]
        public async Task<ActionResult<List<CardListItemResponse>>> Get(long accountId)
        {
            var userId = GetCurrentUserId();
            var response = _cardService.GetAll(accountId, userId);
            return Ok(response);
        }

        [HttpPut("{accountId}/{cardId}")]
        public async Task<IActionResult> UpdateCardName(long accountId, long cardId, [FromQuery] string cardName)
        {
            var userId = GetCurrentUserId();
            var updated = _cardService.UpdateCardName(cardId, accountId, userId, cardName);
            if (!updated)
                return NotFound();
            return NoContent();
        }

        [HttpPut("{accountId}/{cardId}/credit-card-dates")]
        public async Task<IActionResult> UpdateCreditCardDates(long accountId, long cardId, UpdateCreditCardRequest request)
        {
            var userId = GetCurrentUserId();
            var updated = _cardService.UpdateCreditCardDates(cardId, accountId, userId, request);
            if (!updated)
                return NotFound();
            return NoContent();
        }

        [HttpPost("{accountId}/{cardId}/activate")]
        public async Task<IActionResult> Activate(long accountId, long cardId)
        {
            var userId = GetCurrentUserId();
            var activated = _cardService.Activate(cardId, accountId, userId);
            if (!activated)
                return NotFound();
            return NoContent();
        }

        [HttpPost("{accountId}/{cardId}/deactivate")]
        public async Task<IActionResult> Deactivate(long accountId, long cardId)
        {
            var userId = GetCurrentUserId();
            var deactivated = _cardService.Deactivate(cardId, accountId, userId);
            if (!deactivated)
                return NotFound();
            return NoContent();
        }
    }
}