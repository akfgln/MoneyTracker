using Microsoft.EntityFrameworkCore;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Infrastructure.Persistence.Seeders;

public static class CategorySeeder
{
    public static async Task SeedGermanCategoriesAsync(DbContext context)
    {
        var categories = context.Set<Category>();
        
        // Check if categories already exist
        if (await categories.AnyAsync(c => c.IsDefault))
        {
            return; // Already seeded
        }

        // Create income categories
        var incomeCategories = new List<Category>
        {
            new Category
            {
                Name = "Salary/Wages",
                NameGerman = "Gehalt/Lohn",
                Description = "Regular employment income",
                DescriptionGerman = "Regelmäßiges Arbeitseinkommen",
                CategoryType = CategoryType.Income,
                DefaultVatRate = 0.00m,
                IsSystemCategory = true,
                IsDefault = true,
                SortOrder = 1,
                Icon = "💰",
                Color = "#22C55E",
                Keywords = "gehalt,lohn,salary,wage,income,einkommen"
            },
            new Category
            {
                Name = "Freelance Income",
                NameGerman = "Freiberufliche Einkünfte",
                Description = "Income from freelance work",
                DescriptionGerman = "Einkünfte aus freiberuflicher Tätigkeit",
                CategoryType = CategoryType.Income,
                DefaultVatRate = 0.19m,
                IsSystemCategory = true,
                IsDefault = true,
                SortOrder = 2,
                Icon = "💼",
                Color = "#3B82F6",
                Keywords = "freelance,freiberuflich,consulting,beratung,selbständig"
            },
            new Category
            {
                Name = "Investment Returns",
                NameGerman = "Kapitalerträge",
                Description = "Dividends, interest, and capital gains",
                DescriptionGerman = "Dividenden, Zinsen und Kursgewinne",
                CategoryType = CategoryType.Income,
                DefaultVatRate = 0.00m,
                IsSystemCategory = true,
                IsDefault = true,
                SortOrder = 3,
                Icon = "📈",
                Color = "#10B981",
                Keywords = "dividend,dividende,zinsen,interest,aktien,stocks,investment"
            },
            new Category
            {
                Name = "Rental Income",
                NameGerman = "Mieteinnahmen",
                Description = "Income from rental properties",
                DescriptionGerman = "Einkünfte aus Vermietung und Verpachtung",
                CategoryType = CategoryType.Income,
                DefaultVatRate = 0.00m,
                IsSystemCategory = true,
                IsDefault = true,
                SortOrder = 4,
                Icon = "🏠",
                Color = "#8B5CF6",
                Keywords = "miete,rent,vermietung,immobilie,property"
            },
            new Category
            {
                Name = "Business Income",
                NameGerman = "Geschäftseinkünfte",
                Description = "Income from business operations",
                DescriptionGerman = "Einkünfte aus Geschäftstätigkeit",
                CategoryType = CategoryType.Income,
                DefaultVatRate = 0.19m,
                IsSystemCategory = true,
                IsDefault = true,
                SortOrder = 5,
                Icon = "🏢",
                Color = "#F59E0B",
                Keywords = "business,geschäft,unternehmen,firma,verkauf,sales"
            },
            new Category
            {
                Name = "Other Income",
                NameGerman = "Sonstige Einnahmen",
                Description = "All other income sources",
                DescriptionGerman = "Alle anderen Einnahmequellen",
                CategoryType = CategoryType.Income,
                DefaultVatRate = 0.00m,
                IsSystemCategory = true,
                IsDefault = true,
                SortOrder = 99,
                Icon = "💎",
                Color = "#6B7280",
                Keywords = "other,sonstige,verschiedenes,misc"
            }
        };

        // Create expense categories
        var expenseCategories = new List<Category>
        {
            new Category
            {
                Name = "Housing",
                NameGerman = "Wohnen",
                Description = "Rent, utilities, maintenance",
                DescriptionGerman = "Miete, Nebenkosten, Instandhaltung",
                CategoryType = CategoryType.Expense,
                DefaultVatRate = 0.00m,
                IsSystemCategory = true,
                IsDefault = true,
                SortOrder = 1,
                Icon = "🏠",
                Color = "#EF4444",
                Keywords = "miete,rent,nebenkosten,utilities,wohnung,apartment,haus,house",
                BudgetLimit = 1200.00m
            },
            new Category
            {
                Name = "Transportation",
                NameGerman = "Transport",
                Description = "Fuel, public transport, car maintenance",
                DescriptionGerman = "Kraftstoff, ÖPNV, Autowartung",
                CategoryType = CategoryType.Expense,
                DefaultVatRate = 0.19m,
                IsSystemCategory = true,
                IsDefault = true,
                SortOrder = 2,
                Icon = "🚗",
                Color = "#F97316",
                Keywords = "transport,verkehr,auto,car,bus,bahn,train,taxi,benzin,fuel",
                BudgetLimit = 400.00m
            },
            new Category
            {
                Name = "Food & Dining",
                NameGerman = "Essen & Trinken",
                Description = "Groceries, restaurants, dining out",
                DescriptionGerman = "Lebensmittel, Restaurants, Außer-Haus-Essen",
                CategoryType = CategoryType.Expense,
                DefaultVatRate = 0.07m,
                IsSystemCategory = true,
                IsDefault = true,
                SortOrder = 3,
                Icon = "🍽️",
                Color = "#84CC16",
                Keywords = "food,essen,restaurant,supermarket,groceries,lebensmittel,trinken,drink",
                BudgetLimit = 500.00m
            },
            new Category
            {
                Name = "Healthcare",
                NameGerman = "Gesundheit",
                Description = "Medical expenses, insurance, pharmacy",
                DescriptionGerman = "Medizinische Ausgaben, Versicherung, Apotheke",
                CategoryType = CategoryType.Expense,
                DefaultVatRate = 0.00m,
                IsSystemCategory = true,
                IsDefault = true,
                SortOrder = 4,
                Icon = "🏥",
                Color = "#EC4899",
                Keywords = "health,gesundheit,arzt,doctor,apotheke,pharmacy,medizin,medicine",
                BudgetLimit = 200.00m
            },
            new Category
            {
                Name = "Entertainment",
                NameGerman = "Unterhaltung",
                Description = "Movies, sports, hobbies, leisure",
                DescriptionGerman = "Kino, Sport, Hobbys, Freizeit",
                CategoryType = CategoryType.Expense,
                DefaultVatRate = 0.19m,
                IsSystemCategory = true,
                IsDefault = true,
                SortOrder = 5,
                Icon = "🎬",
                Color = "#8B5CF6",
                Keywords = "entertainment,unterhaltung,kino,cinema,sport,hobby,freizeit,leisure",
                BudgetLimit = 300.00m
            },
            new Category
            {
                Name = "Shopping",
                NameGerman = "Einkaufen",
                Description = "Clothing, electronics, personal items",
                DescriptionGerman = "Kleidung, Elektronik, persönliche Gegenstände",
                CategoryType = CategoryType.Expense,
                DefaultVatRate = 0.19m,
                IsSystemCategory = true,
                IsDefault = true,
                SortOrder = 6,
                Icon = "🛍️",
                Color = "#F59E0B",
                Keywords = "shopping,einkaufen,kleidung,clothes,elektronik,electronics,amazon,ebay",
                BudgetLimit = 250.00m
            },
            new Category
            {
                Name = "Education",
                NameGerman = "Bildung",
                Description = "Courses, books, training, education",
                DescriptionGerman = "Kurse, Bücher, Fortbildung, Bildung",
                CategoryType = CategoryType.Expense,
                DefaultVatRate = 0.00m,
                IsSystemCategory = true,
                IsDefault = true,
                SortOrder = 7,
                Icon = "📚",
                Color = "#06B6D4",
                Keywords = "education,bildung,course,kurs,book,buch,training,fortbildung,university",
                BudgetLimit = 150.00m
            },
            new Category
            {
                Name = "Business Expenses",
                NameGerman = "Geschäftsausgaben",
                Description = "Office supplies, software, business travel",
                DescriptionGerman = "Bürobedarf, Software, Geschäftsreisen",
                CategoryType = CategoryType.Expense,
                DefaultVatRate = 0.19m,
                IsSystemCategory = true,
                IsDefault = true,
                SortOrder = 8,
                Icon = "💼",
                Color = "#64748B",
                Keywords = "business,geschäft,office,büro,software,travel,reise,supplies",
                BudgetLimit = 200.00m
            },
            new Category
            {
                Name = "Insurance",
                NameGerman = "Versicherungen",
                Description = "Health, life, property insurance",
                DescriptionGerman = "Kranken-, Lebens-, Sachversicherungen",
                CategoryType = CategoryType.Expense,
                DefaultVatRate = 0.00m,
                IsSystemCategory = true,
                IsDefault = true,
                SortOrder = 9,
                Icon = "🛡️",
                Color = "#0EA5E9",
                Keywords = "insurance,versicherung,krankenversicherung,lebensversicherung,haftpflicht",
                BudgetLimit = 300.00m
            },
            new Category
            {
                Name = "Other Expenses",
                NameGerman = "Sonstige Ausgaben",
                Description = "All other expenses",
                DescriptionGerman = "Alle anderen Ausgaben",
                CategoryType = CategoryType.Expense,
                DefaultVatRate = 0.19m,
                IsSystemCategory = true,
                IsDefault = true,
                SortOrder = 99,
                Icon = "📦",
                Color = "#6B7280",
                Keywords = "other,sonstige,verschiedenes,misc",
                BudgetLimit = 100.00m
            }
        };

        // Set creation metadata
        var now = DateTime.UtcNow;
        var allCategories = incomeCategories.Concat(expenseCategories);
        
        foreach (var category in allCategories)
        {
            category.CreatedAt = now;
            category.CreatedBy = "System";
            category.IsActive = true;
        }

        // Add categories to context
        await categories.AddRangeAsync(allCategories);
        await context.SaveChangesAsync();
    }

    public static async Task SeedTestUserAsync(DbContext context)
    {
        var users = context.Set<User>();
        
        // Check if test user already exists
        if (await users.AnyAsync(u => u.Email == "test@moneytracker.de"))
        {
            return; // Already exists
        }

        var testUser = new User
        {
            Email = "test@moneytracker.de",
            FirstName = "Max",
            LastName = "Mustermann",
            PasswordHash = "$2a$11$dummyPasswordHashForTesting", // This should be properly hashed in real usage
            EmailConfirmed = true,
            PreferredLanguage = "de-DE",
            PreferredCurrency = "EUR",
            Country = "Germany",
            City = "Berlin",
            PostalCode = "10115",
            AcceptedTerms = true,
            AcceptedPrivacyPolicy = true,
            TermsAcceptedDate = DateTime.UtcNow,
            PrivacyPolicyAcceptedDate = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };

        await users.AddAsync(testUser);
        await context.SaveChangesAsync();
    }
}