using System.Text.RegularExpressions;
using Kairos.Shared.Abstractions.Domain;

namespace Kairos.Shared.Contracts.Account.ValueObjects;

public sealed record Email : IValueObject
{
    const string Pattern = @"^.+@.+\..+$";
    const byte MaxLength = 100;
    const byte MinLength = 4;

    public string Value { get; init; }

    public static implicit operator Email?(string value) =>
        Create(value)?.Value;

    Email(string value) => Value = value;

    public static Output<Email?> Create(string value)
    {
        string error = value switch
        {
            var v when string.IsNullOrEmpty(v) => "Email não preenchido",
            var v when v.Length < MinLength => $"Email deve ter ao menos {MinLength} caracteres",
            var v when v.Length > MaxLength => $"Email não pode ultrapassar {MaxLength} caracteres",
            var v when !Regex.IsMatch(v, Pattern) => "Email deve seguir o padrão *@*.*",
            _ => string.Empty
        };

        if (!string.IsNullOrEmpty(error))
        {
            return Output<Email?>.InvalidInput([error]);
        }

        return Output<Email?>.Created(new Email(value));
    }
}
