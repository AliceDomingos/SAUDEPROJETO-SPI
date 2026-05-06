using SPI.Domain.Common;

namespace SPI.Domain.Entities;

public sealed class Specialist : Entity, IAggregateRoot
{
    private Specialist()
    {
    }

    public Specialist(string nome, string especialidade, decimal custoConsulta)
    {
        Update(nome, especialidade, custoConsulta);
        Ativo = true;
        CriadoEm = DateTime.UtcNow;
    }

    public string Nome { get; private set; } = string.Empty;
    public string Especialidade { get; private set; } = string.Empty;
    public decimal CustoConsulta { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public Guid? OrganizationId { get; private set; }

    public Organization? Organization { get; private set; }

    public void AssignOrganization(Guid organizationId) => OrganizationId = organizationId;

    public void Update(string nome, string especialidade, decimal custoConsulta)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new InvalidOperationException("Nome do especialista e obrigatorio.");
        }

        if (string.IsNullOrWhiteSpace(especialidade))
        {
            throw new InvalidOperationException("Especialidade e obrigatoria.");
        }

        if (custoConsulta <= 0)
        {
            throw new InvalidOperationException("Valor da consulta deve ser maior que zero.");
        }

        Nome = nome.Trim();
        Especialidade = especialidade.Trim();
        CustoConsulta = custoConsulta;
    }

    public void Activate() => Ativo = true;

    public void Deactivate() => Ativo = false;
}
