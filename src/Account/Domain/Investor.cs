using Kairos.Account.Domain.Abstraction;
using Kairos.Account.Domain.Enum;
using Kairos.Shared.Contracts;

namespace Kairos.Account.Domain;

/// <summary>
/// Investment account
/// </summary>
internal sealed class Investor : KairosAccount
{
    public string Name { get; private set; }
    public string Document { get; private set; }
    public DateTime Birthdate { get; private set; }
    public Gender Gender { get; private set; }
    public PersonType Type { get; private set; }

    Investor(
        string name,
        string phoneNumber,
        string document,
        string email,
        DateTime birthdate
    )
    {
        Name = name;
        PhoneNumber = phoneNumber;
        UserName = document;
        Document = document;
        Email = email;
        Birthdate = birthdate;
        Type = document.Length == 11 ? PersonType.Natural : PersonType.Legal;
    }

    public static Output<Investor?> OpenAccount(
        string name,
        string document,
        string phoneNumber,
        string email,
        DateTime birthdate,
        bool acceptTerms
    )
    {
        if (acceptTerms is false)
        {
            return Output<Investor?>.PolicyViolation(["Autorize a coleta de dados para prosseguir."]);
        }

        if (birthdate.AddYears(18) > DateTime.Today)
        {
            return Output<Investor?>.PolicyViolation(["É necessário ser maior de idade para abrir a conta."]);
        }

        if (phoneNumber.Length is not 11)
        {
            return Output<Investor?>.InvalidInput(["O número de telefone deve conter DDD."]);
        }

        if (document.Length is not 11 and not 14)
        {
            return Output<Investor?>.InvalidInput(["O documento deve conter apenas 11 caracteres para PF e 14 para PJ."]);
        }

        var emailValidation = Shared.Contracts.Account.ValueObjects.Email.Create(email);

        if (emailValidation.IsFailure)
        {
            return Output<Investor?>.InvalidInput(emailValidation.Messages);
        }

        // TODO: criar value object para phoneNumber e document

        return Output<Investor?>.Created(new Investor(
            name, 
            phoneNumber, 
            document, 
            email, 
            birthdate));
    }
}
