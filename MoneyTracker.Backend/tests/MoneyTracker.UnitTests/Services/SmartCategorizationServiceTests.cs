using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MoneyTracker.Application.Common.Interfaces;
using MoneyTracker.Application.DTOs.Category;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Enums;
using MoneyTracker.Infrastructure.Services;

namespace MoneyTracker.UnitTests.Services;

public class SmartCategorizationServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ICategoryRepository> _mockCategoryRepository;
    private readonly Mock<ILogger<SmartCategorizationService>> _mockLogger;
    private readonly SmartCategorizationService _smartCategorizationService;

    public SmartCategorizationServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockCategoryRepository = new Mock<ICategoryRepository>();
        _mockLogger = new Mock<ILogger<SmartCategorizationService>>();

        _mockUnitOfWork.Setup(x => x.Categories).Returns(_mockCategoryRepository.Object);

        _smartCategorizationService = new SmartCategorizationService(
            _mockUnitOfWork.Object,
            _mockLogger.Object);
    }

    #region SuggestCategoriesAsync Tests

    [Theory]
    [InlineData("REWE Supermarkt Einkauf", "REWE", "food", TransactionType.Expense)]
    [InlineData("Tankstelle Shell Benzin", "Shell", "transportation", TransactionType.Expense)]
    [InlineData("Gehalt Januar 2024", "Arbeitgeber GmbH", "salary", TransactionType.Income)]
    [InlineData("Netflix Abonnement", "Netflix", "entertainment", TransactionType.Expense)]
    public async Task SuggestCategoriesAsync_GermanKeywords_ShouldReturnRelevantSuggestions(
        string description, string merchantName, string expectedCategoryKey, TransactionType transactionType)
    {
        // Arrange
        var categories = new List<Category>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = GetCategoryNameByKey(expectedCategoryKey),
                CategoryType = transactionType == TransactionType.Income ? CategoryType.Income : CategoryType.Expense,
                Keywords = GetKeywordsByCategoryKey(expectedCategoryKey),
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Unrelated Category",
                CategoryType = transactionType == TransactionType.Income ? CategoryType.Income : CategoryType.Expense,
                Keywords = "unrelated,keywords",
                IsActive = true
            }
        };

        _mockCategoryRepository.Setup(x => x.GetCategoriesByTypeAsync(
            transactionType == TransactionType.Income ? CategoryType.Income : CategoryType.Expense, default))
            .ReturnsAsync(categories);

        // Act
        var result = await _smartCategorizationService.SuggestCategoriesAsync(
            description, merchantName, null, transactionType);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        
        // The first suggestion should be the most relevant one
        var topSuggestion = result[0];
        Assert.True(topSuggestion.ConfidenceScore > 0.3); // Minimum confidence threshold
        Assert.Contains(expectedCategoryKey, GetCategoryKeyFromName(topSuggestion.CategoryName), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SuggestCategoriesAsync_NoMatchingCategories_ShouldReturnEmptyList()
    {
        // Arrange
        var description = "Unknown merchant transaction";
        var merchantName = "UnknownMerchant";
        var categories = new List<Category>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Unrelated Category",
                CategoryType = CategoryType.Expense,
                Keywords = "completely,different,keywords",
                IsActive = true
            }
        };

        _mockCategoryRepository.Setup(x => x.GetCategoriesByTypeAsync(CategoryType.Expense, default))
            .ReturnsAsync(categories);

        // Act
        var result = await _smartCategorizationService.SuggestCategoriesAsync(
            description, merchantName, null, TransactionType.Expense);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result); // No suggestions should meet the minimum confidence threshold
    }

    [Fact]
    public async Task SuggestCategoriesAsync_MultipleMerchantMatches_ShouldReturnOrderedByConfidence()
    {
        // Arrange
        var description = "Amazon Prime Mitgliedschaft";
        var merchantName = "Amazon";
        var categories = new List<Category>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Entertainment",
                CategoryType = CategoryType.Expense,
                Keywords = "entertainment,streaming,video,prime",
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Shopping",
                CategoryType = CategoryType.Expense,
                Keywords = "shopping,amazon,online,einkauf",
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Books",
                CategoryType = CategoryType.Expense,
                Keywords = "books,reading,literature",
                IsActive = true
            }
        };

        _mockCategoryRepository.Setup(x => x.GetCategoriesByTypeAsync(CategoryType.Expense, default))
            .ReturnsAsync(categories);

        // Act
        var result = await _smartCategorizationService.SuggestCategoriesAsync(
            description, merchantName, null, TransactionType.Expense);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count > 1);
        
        // Results should be ordered by confidence score (descending)
        for (int i = 0; i < result.Count - 1; i++)
        {
            Assert.True(result[i].ConfidenceScore >= result[i + 1].ConfidenceScore);
        }
    }

    [Fact]
    public async Task SuggestCategoriesAsync_LimitToTopFiveSuggestions_ShouldReturnMaxFiveResults()
    {
        // Arrange
        var description = "general transaction description";
        var merchantName = "general merchant";
        var categories = new List<Category>();
        
        // Create 10 categories that would all match with low confidence
        for (int i = 0; i < 10; i++)
        {
            categories.Add(new Category
            {
                Id = Guid.NewGuid(),
                Name = $"Category {i}",
                CategoryType = CategoryType.Expense,
                Keywords = "general,transaction,description", // All will match with same confidence
                IsActive = true
            });
        }

        _mockCategoryRepository.Setup(x => x.GetCategoriesByTypeAsync(CategoryType.Expense, default))
            .ReturnsAsync(categories);

        // Act
        var result = await _smartCategorizationService.SuggestCategoriesAsync(
            description, merchantName, null, TransactionType.Expense);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count <= 5); // Should be limited to 5 results
    }

    #endregion

    #region LearnFromUserChoiceAsync Tests

    [Fact]
    public async Task LearnFromUserChoiceAsync_ValidInput_ShouldUpdateCategoryKeywords()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var description = "Unique merchant transaction";
        var merchantName = "UniqueMerchant";
        
        var category = new Category
        {
            Id = categoryId,
            Name = "Test Category",
            Keywords = "existing,keywords",
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryId, default))
            .ReturnsAsync(category);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        await _smartCategorizationService.LearnFromUserChoiceAsync(
            userId, description, merchantName, categoryId);

        // Assert
        _mockCategoryRepository.Verify(x => x.GetByIdAsync(categoryId, default), Times.Once);
        _mockCategoryRepository.Verify(x => x.Update(It.Is<Category>(c => 
            c.Keywords.Contains("existing") && 
            c.Keywords.Contains("unique") && 
            c.Keywords.Contains("merchant") &&
            c.Keywords.Contains("transaction"))), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task LearnFromUserChoiceAsync_NonExistentCategory_ShouldNotThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var description = "Test description";
        var merchantName = "TestMerchant";

        _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryId, default))
            .ReturnsAsync((Category?)null);

        // Act & Assert (should not throw)
        await _smartCategorizationService.LearnFromUserChoiceAsync(
            userId, description, merchantName, categoryId);

        _mockCategoryRepository.Verify(x => x.Update(It.IsAny<Category>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task LearnFromUserChoiceAsync_ShouldFilterCommonWords()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var description = "Das ist eine Transaktion mit und für den Test"; // Contains common German words
        var merchantName = "TestMerchant";
        
        var category = new Category
        {
            Id = categoryId,
            Name = "Test Category",
            Keywords = "",
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryId, default))
            .ReturnsAsync(category);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        await _smartCategorizationService.LearnFromUserChoiceAsync(
            userId, description, merchantName, categoryId);

        // Assert
        _mockCategoryRepository.Verify(x => x.Update(It.Is<Category>(c => 
            !c.Keywords.Contains("das") && 
            !c.Keywords.Contains("ist") && 
            !c.Keywords.Contains("und") && 
            c.Keywords.Contains("transaktion") &&
            c.Keywords.Contains("testmerchant"))), Times.Once);
    }

    #endregion

    #region Helper Methods

    private string GetCategoryNameByKey(string categoryKey)
    {
        return categoryKey switch
        {
            "food" => "Lebensmittel & Essen",
            "transportation" => "Transport & Verkehr",
            "salary" => "Gehalt/Lohn",
            "entertainment" => "Unterhaltung & Freizeit",
            "shopping" => "Einkauf & Shopping",
            "housing" => "Wohnen & Nebenkosten",
            "healthcare" => "Gesundheit & Medizin",
            _ => "Sonstige Ausgaben"
        };
    }

    private string GetKeywordsByCategoryKey(string categoryKey)
    {
        return categoryKey switch
        {
            "food" => "supermarkt,restaurant,café,rewe,edeka,aldi,lidl",
            "transportation" => "tankstelle,bahn,bus,uber,taxi,shell,aral",
            "salary" => "gehalt,lohn,salär,vergütung,entgelt",
            "entertainment" => "kino,theater,konzert,netflix,spotify,streaming",
            "shopping" => "amazon,zalando,otto,kleidung,einkauf",
            "housing" => "miete,nebenkosten,strom,gas,wasser",
            "healthcare" => "apotheke,arzt,krankenhaus,medikament",
            _ => "sonstige,andere,verschiedene"
        };
    }

    private string GetCategoryKeyFromName(string categoryName)
    {
        var nameLower = categoryName.ToLower();
        if (nameLower.Contains("lebensmittel") || nameLower.Contains("essen")) return "food";
        if (nameLower.Contains("transport") || nameLower.Contains("verkehr")) return "transportation";
        if (nameLower.Contains("gehalt") || nameLower.Contains("lohn")) return "salary";
        if (nameLower.Contains("unterhaltung") || nameLower.Contains("freizeit")) return "entertainment";
        if (nameLower.Contains("einkauf") || nameLower.Contains("shopping")) return "shopping";
        if (nameLower.Contains("wohnen") || nameLower.Contains("miete")) return "housing";
        if (nameLower.Contains("gesundheit") || nameLower.Contains("medizin")) return "healthcare";
        return "other";
    }

    #endregion
}
