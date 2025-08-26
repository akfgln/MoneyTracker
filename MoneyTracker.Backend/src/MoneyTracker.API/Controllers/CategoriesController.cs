using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Application.Common.Interfaces;
using MoneyTracker.Application.DTOs.Category;
using MoneyTracker.Domain.Enums;
using System.Security.Claims;

namespace MoneyTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(
        ICategoryService categoryService,
        ICurrentUserService currentUserService,
        ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Get categories with filtering and pagination
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCategories([FromQuery] CategoryQueryParameters parameters)
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId) || (Guid.TryParse(userId, out Guid uid) && uid != Guid.Empty))
                return Unauthorized("User not authenticated");

            var result = await _categoryService.GetCategoriesAsync(Guid.Parse(userId), parameters);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCategory(Guid id)
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId) || (Guid.TryParse(userId, out Guid uid) && uid != Guid.Empty))
                return Unauthorized("User not authenticated");

            var category = await _categoryService.GetCategoryByIdAsync(Guid.Parse(userId), id);
            if (category == null)
                return NotFound($"Category with ID {id} not found");

            return Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category {CategoryId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new category
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId) || (Guid.TryParse(userId, out Guid uid) && uid != Guid.Empty))
                return Unauthorized("User not authenticated");

            var category = await _categoryService.CreateCategoryAsync(Guid.Parse(userId), dto);
            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument when creating category");
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when creating category");
            return Forbidden(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update an existing category
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId) || (Guid.TryParse(userId, out Guid uid) && uid != Guid.Empty))
                return Unauthorized("User not authenticated");

            var category = await _categoryService.UpdateCategoryAsync(Guid.Parse(userId), id, dto);
            if (category == null)
                return NotFound($"Category with ID {id} not found");

            return Ok(category);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument when updating category {CategoryId}", id);
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when updating category {CategoryId}", id);
            return Forbidden(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {CategoryId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a category
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId) || (Guid.TryParse(userId, out Guid uid) && uid != Guid.Empty))
                return Unauthorized("User not authenticated");

            var deleted = await _categoryService.DeleteCategoryAsync(Guid.Parse(userId), id);
            if (!deleted)
                return NotFound($"Category with ID {id} not found");

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when deleting category {CategoryId}", id);
            return Forbidden(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when deleting category {CategoryId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {CategoryId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get category hierarchy
    /// </summary>
    [HttpGet("hierarchy")]
    public async Task<IActionResult> GetCategoryHierarchy([FromQuery] CategoryType? categoryType)
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId) || (Guid.TryParse(userId, out Guid uid) && uid != Guid.Empty))
                return Unauthorized("User not authenticated");

            var hierarchy = await _categoryService.GetCategoryHierarchyAsync(Guid.Parse(userId), categoryType);
            return Ok(hierarchy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category hierarchy");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Suggest category based on description and merchant
    /// </summary>
    [HttpPost("suggest")]
    public async Task<IActionResult> SuggestCategory([FromBody] SuggestCategoryDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var suggestions = await _categoryService.SuggestCategoryAsync(dto.Description, dto.MerchantName, dto.Amount, dto.TransactionType);
            return Ok(suggestions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suggesting category for description: {Description}", dto.Description);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get category usage statistics
    /// </summary>
    [HttpGet("{id:guid}/usage-stats")]
    public async Task<IActionResult> GetCategoryUsageStats(Guid id, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId) || (Guid.TryParse(userId, out Guid uid) && uid != Guid.Empty))
                return Unauthorized("User not authenticated");

            var stats = await _categoryService.GetCategoryUsageStatsAsync(Guid.Parse(userId), id, startDate, endDate);
            return Ok(stats);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Category {CategoryId} not found for usage stats", id);
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access to category {CategoryId} usage stats", id);
            return Forbidden(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving usage stats for category {CategoryId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Bulk update multiple categories
    /// </summary>
    [HttpPost("bulk-update")]
    public async Task<IActionResult> BulkUpdateCategories([FromBody] BulkUpdateCategoriesDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId) || (Guid.TryParse(userId, out Guid uid) && uid != Guid.Empty))
                return Unauthorized("User not authenticated");

            var success = await _categoryService.BulkUpdateCategoriesAsync(Guid.Parse(userId), dto);
            if (!success)
                return BadRequest("No categories were updated");

            return Ok(new { Message = "Categories updated successfully", UpdatedCount = dto.CategoryIds.Count });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access during bulk update");
            return Forbidden(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk update of categories");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Import categories from JSON
    /// </summary>
    [HttpPost("import")]
    public async Task<IActionResult> ImportCategories([FromBody] ImportCategoriesDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId) || (Guid.TryParse(userId, out Guid uid) && uid != Guid.Empty))
                return Unauthorized("User not authenticated");

            var importedCategories = await _categoryService.ImportCategoriesAsync(Guid.Parse(userId), dto);
            return Ok(new
            {
                Message = "Categories imported successfully",
                ImportedCount = importedCategories.Count,
                Categories = importedCategories
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing categories");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Export categories to JSON
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportCategories([FromQuery] CategoryType? categoryType)
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId) || (Guid.TryParse(userId, out Guid uid) && uid != Guid.Empty))
                return Unauthorized("User not authenticated");

            var exportData = await _categoryService.ExportCategoriesAsync(Guid.Parse(userId), categoryType);

            var fileName = categoryType.HasValue
                ? $"categories_{categoryType.Value}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json"
                : $"categories_all_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";

            return File(exportData, "application/json", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting categories");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Merge source category into target category
    /// </summary>
    [HttpPost("{id:guid}/merge")]
    public async Task<IActionResult> MergeCategories(Guid id, [FromBody] MergeCategoryDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId) || (Guid.TryParse(userId, out Guid uid) && uid != Guid.Empty))
                return Unauthorized("User not authenticated");

            var success = await _categoryService.MergeCategoriesAsync(Guid.Parse(userId), id, dto.TargetCategoryId);
            if (!success)
                return BadRequest("Failed to merge categories");

            return Ok(new { Message = "Categories merged successfully", SourceCategoryId = id, TargetCategoryId = dto.TargetCategoryId });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument during category merge");
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access during category merge");
            return Forbidden(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error merging categories {SourceId} -> {TargetId}", id, dto.TargetCategoryId);
            return StatusCode(500, "Internal server error");
        }
    }

    #region Helper Methods

    private IActionResult Forbidden(string message)
    {
        return StatusCode(403, new { Error = "Forbidden", Message = message });
    }

    #endregion
}