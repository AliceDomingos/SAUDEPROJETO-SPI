using SPI.Application.DTOs.Evaluations;
using SPI.Application.DTOs.Forms;
using SPI.Application.DTOs.Groups;
using SPI.Application.DTOs.Patients;
using SPI.Application.DTOs.Users;
using SPI.Domain.Entities;
using SPI.Domain.Enums;
using SPI.Domain.ReadModels;

namespace SPI.Application.Mappings;

public static class DomainToDtoMapper
{
    public static UserResponseDto ToDto(this User user) => new()
    {
        Id = user.Id,
        Nome = user.Nome,
        Email = user.Email,
        Role = user.Role.ToApiValue(),
        Ativo = user.Ativo,
        PodeAvaliar = user.Role.CanEvaluate(),
        GroupIds = user.GroupMemberships
            .Select(x => x.GroupId)
            .Union(user.ManagedGroups.Select(x => x.Id))
            .Distinct()
            .OrderBy(x => x)
            .ToArray(),
        GroupNames = user.GroupMemberships
            .Where(x => x.Group is not null)
            .Select(x => x.Group.Nome)
            .Union(user.ManagedGroups.Select(x => x.Nome))
            .Distinct()
            .OrderBy(x => x)
            .ToArray(),
        CriadoEm = user.CriadoEm
    };

    public static PatientResponseDto ToDto(this Patient patient) => new()
    {
        Id = patient.Id,
        Nome = patient.Nome,
        Cpf = patient.Cpf,
        DataNascimento = patient.DataNascimento,
        Sexo = patient.Sexo,
        Idade = patient.Idade,
        AvaliadorId = patient.AvaliadorId,
        NomeResponsavel = patient.NomeResponsavel,
        Telefone = patient.Telefone,
        Email = patient.Email,
        Cep = patient.Cep,
        Estado = patient.Estado,
        Cidade = patient.Cidade,
        Bairro = patient.Bairro,
        Rua = patient.Rua,
        Numero = patient.Numero,
        Complemento = patient.Complemento,
        Observacoes = patient.Observacoes,
        GroupId = patient.GroupId,
        GroupNome = patient.Group.Nome,
        CriadoEm = patient.CriadoEm
    };

    public static EvaluationResponseDto ToDto(this EvaluationDetails evaluation) => new()
    {
        Id = evaluation.Id,
        PatientId = evaluation.PatientId,
        PatientNome = evaluation.PatientNome,
        AvaliadorId = evaluation.AvaliadorId,
        AvaliadorNome = evaluation.AvaliadorNome,
        GroupId = evaluation.GroupId,
        GroupNome = evaluation.GroupNome,
        FormId = evaluation.FormTemplateId,
        FormNome = evaluation.FormNome,
        Respostas = new Dictionary<int, int>(evaluation.Respostas),
        ScoreTotal = evaluation.ScoreTotal,
        PesoTotal = evaluation.PesoTotal,
        Classificacao = evaluation.Classificacao,
        DataAvaliacao = evaluation.DataAvaliacao
    };

    public static GroupResponseDto ToDto(this Group group) => new()
    {
        Id = group.Id,
        Nome = group.Nome,
        GestorId = group.GestorId,
        GestorNome = group.Gestor?.Nome ?? string.Empty,
        Ativo = group.Ativo,
        QuantidadeMembros = group.Members.Select(x => x.UserId).Distinct().Count(),
        CriadoEm = group.CriadoEm
    };

    public static FormResponseDto ToDto(this FormTemplate form) => new()
    {
        Id = form.Id,
        Nome = form.Nome,
        Descricao = form.Descricao,
        GroupId = form.GroupId,
        GroupNome = form.Group?.Nome,
        CriadoPorUsuarioId = form.CriadoPorUsuarioId,
        CriadoPorNome = form.CriadoPorUsuario?.Nome ?? string.Empty,
        Ativo = form.Ativo,
        PesoTotal = form.PesoTotal,
        CriadoEm = form.CriadoEm,
        AtualizadoEm = form.AtualizadoEm,
        Perguntas = form.Questions
            .OrderBy(x => x.Ordem)
            .Select(x => new FormQuestionResponseDto
            {
                Id = x.Id,
                Texto = x.Texto,
                Peso = x.Peso,
                Ordem = x.Ordem,
                Ativa = x.Ativa,
                Opcoes = x.Options
                    .OrderBy(o => o.Valor)
                    .Select(o => new FormQuestionOptionResponseDto { Valor = o.Valor, Descricao = o.Descricao })
                    .ToArray()
            })
            .ToArray(),
        Faixas = form.ClassificationRanges
            .OrderBy(x => x.ScoreMin)
            .Select(x => new FormClassificationRangeResponseDto
            {
                ScoreMin = x.ScoreMin,
                ScoreMax = x.ScoreMax,
                Rotulo = x.Rotulo
            })
            .ToArray()
    };
}



