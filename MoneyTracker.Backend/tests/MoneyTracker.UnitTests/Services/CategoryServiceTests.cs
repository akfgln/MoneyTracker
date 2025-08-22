using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MoneyTracker.Application.Common.Interfaces;
using MoneyTracker.Application.Common.Models;
using MoneyTracker.Application.DTOs.Category;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Enums;
using MoneyTracker.Infrastructure.Services;

namespace MoneyTracker.UnitTests.Services;

public class CategoryServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ICategoryRepository> _mockCategoryRepository;
    private readonly Mock<ISmartCategorizationService> _mockSmartCategorizationService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<CategoryService>> _mockLogger;
    private readonly CategoryService _categoryService;
    private readonly Guid _testUserId = Guid.NewGuid();

    public CategoryServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockCategoryRepository = new Mock<ICategoryRepository>();
        _mockSmartCategorizationService = new Mock<ISmartCategorizationService>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<CategoryService>>();

        // Setup UnitOfWork to return the mock repository
        _mockUnitOfWork.Setup(x => x.Categories).Returns(_mockCategoryRepository.Object);

        _categoryService = new CategoryService(
            _mockUnitOfWork.Object,
            _mockSmartCategorizationService.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    #region CreateCategoryAsync Tests

    [Fact]
    public async Task CreateCategoryAsync_ValidCategory_ShouldCreateSuccessfully()
    {
        // Arrange
        var createDto = new CreateCategoryDto
        {
            Name = "Test Category",
            Description = "Test Description",
            CategoryType = CategoryType.Expense,
            DefaultVatRate = 0.19m,
            IsActive = true
        };

        var expectedCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = createDto.Name,
            Description = createDto.Description,
            CategoryType = createDto.CategoryType,
            DefaultVatRate = createDto.DefaultVatRate,
            IsActive = createDto.IsActive,
            UserId = _testUserId,
            IsSystemCategory = false
        };

        var expectedResponse = new CategoryResponseDto
        {
            Id = expectedCategory.Id,
            Name = expectedCategory.Name,
            Description = expectedCategory.Description,
            CategoryType = expectedCategory.CategoryType,
            DefaultVatRate = expectedCategory.DefaultVatRate,
            IsActive = expectedCategory.IsActive
        };

        _mockCategoryRepository.Setup(x => x.AddAsync(It.IsAny<Category>(), default))
            .ReturnsAsync(expectedCategory);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(default))
            .ReturnsAsync(1);
        _mockMapper.Setup(x => x.Map<CategoryResponseDto>(It.IsAny<Category>()))
            .Returns(expectedResponse);

        // Act
        var result = await _categoryService.CreateCategoryAsync(_testUserId, createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Name, result.Name);
        Assert.Equal(expectedResponse.CategoryType, result.CategoryType);
        Assert.Equal(expectedResponse.DefaultVatRate, result.DefaultVatRate);
        _mockCategoryRepository.Verify(x => x.AddAsync(It.IsAny<Category>(), default), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task CreateCategoryAsync_WithParentCategory_ShouldValidateParent()
    {
        // Arrange
        var parentCategoryId = Guid.NewGuid();
        var parentCategory = new Category
        {
            Id = parentCategoryId,
            CategoryType = CategoryType.Expense,
            IsSystemCategory = true,
            UserId = Guid.NewGuid()
        };

        var createDto = new CreateCategoryDto
        {
            Name = "Child Category",
            CategoryType = CategoryType.Expense,
            ParentCategoryId = parentCategoryId,
            DefaultVatRate = 0.19m
        };

        _mockCategoryRepository.Setup(x => x.GetByIdAsync(parentCategoryId, default))
            .ReturnsAsync(parentCategory);

        var expectedCategory = new Category();
        _mockCategoryRepository.Setup(x => x.AddAsync(It.IsAny<Category>(), default))
            .ReturnsAsync(expectedCategory);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(default))
            .ReturnsAsync(1);
        _mockMapper.Setup(x => x.Map<CategoryResponseDto>(It.IsAny<Category>()))
            .Returns(new CategoryResponseDto());

        // Act
        var result = await _categoryService.CreateCategoryAsync(_testUserId, createDto);

        // Assert
        Assert.NotNull(result);
        _mockCategoryRepository.Verify(x => x.GetByIdAsync(parentCategoryId, default), Times.Once);
    }

    [Fact]
    public async Task CreateCategoryAsync_InvalidParentCategory_ShouldThrowException()
    {
        // Arrange
        var parentCategoryId = Guid.NewGuid();
        var createDto = new CreateCategoryDto
        {
            Name = "Child Category",
            CategoryType = CategoryType.Expense,
            ParentCategoryId = parentCategoryId,
            DefaultVatRate = 0.19m
        };

        _mockCategoryRepository.Setup(x => x.GetByIdAsync(parentCategoryId, default))
            .ReturnsAsync((Category?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _categoryService.CreateCategoryAsync(_testUserId, createDto));
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public async Task CreateCategoryAsync_InvalidVatRate_ShouldThrowException(decimal invalidVatRate)
    {
        // Arrange
        var createDto = new CreateCategoryDto
        {
            Name = "Test Category",
            CategoryType = CategoryType.Expense,
            DefaultVatRate = invalidVatRate
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _categoryService.CreateCategoryAsync(_testUserId, createDto));
    }

    #endregion

    #region GetCategoryByIdAsync Tests

    [Fact]
    public async Task GetCategoryByIdAsync_ExistingCategory_ShouldReturnCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category
        {
            Id = categoryId,
            Name = "Test Category",
            UserId = _testUserId,
            IsActive = true
        };

        var expectedResponse = new CategoryResponseDto
        {
            Id = categoryId,
            Name = "Test Category"
        };

        _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryId, default))
            .ReturnsAsync(category);
        _mockMapper.Setup(x => x.Map<CategoryResponseDto>(category))
            .Returns(expectedResponse);

        // Act
        var result = await _categoryService.GetCategoryByIdAsync(_testUserId, categoryId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Id, result.Id);
        Assert.Equal(expectedResponse.Name, result.Name);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_NonExistentCategory_ShouldReturnNull()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryId, default))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _categoryService.GetCategoryByIdAsync(_testUserId, categoryId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_CategoryBelongsToOtherUser_ShouldReturnNull()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var category = new Category
        {
            Id = categoryId,
            Name = "Test Category",
            UserId = otherUserId,
            IsSystemCategory = false
        };

        _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryId, default))
            .ReturnsAsync(category);

        // Act
        var result = await _categoryService.GetCategoryByIdAsync(_testUserId, categoryId);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region UpdateCategoryAsync Tests

    [Fact]
    public async Task UpdateCategoryAsync_ValidUpdate_ShouldUpdateSuccessfully()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var existingCategory = new Category
        {
            Id = categoryId,
            Name = "Original Name",
            Description = "Original Description",
            UserId = _testUserId,
            IsActive = true,
            CategoryType = CategoryType.Expense
        };

        var updateDto = new UpdateCategoryDto
        {
            Name = "Updated Name",
            Description = "Updated Description",
            DefaultVatRate = 0.19m
        };

        var expectedResponse = new CategoryResponseDto
        {
            Id = categoryId,
            Name = "Updated Name",
            Description = "Updated Description"
        };

        _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryId, default))
            .ReturnsAsync(existingCategory);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(default))
            .ReturnsAsync(1);
        _mockMapper.Setup(x => x.Map<CategoryResponseDto>(It.IsAny<Category>()))
            .Returns(expectedResponse);

        // Act
        var result = await _categoryService.UpdateCategoryAsync(_testUserId, categoryId, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Name, result.Name);
        Assert.Equal(expectedResponse.Description, result.Description);
        _mockCategoryRepository.Verify(x => x.Update(It.IsAny<Category>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateCategoryAsync_NonExistentCategory_ShouldReturnNull()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var updateDto = new UpdateCategoryDto { Name = "Updated Name" };

        _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryId, default))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _categoryService.UpdateCategoryAsync(_testUserId, categoryId, updateDto);

        // Assert
        Assert.Null(result);
        _mockCategoryRepository.Verify(x => x.Update(It.IsAny<Category>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(default), Times.Never);
    }

    #endregion

    #region DeleteCategoryAsync Tests

    [Fact]
    public async Task DeleteCategoryAsync_ValidCategory_ShouldDeleteSuccessfully()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category
        {
            Id = categoryId,
            Name = "Test Category",
            UserId = _testUserId,
            IsSystemCategory = false
        };

        _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryId, default))
            .ReturnsAsync(category);
        _mockCategoryRepository.Setup(x => x.HasTransactionsAsync(categoryId, default))
            .ReturnsAsync(false);
        _mockCategoryRepository.Setup(x => x.HasSubCategoriesAsync(categoryId, default))
            .ReturnsAsync(false);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        var result = await _categoryService.DeleteCategoryAsync(_testUserId, categoryId);

        // Assert
        Assert.True(result);
        _mockCategoryRepository.Verify(x => x.Delete(category), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteCategoryAsync_CategoryWithTransactions_ShouldReturnFalse()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category
        {
            Id = categoryId,
            Name = "Test Category",
            UserId = _testUserId,
            IsSystemCategory = false
        };

        _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryId, default))
            .ReturnsAsync(category);
        _mockCategoryRepository.Setup(x => x.HasTransactionsAsync(categoryId, default))
            .ReturnsAsync(true);

        // Act
        var result = await _categoryService.DeleteCategoryAsync(_testUserId, categoryId);

        // Assert
        Assert.False(result);
        _mockCategoryRepository.Verify(x => x.Delete(It.IsAny<Category>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task DeleteCategoryAsync_SystemCategory_ShouldReturnFalse()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category
        {
            Id = categoryId,
            Name = "System Category",
            UserId = _testUserId,
            IsSystemCategory = true
        };

        _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryId, default))
            .ReturnsAsync(category);

        // Act
        var result = await _categoryService.DeleteCategoryAsync(_testUserId, categoryId);

        // Assert
        Assert.False(result);
        _mockCategoryRepository.Verify(x => x.Delete(It.IsAny<Category>()), Times.Never);
    }

    #endregion

    #region GetCategoryHierarchyAsync Tests

    [Fact]
    public async Task GetCategoryHierarchyAsync_ValidRequest_ShouldReturnHierarchy()
    {
        // Arrange
        var parentCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Parent Category",
            CategoryType = CategoryType.Expense,
            ParentCategoryId = null,
            UserId = _testUserId
        };

        var childCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Child Category",
            CategoryType = CategoryType.Expense,
            ParentCategoryId = parentCategory.Id,
            UserId = _testUserId
        };

        var categories = new List<Category> { parentCategory, childCategory };

        _mockCategoryRepository.Setup(x => x.GetUserCategoriesWithSystemAsync(_testUserId, CategoryType.Expense, default))
            .ReturnsAsync(categories);

        var parentDto = new CategoryResponseDto
        {
            Id = parentCategory.Id,
            Name = parentCategory.Name,
            CategoryType = parentCategory.CategoryType,
            SubCategories = new List<CategoryResponseDto>()
        };

        var childDto = new CategoryResponseDto
        {
            Id = childCategory.Id,
            Name = childCategory.Name,
            CategoryType = childCategory.CategoryType
        };

        _mockMapper.Setup(x => x.Map<CategoryResponseDto>(parentCategory))
            .Returns(parentDto);
        _mockMapper.Setup(x => x.Map<CategoryResponseDto>(childCategory))
            .Returns(childDto);

        // Act
        var result = await _categoryService.GetCategoryHierarchyAsync(_testUserId, CategoryType.Expense);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(CategoryType.Expense, result[0].CategoryType);
    }

    #endregion

    #region SuggestCategoryAsync Tests

    [Fact]
    public async Task SuggestCategoryAsync_ValidInput_ShouldReturnSuggestions()
    {
        // Arrange
        var description = "REWE Supermarkt";
        var merchantName = "REWE";
        var amount = 25.50m;
        var transactionType = TransactionType.Expense;

        var expectedSuggestions = new List<CategorySuggestionDto>
        {
            new()
            {
                CategoryId = Guid.NewGuid(),
                CategoryName = "Lebensmittel",
                ConfidenceScore = 0.9,
                MatchReason = "Merchant match: REWE"
            }
        };

        _mockSmartCategorizationService.Setup(x => x.SuggestCategoriesAsync(
            description, merchantName, amount, transactionType))
            .ReturnsAsync(expectedSuggestions);

        // Act
        var result = await _categoryService.SuggestCategoryAsync(description, merchantName, amount, transactionType);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(expectedSuggestions[0].CategoryName, result[0].CategoryName);
        Assert.Equal(expectedSuggestions[0].ConfidenceScore, result[0].ConfidenceScore);
    }

    #endregion
}
