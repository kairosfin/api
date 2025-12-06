using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Kairos.Account.Configuration;
using Kairos.Account.Domain;
using Kairos.Account.Infra;
using Kairos.Shared.Contracts;
using Kairos.Shared.Contracts.Account.AccessAccount;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Kairos.Account.Business.UseCases;

internal sealed class AccessAccountUseCase(
    IOptions<Settings> config,
    ILogger<AccessAccountUseCase> logger,
    SignInManager<Investor> identity,
    AccountContext db
) : IRequestHandler<AccessAccountCommand, Output<string>>
{
    readonly JwtOptions _settings = config.Value.Jwt;

    public async Task<Output<string>> Handle(
        AccessAccountCommand input, 
        CancellationToken cancellationToken)
    {
        var enrichers = new Dictionary<string, object?> 
        { 
            ["Identifier"] = input.Identifier.Value, 
            ["CorrelationId"] = input.CorrelationId 
        };
        
        using (logger.BeginScope(enrichers))
        {
            try
            {
                var id = input.Identifier;

                var account = await db.Investors.FirstOrDefaultAsync(
                    i =>
                        (id.Type == AccountIdentifier.Document && i.Document == id.Value) ||
                        (id.Type == AccountIdentifier.Email && i.Email == id.Value) ||
                        (id.Type == AccountIdentifier.PhoneNumber && i.PhoneNumber == id.Value) ||
                        (id.Type == AccountIdentifier.AccountNumber && i.Id.ToString() == id.Value),
                    cancellationToken);

                if (account is null)
                {
                    logger.LogWarning("Sign-in failed. Account not found.");
                    return Output<string>.PolicyViolation(["Credenciais inválidas."]);
                }

                var result = await identity.CheckPasswordSignInAsync(
                    account, 
                    input.Password, 
                    lockoutOnFailure: true);

                if (result.IsLockedOut)
                {
                    logger.LogWarning("Sign-in failed. Account is locked out.");
                    return Output<string>.PolicyViolation(["Esta conta está bloqueada. Tente novamente após 5 minutos."]);
                }

                if (result.IsNotAllowed)
                {
                    logger.LogWarning("Sign-in failed. Email not confirmed.");
                    return Output<string>.PolicyViolation(["Confirme seu e-mail antes de acessar a conta."]);
                }
                
                if (result.Succeeded is false)
                {
                    logger.LogWarning("Sign-in failed. Invalid password.");
                    return Output<string>.PolicyViolation(["Credenciais inválidas."]);
                }

                logger.LogInformation("Sign-in successful. Generating token.");

                var token = GenerateJwtToken(account);

                return Output<string>.Ok(token, ["Autenticação realizada com sucesso!"]);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred during sign-in.");
                return Output<string>.UnexpectedError([ex.Message]);
            }
        }
    }

    string GenerateJwtToken(Investor account)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_settings.Secret);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, account.Email!),
            new(JwtRegisteredClaimNames.Name, account.Name),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes),
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
