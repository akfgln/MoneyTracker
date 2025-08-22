using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pomelo.EntityFrameworkCore.MySql.Metadata;

#nullable disable

namespace MoneyTracker.Infrastructure.Migrations.MySql
{
    /// <inheritdoc />
    public partial class AddUploadedFileEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UploadedFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    OriginalFileName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_0900_ai_ci"),
                    StoredFileName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_0900_ai_ci"),
                    FilePath = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false, collation: "utf8mb4_0900_ai_ci"),
                    ContentType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci"),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FileType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ProcessingMessage = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true, collation: "utf8mb4_0900_ai_ci"),
                    UploadDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ProcessedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    TransactionId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    AccountId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ExtractedText = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_0900_ai_ci"),
                    ExtractedDataJson = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_0900_ai_ci"),
                    Tags = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true, collation: "utf8mb4_0900_ai_ci"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    BankName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci"),
                    StatementPeriodStart = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    StatementPeriodEnd = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    VirusScanResult = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true, collation: "utf8mb4_0900_ai_ci"),
                    VirusScanDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci"),
                    UpdatedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci"),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci"),
                    RowVersion = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadedFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UploadedFiles_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UploadedFiles_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UploadedFiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFiles_AccountId",
                table: "UploadedFiles",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFiles_FileType",
                table: "UploadedFiles",
                column: "FileType");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFiles_Status",
                table: "UploadedFiles",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFiles_TransactionId",
                table: "UploadedFiles",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFiles_UploadDate",
                table: "UploadedFiles",
                column: "UploadDate");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFiles_UserId",
                table: "UploadedFiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFiles_UserId_FileType",
                table: "UploadedFiles",
                columns: new[] { "UserId", "FileType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UploadedFiles");
        }
    }
}
