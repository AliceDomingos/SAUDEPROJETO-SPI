using SPI.Domain.Common;

namespace SPI.Domain.Entities;

public sealed class Patient : Entity, IAggregateRoot
{
    private static readonly HashSet<string> EstadosBrasileirosValidos =
    [
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA",
        "MT", "MS", "MG", "PA", "PB", "PR", "PE", "PI", "RJ", "RN",
        "RS", "RO", "RR", "SC", "SP", "SE", "TO"
    ];

    private Patient()
    {
    }

    public Patient(
        string nome,
        string cpf,
        DateTime dataNascimento,
        string sexo,
        string? telefone,
        string? email,
        string? nomeResponsavel,
        string? cep,
        string? estado,
        string? cidade,
        string? bairro,
        string? rua,
        string? numero,
        string? complemento,
        string? observacoes,
        int? avaliadorId,
        int groupId)
    {
        if (groupId <= 0)
        {
            throw new InvalidOperationException("Grupo invalido.");
        }

        AvaliadorId = avaliadorId;
        GroupId = groupId;
        CriadoEm = DateTime.UtcNow;
        ApplyDetails(
            nome,
            cpf,
            dataNascimento,
            sexo,
            telefone,
            email,
            nomeResponsavel,
            cep,
            estado,
            cidade,
            bairro,
            rua,
            numero,
            complemento,
            observacoes);
    }

    public string Nome { get; private set; } = string.Empty;
    public string Cpf { get; private set; } = string.Empty;
    public DateTime DataNascimento { get; private set; }
    public string Sexo { get; private set; } = string.Empty;
    public int? Idade { get; private set; }
    public int? AvaliadorId { get; private set; }
    public string? NomeResponsavel { get; private set; }
    public string? Telefone { get; private set; }
    public string? Email { get; private set; }
    public string? Cep { get; private set; }
    public string? Estado { get; private set; }
    public string? Cidade { get; private set; }
    public string? Bairro { get; private set; }
    public string? Rua { get; private set; }
    public string? Numero { get; private set; }
    public string? Complemento { get; private set; }
    public string? Observacoes { get; private set; }
    public int GroupId { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public int? OrganizationId { get; private set; }

    public User? Avaliador { get; private set; }
    public Group Group { get; private set; } = null!;
    public Organization? Organization { get; private set; }

    public void AssignOrganization(int organizationId) => OrganizationId = organizationId;

    public IReadOnlyCollection<Evaluation> Avaliacoes => _avaliacoes;
    private readonly List<Evaluation> _avaliacoes = [];

    public void UpdateDetails(
        string nome,
        string cpf,
        DateTime dataNascimento,
        string sexo,
        string? telefone,
        string? email,
        string? nomeResponsavel,
        string? cep,
        string? estado,
        string? cidade,
        string? bairro,
        string? rua,
        string? numero,
        string? complemento,
        string? observacoes,
        int groupId)
    {
        if (groupId <= 0)
        {
            throw new InvalidOperationException("Grupo invalido.");
        }

        GroupId = groupId;
        ApplyDetails(
            nome,
            cpf,
            dataNascimento,
            sexo,
            telefone,
            email,
            nomeResponsavel,
            cep,
            estado,
            cidade,
            bairro,
            rua,
            numero,
            complemento,
            observacoes);
    }

    private void ApplyDetails(
        string nome,
        string cpf,
        DateTime dataNascimento,
        string sexo,
        string? telefone,
        string? email,
        string? nomeResponsavel,
        string? cep,
        string? estado,
        string? cidade,
        string? bairro,
        string? rua,
        string? numero,
        string? complemento,
        string? observacoes)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new InvalidOperationException("Nome do paciente e obrigatorio.");
        }

        var normalizedCpf = NormalizeDigits(cpf);
        if (normalizedCpf.Length != 11)
        {
            throw new InvalidOperationException("CPF do paciente deve conter 11 digitos.");
        }

        var normalizedBirthDate = dataNascimento.Date;
        if (normalizedBirthDate == default)
        {
            throw new InvalidOperationException("Data de nascimento do paciente e obrigatoria.");
        }

        if (normalizedBirthDate > DateTime.UtcNow.Date)
        {
            throw new InvalidOperationException("Data de nascimento do paciente nao pode ser futura.");
        }

        var normalizedSex = (sexo ?? string.Empty).Trim().ToLowerInvariant();
        if (normalizedSex is not ("feminino" or "masculino" or "outro"))
        {
            throw new InvalidOperationException("Sexo do paciente e invalido.");
        }

        var normalizedEmail = NormalizeNullable(email);
        if (normalizedEmail is not null && (!normalizedEmail.Contains('@') || normalizedEmail.StartsWith('@') || normalizedEmail.EndsWith('@')))
        {
            throw new InvalidOperationException("Email do paciente e invalido.");
        }

        var normalizedCep = NormalizeDigitsOrNull(cep);
        if (normalizedCep is not null && normalizedCep.Length != 8)
        {
            throw new InvalidOperationException("CEP do paciente deve conter 8 digitos.");
        }

        var normalizedState = NormalizeNullable(estado)?.ToUpperInvariant();
        if (normalizedState is not null && !EstadosBrasileirosValidos.Contains(normalizedState))
        {
            throw new InvalidOperationException("Estado do paciente e invalido.");
        }

        Nome = nome.Trim();
        Cpf = normalizedCpf;
        DataNascimento = normalizedBirthDate;
        Sexo = normalizedSex;
        Idade = CalculateAge(normalizedBirthDate);
        NomeResponsavel = NormalizeNullable(nomeResponsavel);
        Telefone = NormalizeDigitsOrNull(telefone);
        Email = normalizedEmail;
        Cep = normalizedCep;
        Estado = normalizedState;
        Cidade = NormalizeNullable(cidade);
        Bairro = NormalizeNullable(bairro);
        Rua = NormalizeNullable(rua);
        Numero = NormalizeNullable(numero);
        Complemento = NormalizeNullable(complemento);
        Observacoes = NormalizeNullable(observacoes);
    }

    private static int CalculateAge(DateTime birthDate)
    {
        var today = DateTime.UtcNow.Date;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age))
        {
            age -= 1;
        }

        return Math.Max(age, 0);
    }

    private static string NormalizeDigits(string? value) =>
        new((value ?? string.Empty).Where(char.IsDigit).ToArray());

    private static string? NormalizeDigitsOrNull(string? value)
    {
        var digits = NormalizeDigits(value);
        return string.IsNullOrWhiteSpace(digits) ? null : digits;
    }

    private static string? NormalizeNullable(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}



