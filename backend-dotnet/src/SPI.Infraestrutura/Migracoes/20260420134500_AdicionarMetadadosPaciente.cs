using SPI.Infrastructure.Data.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SPI.Infrastructure.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260420134500_AdicionarMetadadosPaciente")]
public partial class AddPatientMetadata : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF COL_LENGTH(N'dbo.patients', N'cpf') IS NULL
            BEGIN
                ALTER TABLE dbo.patients ADD cpf NVARCHAR(11) NOT NULL CONSTRAINT DF_patients_cpf DEFAULT (N'');
            END;

            IF COL_LENGTH(N'dbo.patients', N'data_nascimento') IS NULL
            BEGIN
                ALTER TABLE dbo.patients ADD data_nascimento DATE NOT NULL CONSTRAINT DF_patients_data_nascimento DEFAULT ('2000-01-01');
            END;

            IF COL_LENGTH(N'dbo.patients', N'sexo') IS NULL
            BEGIN
                ALTER TABLE dbo.patients ADD sexo NVARCHAR(20) NOT NULL CONSTRAINT DF_patients_sexo DEFAULT (N'outro');
            END;

            IF COL_LENGTH(N'dbo.patients', N'telefone') IS NULL
            BEGIN
                ALTER TABLE dbo.patients ADD telefone NVARCHAR(30) NULL;
            END;

            IF COL_LENGTH(N'dbo.patients', N'email') IS NULL
            BEGIN
                ALTER TABLE dbo.patients ADD email NVARCHAR(200) NULL;
            END;

            IF COL_LENGTH(N'dbo.patients', N'endereco') IS NULL
            BEGIN
                ALTER TABLE dbo.patients ADD endereco NVARCHAR(500) NULL;
            END;

            IF COL_LENGTH(N'dbo.patients', N'observacoes') IS NULL
            BEGIN
                ALTER TABLE dbo.patients ADD observacoes NVARCHAR(2000) NULL;
            END;

            IF COL_LENGTH(N'dbo.patients', N'documentos') IS NULL
            BEGIN
                ALTER TABLE dbo.patients ADD documentos NVARCHAR(4000) NULL;
            END;

            IF COL_LENGTH(N'dbo.patients', N'historico') IS NULL
            BEGIN
                ALTER TABLE dbo.patients ADD historico NVARCHAR(4000) NULL;
            END;

            IF NOT EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = N'IX_patients_cpf'
                  AND object_id = OBJECT_ID(N'dbo.patients')
            )
            BEGIN
                EXEC(N'CREATE UNIQUE INDEX IX_patients_cpf ON dbo.patients(cpf) WHERE cpf <> N'''';');
            END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = N'IX_patients_cpf'
                  AND object_id = OBJECT_ID(N'dbo.patients')
            )
            BEGIN
                DROP INDEX IX_patients_cpf ON dbo.patients;
            END;

            IF COL_LENGTH(N'dbo.patients', N'historico') IS NOT NULL
            BEGIN
                ALTER TABLE dbo.patients DROP COLUMN historico;
            END;

            IF COL_LENGTH(N'dbo.patients', N'documentos') IS NOT NULL
            BEGIN
                ALTER TABLE dbo.patients DROP COLUMN documentos;
            END;

            IF COL_LENGTH(N'dbo.patients', N'observacoes') IS NOT NULL
            BEGIN
                ALTER TABLE dbo.patients DROP COLUMN observacoes;
            END;

            IF COL_LENGTH(N'dbo.patients', N'endereco') IS NOT NULL
            BEGIN
                ALTER TABLE dbo.patients DROP COLUMN endereco;
            END;

            IF COL_LENGTH(N'dbo.patients', N'email') IS NOT NULL
            BEGIN
                ALTER TABLE dbo.patients DROP COLUMN email;
            END;

            IF COL_LENGTH(N'dbo.patients', N'telefone') IS NOT NULL
            BEGIN
                ALTER TABLE dbo.patients DROP COLUMN telefone;
            END;

            IF COL_LENGTH(N'dbo.patients', N'sexo') IS NOT NULL
            BEGIN
                ALTER TABLE dbo.patients DROP COLUMN sexo;
            END;

            IF COL_LENGTH(N'dbo.patients', N'data_nascimento') IS NOT NULL
            BEGIN
                ALTER TABLE dbo.patients DROP COLUMN data_nascimento;
            END;

            IF COL_LENGTH(N'dbo.patients', N'cpf') IS NOT NULL
            BEGIN
                ALTER TABLE dbo.patients DROP COLUMN cpf;
            END;
            """);
    }
}
