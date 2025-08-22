using AutoMapper;
using MoneyTracker.Application.DTOs.Transaction;
using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Application.Common.Mappings;

public class TransactionMappingProfile : Profile
{
    public TransactionMappingProfile()
    {
        // Transaction Entity to TransactionResponseDto
        CreateMap<Transaction, TransactionResponseDto>()
            .ForMember(dest => dest.AccountName, opt => opt.Ignore()) // Will be set manually in service
            .ForMember(dest => dest.CategoryName, opt => opt.Ignore()) // Will be set manually in service
            .ForMember(dest => dest.CategoryIcon, opt => opt.Ignore()) // Will be set manually in service
            .ForMember(dest => dest.CategoryColor, opt => opt.Ignore()) // Will be set manually in service
            .ForMember(dest => dest.VatRateDescription, opt => opt.Ignore()) // Will be set manually in service
            .ForMember(dest => dest.TransactionTypeDisplay, opt => opt.Ignore()) // Will be set manually in service
            .ForMember(dest => dest.PaymentMethodDisplay, opt => opt.Ignore()) // Will be set manually in service
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.TagsList))
            .ForMember(dest => dest.DisplayAmount, opt => opt.Ignore()) // Will be set manually in service
            .ForMember(dest => dest.DisplayDescription, opt => opt.Ignore()) // Will be set manually in service
            .ForMember(dest => dest.DaysFromToday, opt => opt.Ignore()); // Will be set manually in service

        // CreateTransactionDto to Transaction Entity
        CreateMap<CreateTransactionDto, Transaction>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Will be set in service
            .ForMember(dest => dest.UserId, opt => opt.Ignore()) // Will be set in service
            .ForMember(dest => dest.NetAmount, opt => opt.Ignore()) // Will be calculated in service
            .ForMember(dest => dest.VatAmount, opt => opt.Ignore()) // Will be calculated in service
            .ForMember(dest => dest.VatRate, opt => opt.Ignore()) // Will be calculated in service
            .ForMember(dest => dest.BookingDate, opt => opt.MapFrom(src => src.BookingDate ?? src.TransactionDate))
            .ForMember(dest => dest.TagsList, opt => opt.MapFrom(src => src.Tags ?? new List<string>()))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Will be set in service
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()) // Will be set in service
            .ForMember(dest => dest.IsVerified, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.IsPending, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Account, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.VerifiedDate, opt => opt.Ignore())
            .ForMember(dest => dest.VerifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.ProcessedDate, opt => opt.Ignore());

        // UpdateTransactionDto to Transaction Entity (partial mapping for updates)
        CreateMap<UpdateTransactionDto, Transaction>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.NetAmount, opt => opt.Ignore()) // Will be recalculated in service
            .ForMember(dest => dest.VatAmount, opt => opt.Ignore()) // Will be recalculated in service
            .ForMember(dest => dest.VatRate, opt => opt.Ignore()) // Will be recalculated in service
            .ForMember(dest => dest.TagsList, opt => opt.MapFrom(src => src.Tags))
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Account, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.VerifiedDate, opt => opt.Ignore())
            .ForMember(dest => dest.VerifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.ProcessedDate, opt => opt.Ignore())
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember, destMember) => 
            {
                // Only map if the source value is not null (for update scenarios)
                return srcMember != null;
            }));

        // VatCalculationResult mappings
        CreateMap<VatCalculationResult, VatCalculationResult>()
            .ReverseMap();

        // Query parameter mappings
        CreateMap<TransactionQueryParameters, TransactionQueryParameters>()
            .ReverseMap();

        CreateMap<TransactionSearchDto, TransactionSearchDto>()
            .ReverseMap();

        CreateMap<SummaryQueryParameters, SummaryQueryParameters>()
            .ReverseMap();

        // Summary DTOs
        CreateMap<TransactionSummaryDto, TransactionSummaryDto>()
            .ReverseMap();

        CreateMap<CategorySummaryDto, CategorySummaryDto>()
            .ReverseMap();

        CreateMap<AccountSummaryDto, AccountSummaryDto>()
            .ReverseMap();

        CreateMap<MonthlySummaryDto, MonthlySummaryDto>()
            .ReverseMap();

        CreateMap<VatSummaryDto, VatSummaryDto>()
            .ReverseMap();

        CreateMap<VatRateSummaryDto, VatRateSummaryDto>()
            .ReverseMap();
    }
}