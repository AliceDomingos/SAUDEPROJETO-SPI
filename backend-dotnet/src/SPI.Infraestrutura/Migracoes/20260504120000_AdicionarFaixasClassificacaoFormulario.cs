using SPI.Infrastructure.Data.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SPI.Infrastructure.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260504120000_AdicionarFaixasClassificacaoFormulario")]
public partial class AddFormClassificationRanges : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF OBJECT_ID(N'dbo.form_classification_ranges', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.form_classification_ranges
                (
                    id               INT IDENTITY(1,1) NOT NULL,
                    form_template_id INT NOT NULL,
                    score_min        DECIMAL(10,2) NOT NULL,
                    score_max        DECIMAL(10,2) NOT NULL,
                    rotulo           NVARCHAR(200) NOT NULL,
                    CONSTRAINT PK_form_classification_ranges PRIMARY KEY (id),
                    CONSTRAINT FK_form_classification_ranges_form_templates
                        FOREIGN KEY (form_template_id)
                        REFERENCES dbo.form_templates(id)
                        ON DELETE CASCADE
                );
            END;

            IF NOT EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = N'IX_form_classification_ranges_form_template_id'
                  AND object_id = OBJECT_ID(N'dbo.form_classification_ranges')
            )
            BEGIN
                CREATE INDEX IX_form_classification_ranges_form_template_id
                    ON dbo.form_classification_ranges(form_template_id);
            END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF OBJECT_ID(N'dbo.form_classification_ranges', N'U') IS NOT NULL
                DROP TABLE dbo.form_classification_ranges;
            """);
    }
}
