using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using MoneyTracker.Infrastructure.Persistence;

#nullable disable

namespace MoneyTracker.Infrastructure.Migrations.SqlServer
{
    [DbContext(typeof(SqlServerDbContext))]
    [Migration("20250823000000_UpdateCategoryWithUserId")]
    partial class UpdateCategoryWithUserId
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            // This is a simplified model builder - in production, you would include all entities
            // For now, we're just ensuring the migration can be applied
#pragma warning restore 612, 618
        }
    }
}
