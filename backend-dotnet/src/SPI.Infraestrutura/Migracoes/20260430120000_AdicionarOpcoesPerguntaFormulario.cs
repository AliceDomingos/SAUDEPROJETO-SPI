using SPI.Infrastructure.Data.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SPI.Infrastructure.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260430120000_AdicionarOpcoesPerguntaFormulario")]
public partial class AddFormQuestionOptions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF OBJECT_ID(N'dbo.form_question_options', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.form_question_options
                (
                    id              INT IDENTITY(1,1) NOT NULL,
                    form_question_id INT NOT NULL,
                    valor           INT NOT NULL,
                    descricao       NVARCHAR(500) NOT NULL,
                    CONSTRAINT PK_form_question_options PRIMARY KEY (id),
                    CONSTRAINT FK_form_question_options_form_questions
                        FOREIGN KEY (form_question_id)
                        REFERENCES dbo.form_questions(id)
                        ON DELETE CASCADE
                );
            END;

            IF NOT EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = N'IX_form_question_options_form_question_id'
                  AND object_id = OBJECT_ID(N'dbo.form_question_options')
            )
            BEGIN
                CREATE INDEX IX_form_question_options_form_question_id
                    ON dbo.form_question_options(form_question_id);
            END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF OBJECT_ID(N'dbo.form_question_options', N'U') IS NOT NULL
                DROP TABLE dbo.form_question_options;
            """);
    }
}
