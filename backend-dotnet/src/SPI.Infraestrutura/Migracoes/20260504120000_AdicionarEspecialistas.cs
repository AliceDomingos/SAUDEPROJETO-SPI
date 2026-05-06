using SPI.Infrastructure.Data.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SPI.Infrastructure.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260504120000_AdicionarEspecialistas")]
public partial class AddSpecialists : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF OBJECT_ID(N'dbo.specialists', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.specialists (
                    id INT IDENTITY(1,1) NOT NULL,
                    nome NVARCHAR(200) NOT NULL,
                    especialidade NVARCHAR(120) NOT NULL,
                    custo_consulta DECIMAL(10,2) NOT NULL,
                    ativo BIT NOT NULL CONSTRAINT DF_specialists_ativo DEFAULT ((1)),
                    criado_em DATETIME2 NOT NULL CONSTRAINT DF_specialists_criado_em DEFAULT (SYSUTCDATETIME()),
                    organization_id INT NULL,
                    CONSTRAINT PK_specialists PRIMARY KEY (id),
                    CONSTRAINT FK_specialists_organizations_organization_id FOREIGN KEY (organization_id) REFERENCES dbo.organizations(id)
                );

                EXEC(N'CREATE INDEX IX_specialists_nome ON dbo.specialists(nome);');
                EXEC(N'CREATE INDEX IX_specialists_especialidade ON dbo.specialists(especialidade);');
                EXEC(N'CREATE INDEX IX_specialists_organization_id ON dbo.specialists(organization_id);');
            END;

            IF COL_LENGTH(N'dbo.evaluation_referrals', N'specialist_id') IS NULL
            BEGIN
                ALTER TABLE dbo.evaluation_referrals ADD specialist_id INT NULL;
                ALTER TABLE dbo.evaluation_referrals ADD specialist_nome NVARCHAR(200) NULL;
                ALTER TABLE dbo.evaluation_referrals
                    ADD CONSTRAINT FK_evaluation_referrals_specialists_specialist_id
                    FOREIGN KEY (specialist_id) REFERENCES dbo.specialists(id);
                EXEC(N'CREATE INDEX IX_evaluation_referrals_specialist_id ON dbo.evaluation_referrals(specialist_id);');
            END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF COL_LENGTH(N'dbo.evaluation_referrals', N'specialist_id') IS NOT NULL
            BEGIN
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_evaluation_referrals_specialists_specialist_id')
                    ALTER TABLE dbo.evaluation_referrals DROP CONSTRAINT FK_evaluation_referrals_specialists_specialist_id;

                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_evaluation_referrals_specialist_id' AND object_id = OBJECT_ID(N'dbo.evaluation_referrals'))
                    DROP INDEX IX_evaluation_referrals_specialist_id ON dbo.evaluation_referrals;

                ALTER TABLE dbo.evaluation_referrals DROP COLUMN specialist_id;
                ALTER TABLE dbo.evaluation_referrals DROP COLUMN specialist_nome;
            END;

            IF OBJECT_ID(N'dbo.specialists', N'U') IS NOT NULL
            BEGIN
                DROP TABLE dbo.specialists;
            END;
            """);
    }
}
