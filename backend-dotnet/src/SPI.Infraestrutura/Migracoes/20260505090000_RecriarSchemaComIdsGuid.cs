using SPI.Infrastructure.Data.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SPI.Infrastructure.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260505090000_RecriarSchemaComIdsGuid")]
public partial class RecreateSchemaWithGuidIds : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DECLARE @sql NVARCHAR(MAX) = N'';

            SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + N'.' + QUOTENAME(OBJECT_NAME(parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(name) + N';' + CHAR(13)
            FROM sys.foreign_keys
            WHERE parent_object_id IN (
                    OBJECT_ID(N'dbo.evaluation_referrals'),
                    OBJECT_ID(N'dbo.evaluations'),
                    OBJECT_ID(N'dbo.form_questions'),
                    OBJECT_ID(N'dbo.form_templates'),
                    OBJECT_ID(N'dbo.patients'),
                    OBJECT_ID(N'dbo.user_group_memberships'),
                    OBJECT_ID(N'dbo.groups'),
                    OBJECT_ID(N'dbo.organizations'),
                    OBJECT_ID(N'dbo.users'),
                    OBJECT_ID(N'dbo.specialists')
                )
                OR referenced_object_id IN (
                    OBJECT_ID(N'dbo.evaluation_referrals'),
                    OBJECT_ID(N'dbo.evaluations'),
                    OBJECT_ID(N'dbo.form_questions'),
                    OBJECT_ID(N'dbo.form_templates'),
                    OBJECT_ID(N'dbo.patients'),
                    OBJECT_ID(N'dbo.user_group_memberships'),
                    OBJECT_ID(N'dbo.groups'),
                    OBJECT_ID(N'dbo.organizations'),
                    OBJECT_ID(N'dbo.users'),
                    OBJECT_ID(N'dbo.specialists')
                );

            IF @sql <> N''
                EXEC sp_executesql @sql;

            DROP TABLE IF EXISTS dbo.evaluation_referrals;
            DROP TABLE IF EXISTS dbo.evaluations;
            DROP TABLE IF EXISTS dbo.form_questions;
            DROP TABLE IF EXISTS dbo.form_templates;
            DROP TABLE IF EXISTS dbo.patients;
            DROP TABLE IF EXISTS dbo.user_group_memberships;
            DROP TABLE IF EXISTS dbo.groups;
            DROP TABLE IF EXISTS dbo.specialists;
            DROP TABLE IF EXISTS dbo.organizations;
            DROP TABLE IF EXISTS dbo.users;

            CREATE TABLE dbo.users (
                id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_users_id DEFAULT (NEWID()),
                nome NVARCHAR(200) NOT NULL,
                email NVARCHAR(200) NOT NULL,
                senha_hash NVARCHAR(256) NOT NULL,
                role NVARCHAR(30) NOT NULL,
                ativo BIT NOT NULL CONSTRAINT DF_users_ativo DEFAULT ((1)),
                criado_em DATETIME2 NOT NULL CONSTRAINT DF_users_criado_em DEFAULT (SYSUTCDATETIME()),
                organization_id UNIQUEIDENTIFIER NULL,
                CONSTRAINT PK_users PRIMARY KEY (id)
            );

            CREATE UNIQUE INDEX IX_users_email ON dbo.users(email);

            CREATE TABLE dbo.organizations (
                id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_organizations_id DEFAULT (NEWID()),
                nome NVARCHAR(200) NOT NULL,
                admin_id UNIQUEIDENTIFIER NOT NULL,
                criado_em DATETIME2 NOT NULL CONSTRAINT DF_organizations_criado_em DEFAULT (SYSUTCDATETIME()),
                CONSTRAINT PK_organizations PRIMARY KEY (id),
                CONSTRAINT FK_organizations_users_admin_id FOREIGN KEY (admin_id) REFERENCES dbo.users(id)
            );

            CREATE UNIQUE INDEX IX_organizations_admin_id ON dbo.organizations(admin_id);

            ALTER TABLE dbo.users
                ADD CONSTRAINT FK_users_organizations_organization_id
                FOREIGN KEY (organization_id) REFERENCES dbo.organizations(id);

            CREATE INDEX IX_users_organization_id ON dbo.users(organization_id);

            CREATE TABLE dbo.groups (
                id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_groups_id DEFAULT (NEWID()),
                nome NVARCHAR(200) NOT NULL,
                gestor_id UNIQUEIDENTIFIER NOT NULL,
                ativo BIT NOT NULL CONSTRAINT DF_groups_ativo DEFAULT ((1)),
                criado_em DATETIME2 NOT NULL CONSTRAINT DF_groups_criado_em DEFAULT (SYSUTCDATETIME()),
                organization_id UNIQUEIDENTIFIER NULL,
                CONSTRAINT PK_groups PRIMARY KEY (id),
                CONSTRAINT FK_groups_users_gestor_id FOREIGN KEY (gestor_id) REFERENCES dbo.users(id),
                CONSTRAINT FK_groups_organizations_organization_id FOREIGN KEY (organization_id) REFERENCES dbo.organizations(id)
            );

            CREATE INDEX IX_groups_gestor_id ON dbo.groups(gestor_id);
            CREATE INDEX IX_groups_organization_id ON dbo.groups(organization_id);

            CREATE TABLE dbo.user_group_memberships (
                user_id UNIQUEIDENTIFIER NOT NULL,
                group_id UNIQUEIDENTIFIER NOT NULL,
                criado_em DATETIME2 NOT NULL CONSTRAINT DF_user_group_memberships_criado_em DEFAULT (SYSUTCDATETIME()),
                CONSTRAINT PK_user_group_memberships PRIMARY KEY (user_id, group_id),
                CONSTRAINT FK_user_group_memberships_users_user_id FOREIGN KEY (user_id) REFERENCES dbo.users(id) ON DELETE CASCADE,
                CONSTRAINT FK_user_group_memberships_groups_group_id FOREIGN KEY (group_id) REFERENCES dbo.groups(id) ON DELETE CASCADE
            );

            CREATE INDEX IX_user_group_memberships_group_id ON dbo.user_group_memberships(group_id);

            CREATE TABLE dbo.patients (
                id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_patients_id DEFAULT (NEWID()),
                nome NVARCHAR(200) NOT NULL,
                cpf NVARCHAR(11) NOT NULL,
                data_nascimento DATE NOT NULL,
                sexo NVARCHAR(20) NOT NULL,
                idade INT NULL,
                avaliador_id UNIQUEIDENTIFIER NULL,
                nome_responsavel NVARCHAR(200) NULL,
                telefone NVARCHAR(30) NULL,
                email NVARCHAR(200) NULL,
                cep NVARCHAR(8) NULL,
                estado NVARCHAR(2) NULL,
                cidade NVARCHAR(120) NULL,
                bairro NVARCHAR(120) NULL,
                rua NVARCHAR(200) NULL,
                numero NVARCHAR(30) NULL,
                complemento NVARCHAR(200) NULL,
                observacoes NVARCHAR(2000) NULL,
                group_id UNIQUEIDENTIFIER NOT NULL,
                criado_em DATETIME2 NOT NULL CONSTRAINT DF_patients_criado_em DEFAULT (SYSUTCDATETIME()),
                organization_id UNIQUEIDENTIFIER NULL,
                CONSTRAINT PK_patients PRIMARY KEY (id),
                CONSTRAINT FK_patients_users_avaliador_id FOREIGN KEY (avaliador_id) REFERENCES dbo.users(id),
                CONSTRAINT FK_patients_groups_group_id FOREIGN KEY (group_id) REFERENCES dbo.groups(id),
                CONSTRAINT FK_patients_organizations_organization_id FOREIGN KEY (organization_id) REFERENCES dbo.organizations(id)
            );

            CREATE UNIQUE INDEX IX_patients_cpf ON dbo.patients(cpf);
            CREATE INDEX IX_patients_avaliador_id ON dbo.patients(avaliador_id);
            CREATE INDEX IX_patients_group_id ON dbo.patients(group_id);
            CREATE INDEX IX_patients_organization_id ON dbo.patients(organization_id);

            CREATE TABLE dbo.form_templates (
                id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_form_templates_id DEFAULT (NEWID()),
                nome NVARCHAR(200) NOT NULL,
                descricao NVARCHAR(1000) NULL,
                group_id UNIQUEIDENTIFIER NULL,
                criado_por_usuario_id UNIQUEIDENTIFIER NOT NULL,
                ativo BIT NOT NULL CONSTRAINT DF_form_templates_ativo DEFAULT ((1)),
                criado_em DATETIME2 NOT NULL CONSTRAINT DF_form_templates_criado_em DEFAULT (SYSUTCDATETIME()),
                atualizado_em DATETIME2 NOT NULL CONSTRAINT DF_form_templates_atualizado_em DEFAULT (SYSUTCDATETIME()),
                organization_id UNIQUEIDENTIFIER NULL,
                CONSTRAINT PK_form_templates PRIMARY KEY (id),
                CONSTRAINT FK_form_templates_groups_group_id FOREIGN KEY (group_id) REFERENCES dbo.groups(id),
                CONSTRAINT FK_form_templates_users_criado_por_usuario_id FOREIGN KEY (criado_por_usuario_id) REFERENCES dbo.users(id),
                CONSTRAINT FK_form_templates_organizations_organization_id FOREIGN KEY (organization_id) REFERENCES dbo.organizations(id)
            );

            CREATE INDEX IX_form_templates_group_id ON dbo.form_templates(group_id);
            CREATE INDEX IX_form_templates_criado_por_usuario_id ON dbo.form_templates(criado_por_usuario_id);
            CREATE INDEX IX_form_templates_organization_id ON dbo.form_templates(organization_id);

            CREATE TABLE dbo.form_questions (
                id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_form_questions_id DEFAULT (NEWID()),
                form_template_id UNIQUEIDENTIFIER NOT NULL,
                texto NVARCHAR(1000) NOT NULL,
                peso DECIMAL(10,2) NOT NULL,
                ordem INT NOT NULL,
                ativa BIT NOT NULL CONSTRAINT DF_form_questions_ativa DEFAULT ((1)),
                CONSTRAINT PK_form_questions PRIMARY KEY (id),
                CONSTRAINT FK_form_questions_form_templates_form_template_id FOREIGN KEY (form_template_id) REFERENCES dbo.form_templates(id) ON DELETE CASCADE
            );

            CREATE INDEX IX_form_questions_form_template_id ON dbo.form_questions(form_template_id);

            CREATE TABLE dbo.specialists (
                id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_specialists_id DEFAULT (NEWID()),
                nome NVARCHAR(200) NOT NULL,
                especialidade NVARCHAR(120) NOT NULL,
                custo_consulta DECIMAL(10,2) NOT NULL,
                ativo BIT NOT NULL CONSTRAINT DF_specialists_ativo DEFAULT ((1)),
                criado_em DATETIME2 NOT NULL CONSTRAINT DF_specialists_criado_em DEFAULT (SYSUTCDATETIME()),
                organization_id UNIQUEIDENTIFIER NULL,
                CONSTRAINT PK_specialists PRIMARY KEY (id),
                CONSTRAINT FK_specialists_organizations_organization_id FOREIGN KEY (organization_id) REFERENCES dbo.organizations(id)
            );

            CREATE INDEX IX_specialists_nome ON dbo.specialists(nome);
            CREATE INDEX IX_specialists_especialidade ON dbo.specialists(especialidade);
            CREATE INDEX IX_specialists_organization_id ON dbo.specialists(organization_id);

            CREATE TABLE dbo.evaluations (
                id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_evaluations_id DEFAULT (NEWID()),
                patient_id UNIQUEIDENTIFIER NOT NULL,
                avaliador_id UNIQUEIDENTIFIER NOT NULL,
                group_id UNIQUEIDENTIFIER NOT NULL,
                form_template_id UNIQUEIDENTIFIER NULL,
                respostas NVARCHAR(MAX) NOT NULL,
                score_total DECIMAL(10,2) NOT NULL,
                peso_total DECIMAL(10,2) NOT NULL,
                classificacao NVARCHAR(50) NOT NULL,
                observacoes NVARCHAR(2000) NULL,
                data_avaliacao DATETIME2 NOT NULL CONSTRAINT DF_evaluations_data_avaliacao DEFAULT (SYSUTCDATETIME()),
                organization_id UNIQUEIDENTIFIER NULL,
                CONSTRAINT PK_evaluations PRIMARY KEY (id),
                CONSTRAINT FK_evaluations_patients_patient_id FOREIGN KEY (patient_id) REFERENCES dbo.patients(id),
                CONSTRAINT FK_evaluations_users_avaliador_id FOREIGN KEY (avaliador_id) REFERENCES dbo.users(id),
                CONSTRAINT FK_evaluations_groups_group_id FOREIGN KEY (group_id) REFERENCES dbo.groups(id),
                CONSTRAINT FK_evaluations_form_templates_form_template_id FOREIGN KEY (form_template_id) REFERENCES dbo.form_templates(id),
                CONSTRAINT FK_evaluations_organizations_organization_id FOREIGN KEY (organization_id) REFERENCES dbo.organizations(id)
            );

            CREATE INDEX IX_evaluations_patient_id ON dbo.evaluations(patient_id);
            CREATE INDEX IX_evaluations_avaliador_id ON dbo.evaluations(avaliador_id);
            CREATE INDEX IX_evaluations_group_id ON dbo.evaluations(group_id);
            CREATE INDEX IX_evaluations_form_template_id ON dbo.evaluations(form_template_id);
            CREATE INDEX IX_evaluations_organization_id ON dbo.evaluations(organization_id);

            CREATE TABLE dbo.evaluation_referrals (
                id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_evaluation_referrals_id DEFAULT (NEWID()),
                evaluation_id UNIQUEIDENTIFIER NOT NULL,
                patient_id UNIQUEIDENTIFIER NOT NULL,
                encaminhado BIT NOT NULL,
                specialist_id UNIQUEIDENTIFIER NULL,
                specialist_nome NVARCHAR(200) NULL,
                especialidade NVARCHAR(120) NULL,
                custo_estimado DECIMAL(10,2) NOT NULL CONSTRAINT DF_evaluation_referrals_custo_estimado DEFAULT ((0)),
                criado_em DATETIME2 NOT NULL CONSTRAINT DF_evaluation_referrals_criado_em DEFAULT (SYSUTCDATETIME()),
                criado_por_usuario_id UNIQUEIDENTIFIER NOT NULL,
                organization_id UNIQUEIDENTIFIER NULL,
                CONSTRAINT PK_evaluation_referrals PRIMARY KEY (id),
                CONSTRAINT FK_evaluation_referrals_evaluations_evaluation_id FOREIGN KEY (evaluation_id) REFERENCES dbo.evaluations(id) ON DELETE CASCADE,
                CONSTRAINT FK_evaluation_referrals_patients_patient_id FOREIGN KEY (patient_id) REFERENCES dbo.patients(id),
                CONSTRAINT FK_evaluation_referrals_specialists_specialist_id FOREIGN KEY (specialist_id) REFERENCES dbo.specialists(id),
                CONSTRAINT FK_evaluation_referrals_users_criado_por_usuario_id FOREIGN KEY (criado_por_usuario_id) REFERENCES dbo.users(id),
                CONSTRAINT FK_evaluation_referrals_organizations_organization_id FOREIGN KEY (organization_id) REFERENCES dbo.organizations(id)
            );

            CREATE UNIQUE INDEX IX_evaluation_referrals_evaluation_id ON dbo.evaluation_referrals(evaluation_id);
            CREATE INDEX IX_evaluation_referrals_patient_id ON dbo.evaluation_referrals(patient_id);
            CREATE INDEX IX_evaluation_referrals_specialist_id ON dbo.evaluation_referrals(specialist_id);
            CREATE INDEX IX_evaluation_referrals_especialidade ON dbo.evaluation_referrals(especialidade);
            CREATE INDEX IX_evaluation_referrals_organization_id ON dbo.evaluation_referrals(organization_id);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            PRINT N'A migracao RecriarSchemaComIdsGuid e destrutiva e nao possui rollback automatico.';
            """);
    }
}
