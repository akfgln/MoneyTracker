using AutoMapper;
using MoneyTracker.Application.DTOs.Category;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Application.Common.Mappings;

public class CategoryMappingProfile : Profile
{
    public CategoryMappingProfile()
    {
        // Entity to DTO mappings
        CreateMap<Category, CategoryResponseDto>()
            .ForMember(dest => dest.CategoryTypeName, opt => opt.Ignore()) // Set in service
            .ForMember(dest => dest.ParentCategoryName, opt => opt.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.DisplayName : null))
            .ForMember(dest => dest.TransactionCount, opt => opt.Ignore()) // Set in service
            .ForMember(dest => dest.TotalAmount, opt => opt.Ignore()) // Set in service
            .ForMember(dest => dest.SubCategories, opt => opt.Ignore()); // Handled separately in hierarchy building

        // DTO to Entity mappings
        CreateMap<CreateCategoryDto, Category>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.IsSystemCategory, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.ParentCategory, opt => opt.Ignore())
            .ForMember(dest => dest.SubCategories, opt => opt.Ignore())
            .ForMember(dest => dest.Transactions, opt => opt.Ignore())
            .ForMember(dest => dest.VatRate, opt => opt.Ignore())
            .ForMember(dest => dest.BudgetLimitMoney, opt => opt.Ignore())
            .ForMember(dest => dest.DisplayName, opt => opt.Ignore())
            .ForMember(dest => dest.DisplayDescription, opt => opt.Ignore())
            .ForMember(dest => dest.IsParentCategory, opt => opt.Ignore())
            .ForMember(dest => dest.IsSubCategory, opt => opt.Ignore())
            .ForMember(dest => dest.Level, opt => opt.Ignore())
            .ForMember(dest => dest.FullPath, opt => opt.Ignore())
            .ForMember(dest => dest.NameGerman, opt => opt.Ignore())
            .ForMember(dest => dest.DescriptionGerman, opt => opt.Ignore())
            .ForMember(dest => dest.IsDefault, opt => opt.Ignore())
            .ForMember(dest => dest.BudgetLimit, opt => opt.Ignore())
            .ForMember(dest => dest.BudgetCurrency, opt => opt.Ignore())
            .ForMember(dest => dest.SortOrder, opt => opt.Ignore());

        CreateMap<UpdateCategoryDto, Category>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<ImportCategoryItemDto, CreateCategoryDto>();

        // Usage stats mappings
        CreateMap<MonthlyUsageDto, MonthlyUsageDto>();
        CreateMap<CategoryUsageStatsDto, CategoryUsageStatsDto>();

        // Suggestion mappings
        CreateMap<Category, CategorySuggestionDto>()
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.DisplayName))
            .ForMember(dest => dest.CategoryIcon, opt => opt.MapFrom(src => src.Icon))
            .ForMember(dest => dest.CategoryColor, opt => opt.MapFrom(src => src.Color))
            .ForMember(dest => dest.ConfidenceScore, opt => opt.Ignore())
            .ForMember(dest => dest.MatchReason, opt => opt.Ignore());

        // Hierarchy mappings
        CreateMap<CategoryType, CategoryHierarchyDto>()
            .ForMember(dest => dest.CategoryType, opt => opt.MapFrom(src => src))
            .ForMember(dest => dest.CategoryTypeName, opt => opt.Ignore()) // Set in service
            .ForMember(dest => dest.Categories, opt => opt.Ignore()); // Set in service
    }
}