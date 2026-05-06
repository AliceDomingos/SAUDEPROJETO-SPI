using SPI.Infrastructure.Data.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SPI.Infrastructure.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260503103000_AdicionarObservacoesAvaliacao")]
public partial class AddEvaluationObservations : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF COL_LENGTH(N'dbo.evaluations', N'observacoes') IS NULL
            BEGIN
                ALTER TABLE dbo.evaluations ADD observacoes NVARCHAR(2000) NULL;
            END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF COL_LENGTH(N'dbo.evaluations', N'observacoes') IS NOT NULL
            BEGIN
                ALTER TABLE dbo.evaluations DROP COLUMN observacoes;
            END;
            """);
    }
}
