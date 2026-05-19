using System.Security.Claims;
using FinancesControl.API.TransactionCategories.DTOs;
using FinancesControl.API.TransactionCategories.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancesControl.API.TransactionCategories.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TransactionCategoryController : ControllerBase
    {
        private readonly ITransactionCategoryService _service;

        public TransactionCategoryController(ITransactionCategoryService service)
        {
            _service = service;
        }

        private string? GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTransactionCategoryRequest request)
        {
            var userId = GetUserId();
            long id = await _service.CreateCategory(request, userId);
            return CreatedAtAction(nameof(GetById), new { categoryId = id }, null);
        }

        [HttpGet("{categoryId}")]
        public async Task<ActionResult<TransactionCategoryDetailResponse>> GetById(long categoryId)
        {
            var userId = GetUserId();
            var resp = await _service.GetById(categoryId, userId);
            if (resp == null) return NotFound();
            return Ok(resp);
        }

        [HttpGet]
        public async Task<ActionResult<List<TransactionCategoryListItemResponse>>> Get()
        {
            var userId = GetUserId();
            var resp = await _service.GetAll(userId);
            return Ok(resp);
        }

        [HttpPatch("{categoryId}")]
        public async Task<IActionResult> Rename(long categoryId, RenameTransactionCategoryRequest request)
        {
            var userId = GetUserId();
            var updated = await _service.Rename(categoryId, request, userId);
            if (!updated) return NotFound();
            return NoContent();
        }

        [HttpPost("{categoryId}/activate")]
        public async Task<IActionResult> Activate(long categoryId)
        {
            var userId = GetUserId();
            var ok = await _service.Activate(categoryId, userId);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpPost("{categoryId}/deactivate")]
        public async Task<IActionResult> Deactivate(long categoryId)
        {
            var userId = GetUserId();
            var ok = await _service.Deactivate(categoryId, userId);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}