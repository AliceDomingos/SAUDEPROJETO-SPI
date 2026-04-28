using SPI.Infrastructure.Data.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SPI.Infrastructure.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260426000000_AdicionarOrganizacao")]
public partial class AddOrganization : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF OBJECT_ID(N'dbo.organizations', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.organizations (
                    id INT IDENTITY(1,1) NOT NULL,
                    nome NVARCHAR(200) NOT NULL,
                    admin_id INT NOT NULL,
                    criado_em DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                    CONSTRAINT PK_organizations PRIMARY KEY (id),
                    CONSTRAINT FK_organizations_users_admin_id FOREIGN KEY (admin_id) REFERENCES dbo.users(id)
                );

                CREATE UNIQUE INDEX IX_organizations_admin_id ON dbo.organizations (admin_id);
            END;

            IF COL_LENGTH(N'dbo.users', N'organization_id') IS NULL
            BEGIN
                ALTER TABLE dbo.users ADD organization_id INT NULL;
                ALTER TABLE dbo.users ADD CONSTRAINT FK_users_organizations_organization_id
                    FOREIGN KEY (organization_id) REFERENCES dbo.organizations(id);
            END;

            IF COL_LENGTH(N'dbo.groups', N'organization_id') IS NULL
            BEGIN
                ALTER TABLE dbo.groups ADD organization_id INT NULL;
                ALTER TABLE dbo.groups ADD CONSTRAINT FK_groups_organizations_organization_id
                    FOREIGN KEY (organization_id) REFERENCES dbo.organizations(id);
            END;

            IF COL_LENGTH(N'dbo.patients', N'organization_id') IS NULL
            BEGIN
                ALTER TABLE dbo.patients ADD organization_id INT NULL;
                ALTER TABLE dbo.patients ADD CONSTRAINT FK_patients_organizations_organization_id
                    FOREIGN KEY (organization_id) REFERENCES dbo.organizations(id);
            END;

            IF COL_LENGTH(N'dbo.evaluations', N'organization_id') IS NULL
            BEGIN
                ALTER TABLE dbo.evaluations ADD organization_id INT NULL;
                ALTER TABLE dbo.evaluations ADD CONSTRAINT FK_evaluations_organizations_organization_id
                    FOREIGN KEY (organization_id) REFERENCES dbo.organizations(id);
            END;

            IF COL_LENGTH(N'dbo.form_templates', N'organization_id') IS NULL
            BEGIN
                ALTER TABLE dbo.form_templates ADD organization_id INT NULL;
                ALTER TABLE dbo.form_templates ADD CONSTRAINT FK_form_templates_organizations_organization_id
                    FOREIGN KEY (organization_id) REFERENCES dbo.organizations(id);
            END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF COL_LENGTH(N'dbo.form_templates', N'organization_id') IS NOT NULL
            BEGIN
                ALTER TABLE dbo.form_templates DROP CONSTRAINT FK_form_templates_organizations_organization_id;
                ALTER TABLE dbo.form_templates DROP COLUMN organization_id;
            END;

            IF COL_LENGTH(N'dbo.evaluations', N'organization_id') IS NOT NULL
            BEGIN
                ALTER TABLE dbo.evaluations DROP CONSTRAINT FK_evaluations_organizations_organization_id;
                ALTER TABLE dbo.evaluations DROP COLUMN organization_id;
            END;

            IF COL_LENGTH(N'dbo.patients', N'organization_id') IS NOT NULL
            BEGIN
                ALTER TABLE dbo.patients DROP CONSTRAINT FK_patients_organizations_organization_id;
                ALTER TABLE dbo.patients DROP COLUMN organization_id;
            END;

            IF COL_LENGTH(N'dbo.groups', N'organization_id') IS NOT NULL
            BEGIN
                ALTER TABLE dbo.groups DROP CONSTRAINT FK_groups_organizations_organization_id;
                ALTER TABLE dbo.groups DROP COLUMN organization_id;
            END;

            IF COL_LENGTH(N'dbo.users', N'organization_id') IS NOT NULL
            BEGIN
                ALTER TABLE dbo.users DROP CONSTRAINT FK_users_organizations_organization_id;
                ALTER TABLE dbo.users DROP COLUMN organization_id;
            END;

            IF OBJECT_ID(N'dbo.organizations', N'U') IS NOT NULL
            BEGIN
                DROP TABLE dbo.organizations;
            END;
            """);
    }
}
