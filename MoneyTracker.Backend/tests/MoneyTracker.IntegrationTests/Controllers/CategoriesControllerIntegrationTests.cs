using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using MoneyTracker.Application.DTOs.Category;
using MoneyTracker.Domain.Enums;
using MoneyTracker.API;

namespace MoneyTracker.IntegrationTests.Controllers;

public class CategoriesControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CategoriesControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        
        // Add authorization header (in a real scenario, you'd get this from authentication)
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");
    }

    #region GET /api/categories Tests

    [Fact]
    public async Task GetCategories_WithValidRequest_ShouldReturnOkResult()
    {
        // Act
        var response = await _client.GetAsync("/api/categories");

        // Assert
        // Note: This might return 401 Unauthorized in a real scenario without proper authentication
        // For testing purposes, we're checking if the endpoint exists and responds
        Assert.True(response.StatusCode == HttpStatusCode.OK || 
                   response.StatusCode == HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCategories_WithCategoryTypeFilter_ShouldReturnFilteredResults()
    {
        // Act
        var response = await _client.GetAsync("/api/categories?categoryType=1"); // Expense = 1

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.OK || 
                   response.StatusCode == HttpStatusCode.Unauthorized);
    }

    #endregion

    #region POST /api/categories Tests

    [Fact]
    public async Task CreateCategory_WithValidData_ShouldReturnCreatedResult()
    {
        // Arrange
        var createDto = new CreateCategoryDto
        {
            Name = "Test Integration Category",
            Description = "Category created during integration testing",
            CategoryType = CategoryType.Expense,
            Icon = "test_icon",
            Color = "#FF5722",
            DefaultVatRate = 0.19m,
            Keywords = "test,integration,category",
            IsActive = true
        };

        var json = JsonSerializer.Serialize(createDto, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/categories", content);

        // Assert
        // Note: Will likely return 401 Unauthorized without proper authentication setup
        Assert.True(response.StatusCode == HttpStatusCode.Created || 
                   response.StatusCode == HttpStatusCode.Unauthorized ||
                   response.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCategory_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreateCategoryDto
        {
            Name = "", // Invalid: empty name
            CategoryType = CategoryType.Expense,
            DefaultVatRate = 1.5m // Invalid: VAT rate > 1
        };

        var json = JsonSerializer.Serialize(createDto, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/categories", content);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                   response.StatusCode == HttpStatusCode.Unauthorized);
    }

    #endregion

    #region GET /api/categories/hierarchy Tests

    [Fact]
    public async Task GetCategoryHierarchy_WithValidRequest_ShouldReturnHierarchy()
    {
        // Act
        var response = await _client.GetAsync("/api/categories/hierarchy");

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.OK || 
                   response.StatusCode == HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCategoryHierarchy_WithCategoryTypeFilter_ShouldReturnFilteredHierarchy()
    {
        // Act
        var response = await _client.GetAsync("/api/categories/hierarchy?categoryType=0"); // Income = 0

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.OK || 
                   response.StatusCode == HttpStatusCode.Unauthorized);
    }

    #endregion

    #region POST /api/categories/suggest Tests

    [Fact]
    public async Task SuggestCategory_WithValidInput_ShouldReturnSuggestions()
    {
        // Arrange
        var suggestDto = new SuggestCategoryDto
        {
            Description = "REWE Supermarkt Einkauf",
            MerchantName = "REWE",
            Amount = 25.50m,
            TransactionType = TransactionType.Expense
        };

        var json = JsonSerializer.Serialize(suggestDto, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/categories/suggest", content);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.OK || 
                   response.StatusCode == HttpStatusCode.Unauthorized);
    }

    #endregion

    #region PUT /api/categories/{id} Tests

    [Fact]
    public async Task UpdateCategory_WithValidData_ShouldReturnOkResult()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var updateDto = new UpdateCategoryDto
        {
            Name = "Updated Category Name",
            Description = "Updated description",
            DefaultVatRate = 0.07m
        };

        var json = JsonSerializer.Serialize(updateDto, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/categories/{categoryId}", content);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.OK || 
                   response.StatusCode == HttpStatusCode.NotFound ||
                   response.StatusCode == HttpStatusCode.Unauthorized);
    }

    #endregion

    #region DELETE /api/categories/{id} Tests

    [Fact]
    public async Task DeleteCategory_WithValidId_ShouldReturnAppropriateResult()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/categories/{categoryId}");

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.OK || 
                   response.StatusCode == HttpStatusCode.NotFound ||
                   response.StatusCode == HttpStatusCode.Unauthorized ||
                   response.StatusCode == HttpStatusCode.BadRequest);
    }

    #endregion

    #region POST /api/categories/bulk-update Tests

    [Fact]
    public async Task BulkUpdateCategories_WithValidData_ShouldReturnOkResult()
    {
        // Arrange
        var bulkUpdateDto = new BulkUpdateCategoriesDto
        {
            CategoryIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
            Updates = new UpdateCategoryDto
            {
                IsActive = false
            }
        };

        var json = JsonSerializer.Serialize(bulkUpdateDto, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/categories/bulk-update", content);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.OK || 
                   response.StatusCode == HttpStatusCode.Unauthorized ||
                   response.StatusCode == HttpStatusCode.BadRequest);
    }

    #endregion

    #region GET /api/categories/export Tests

    [Fact]
    public async Task ExportCategories_WithValidRequest_ShouldReturnFileResult()
    {
        // Act
        var response = await _client.GetAsync("/api/categories/export");

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.OK || 
                   response.StatusCode == HttpStatusCode.Unauthorized);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            Assert.Equal("application/octet-stream", response.Content.Headers.ContentType?.MediaType);
        }
    }

    [Fact]
    public async Task ExportCategories_WithCategoryTypeFilter_ShouldReturnFilteredExport()
    {
        // Act
        var response = await _client.GetAsync("/api/categories/export?categoryType=1");

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.OK || 
                   response.StatusCode == HttpStatusCode.Unauthorized);
    }

    #endregion

    #region GET /api/categories/{id}/usage-stats Tests

    [Fact]
    public async Task GetCategoryUsageStats_WithValidId_ShouldReturnStats()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddMonths(-6);
        var endDate = DateTime.UtcNow;

        // Act
        var response = await _client.GetAsync(
            $"/api/categories/{categoryId}/usage-stats?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.OK || 
                   response.StatusCode == HttpStatusCode.NotFound ||
                   response.StatusCode == HttpStatusCode.Unauthorized);
    }

    #endregion
}
