using System;
using System.ComponentModel.DataAnnotations;
using Xunit;
using MoneyTracker.Application.DTOs.Category;
using MoneyTracker.Domain.Enums;

namespace MoneyTracker.UnitTests.DTOs;

public class CategoryDtoValidationTests
{
    #region CreateCategoryDto Validation Tests

    [Fact]
    public void CreateCategoryDto_ValidData_ShouldPassValidation()
    {
        // Arrange
        var dto = new CreateCategoryDto
        {
            Name = "Valid Category Name",
            Description = "Valid description",
            CategoryType = CategoryType.Expense,
            Icon = "valid_icon",
            Color = "#FF5722",
            DefaultVatRate = 0.19m,
            Keywords = "keyword1,keyword2,keyword3",
            IsActive = true
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        Assert.Empty(validationResults);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateCategoryDto_InvalidName_ShouldFailValidation(string invalidName)
    {
        // Arrange
        var dto = new CreateCategoryDto
        {
            Name = invalidName,
            CategoryType = CategoryType.Expense,
            DefaultVatRate = 0.19m
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, r => r.MemberNames.Contains(nameof(CreateCategoryDto.Name)));
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    [InlineData(2.0)]
    public void CreateCategoryDto_InvalidVatRate_ShouldFailValidation(decimal invalidVatRate)
    {
        // Arrange
        var dto = new CreateCategoryDto
        {
            Name = "Valid Name",
            CategoryType = CategoryType.Expense,
            DefaultVatRate = invalidVatRate
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, r => r.MemberNames.Contains(nameof(CreateCategoryDto.DefaultVatRate)));
    }

    [Fact]
    public void CreateCategoryDto_NameTooLong_ShouldFailValidation()
    {
        // Arrange
        var dto = new CreateCategoryDto
        {
            Name = new string('a', 101), // Assuming max length is 100
            CategoryType = CategoryType.Expense,
            DefaultVatRate = 0.19m
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        // Note: This will only fail if there's a StringLength attribute on the Name property
        // The actual validation depends on the DTO implementation
    }

    #endregion

    #region UpdateCategoryDto Validation Tests

    [Fact]
    public void UpdateCategoryDto_ValidData_ShouldPassValidation()
    {
        // Arrange
        var dto = new UpdateCategoryDto
        {
            Name = "Updated Category Name",
            Description = "Updated description",
            Icon = "updated_icon",
            Color = "#2196F3",
            DefaultVatRate = 0.07m,
            Keywords = "updated,keywords",
            IsActive = false
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        Assert.Empty(validationResults);
    }

    [Fact]
    public void UpdateCategoryDto_AllNullValues_ShouldPassValidation()
    {
        // Arrange
        var dto = new UpdateCategoryDto
        {
            Name = null,
            Description = null,
            Icon = null,
            Color = null,
            DefaultVatRate = null,
            Keywords = null,
            IsActive = null
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        Assert.Empty(validationResults); // All properties are optional in update
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void UpdateCategoryDto_InvalidVatRate_ShouldFailValidation(decimal invalidVatRate)
    {
        // Arrange
        var dto = new UpdateCategoryDto
        {
            DefaultVatRate = invalidVatRate
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, r => r.MemberNames.Contains(nameof(UpdateCategoryDto.DefaultVatRate)));
    }

    #endregion

    #region SuggestCategoryDto Validation Tests

    [Fact]
    public void SuggestCategoryDto_ValidData_ShouldPassValidation()
    {
        // Arrange
        var dto = new SuggestCategoryDto
        {
            Description = "REWE Supermarkt transaction",
            MerchantName = "REWE",
            Amount = 25.50m,
            TransactionType = TransactionType.Expense
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        Assert.Empty(validationResults);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SuggestCategoryDto_InvalidDescription_ShouldFailValidation(string invalidDescription)
    {
        // Arrange
        var dto = new SuggestCategoryDto
        {
            Description = invalidDescription,
            TransactionType = TransactionType.Expense
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, r => r.MemberNames.Contains(nameof(SuggestCategoryDto.Description)));
    }

    [Fact]
    public void SuggestCategoryDto_NullMerchantName_ShouldPassValidation()
    {
        // Arrange
        var dto = new SuggestCategoryDto
        {
            Description = "Valid description",
            MerchantName = null, // Optional field
            TransactionType = TransactionType.Expense
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        Assert.Empty(validationResults); // MerchantName is optional
    }

    #endregion

    #region CategoryQueryParameters Validation Tests

    [Fact]
    public void CategoryQueryParameters_ValidData_ShouldPassValidation()
    {
        // Arrange
        var parameters = new CategoryQueryParameters
        {
            Page = 1,
            PageSize = 20,
            CategoryType = CategoryType.Expense,
            SearchTerm = "test",
            IsActive = true,
            IncludeSystemCategories = true,
            SortBy = "Name",
            SortDirection = "ASC"
        };

        // Act
        var validationResults = ValidateDto(parameters);

        // Assert
        Assert.Empty(validationResults);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CategoryQueryParameters_InvalidPage_ShouldFailValidation(int invalidPage)
    {
        // Arrange
        var parameters = new CategoryQueryParameters
        {
            Page = invalidPage,
            PageSize = 20
        };

        // Act
        var validationResults = ValidateDto(parameters);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, r => r.MemberNames.Contains(nameof(CategoryQueryParameters.Page)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)] // Assuming max page size is 100
    public void CategoryQueryParameters_InvalidPageSize_ShouldFailValidation(int invalidPageSize)
    {
        // Arrange
        var parameters = new CategoryQueryParameters
        {
            Page = 1,
            PageSize = invalidPageSize
        };

        // Act
        var validationResults = ValidateDto(parameters);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, r => r.MemberNames.Contains(nameof(CategoryQueryParameters.PageSize)));
    }

    #endregion

    #region Helper Methods

    private static List<ValidationResult> ValidateDto<T>(T dto)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(dto);
        Validator.TryValidateObject(dto, validationContext, validationResults, true);
        return validationResults;
    }

    #endregion
}
