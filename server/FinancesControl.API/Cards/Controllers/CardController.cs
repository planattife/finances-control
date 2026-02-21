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

        [HttpPost("debit")]
        public async Task<IActionResult> CreateDebitCard(CreateDebitCardRequest request)
        {
            var id = _cardService.CreateDebitCard(request);
            return CreatedAtAction(nameof(GetById), new { cardId = id }, null);
        }

        [HttpPost("credit")]
        public async Task<IActionResult> CreateCreditCard(CreateCreditCardRequest request)
        {
            var id = _cardService.CreateCreditCard(request);
            return CreatedAtAction(nameof(GetCreditById), new { cardId = id }, null);
        }

        [HttpGet("{cardId}")]
        public async Task<ActionResult<CardDetailResponse>> GetById(int cardId)
        {
            var response = _cardService.GetById(cardId);
            if (response == null)
                return NotFound();
            return Ok(response);
        }

        [HttpGet("credit/{cardId}")]
        public async Task<ActionResult<CreditCardDetailResponse>> GetCreditById(int cardId)
        {
            var response = _cardService.GetCreditById(cardId);
            if (response == null)
                return NotFound();
            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult<List<CardListItemResponse>>> Get()
        {
            var response = _cardService.GetAll();
            return Ok(response);
        }

        [HttpPut("{cardId}")]
        public async Task<IActionResult> UpdateCardName(int cardId, string cardName)
        {
            var updated = _cardService.UpdateCardName(cardId, cardName);
            if (!updated)
                return NotFound();
            return NoContent();
        }

        [HttpPut("{cardId}/credit-card-dates")]
        public async Task<IActionResult> UpdateCreditCardDates(int cardId, UpdateCreditCardRequest request)
        {
            var updated = _cardService.UpdateCreditCardDates(cardId, request);
            if (!updated)
                return NotFound();
            return NoContent();
        }

        [HttpPost("{cardId}/activate")]
        public async Task<IActionResult> Activate(int cardId)
        {
            var activated = _cardService.Activate(cardId);
            if (!activated)
                return NotFound();
            return NoContent();
        }

        [HttpPost("{cardId}/deactivate")]
        public async Task<IActionResult> Deactivate(int cardId)
        {
            var deactivated = _cardService.Deactivate(cardId);
            if (!deactivated)
                return NotFound();
            return NoContent();
        }
    }
}