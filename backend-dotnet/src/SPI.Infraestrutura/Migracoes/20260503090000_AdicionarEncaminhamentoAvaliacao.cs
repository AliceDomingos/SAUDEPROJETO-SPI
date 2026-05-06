using SPI.Infrastructure.Data.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SPI.Infrastructure.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260503090000_AdicionarEncaminhamentoAvaliacao")]
public partial class AddEvaluationReferral : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF OBJECT_ID(N'dbo.evaluation_referrals', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.evaluation_referrals (
                    id INT IDENTITY(1,1) NOT NULL,
                    evaluation_id INT NOT NULL,
                    patient_id INT NOT NULL,
                    encaminhado BIT NOT NULL,
                    especialidade NVARCHAR(120) NULL,
                    custo_estimado DECIMAL(10,2) NOT NULL CONSTRAINT DF_evaluation_referrals_custo_estimado DEFAULT ((0)),
                    criado_em DATETIME2 NOT NULL CONSTRAINT DF_evaluation_referrals_criado_em DEFAULT (SYSUTCDATETIME()),
                    criado_por_usuario_id INT NOT NULL,
                    organization_id INT NULL,
                    CONSTRAINT PK_evaluation_referrals PRIMARY KEY (id),
                    CONSTRAINT FK_evaluation_referrals_evaluations_evaluation_id FOREIGN KEY (evaluation_id) REFERENCES dbo.evaluations(id) ON DELETE CASCADE,
                    CONSTRAINT FK_evaluation_referrals_patients_patient_id FOREIGN KEY (patient_id) REFERENCES dbo.patients(id),
                    CONSTRAINT FK_evaluation_referrals_users_criado_por_usuario_id FOREIGN KEY (criado_por_usuario_id) REFERENCES dbo.users(id),
                    CONSTRAINT FK_evaluation_referrals_organizations_organization_id FOREIGN KEY (organization_id) REFERENCES dbo.organizations(id)
                );

                EXEC(N'CREATE UNIQUE INDEX IX_evaluation_referrals_evaluation_id ON dbo.evaluation_referrals(evaluation_id);');
                EXEC(N'CREATE INDEX IX_evaluation_referrals_patient_id ON dbo.evaluation_referrals(patient_id);');
                EXEC(N'CREATE INDEX IX_evaluation_referrals_especialidade ON dbo.evaluation_referrals(especialidade);');
            END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF OBJECT_ID(N'dbo.evaluation_referrals', N'U') IS NOT NULL
            BEGIN
                DROP TABLE dbo.evaluation_referrals;
            END;
            """);
    }
}
