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

        [HttpPost]
        public async Task<IActionResult> Create(CreateTransactionCategoryRequest request)
        {
            long id = _service.CreateCategory(request);
            return CreatedAtAction(nameof(GetById), new { categoryId = id }, null);
        }

        [HttpGet("{categoryId}")]
        public async Task<ActionResult<TransactionCategoryDetailResponse>> GetById(long categoryId)
        {
            var resp = _service.GetById(categoryId);
            if (resp == null) return NotFound();
            return Ok(resp);
        }

        [HttpGet]
        public async Task<ActionResult<List<TransactionCategoryListItemResponse>>> Get()
        {
            var resp = _service.GetAll();
            return Ok(resp);
        }

        [HttpPatch("{categoryId}")]
        public async Task<IActionResult> Rename(long categoryId, RenameTransactionCategoryRequest request)
        {
            var updated = _service.Rename(categoryId, request);
            if (!updated) return NotFound();
            return NoContent();
        }

        [HttpPost("{categoryId}/activate")]
        public async Task<IActionResult> Activate(long categoryId)
        {
            var ok = _service.Activate(categoryId);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpPost("{categoryId}/deactivate")]
        public async Task<IActionResult> Deactivate(long categoryId)
        {
            var ok = _service.Deactivate(categoryId);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}