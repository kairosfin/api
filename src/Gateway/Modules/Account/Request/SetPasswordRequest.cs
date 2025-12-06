using System.ComponentModel.DataAnnotations;
using Kairos.Shared.Contracts.Account.AccessAccount;

namespace Kairos.Gateway.Modules.Account.Request;

internal sealed record SetPasswordRequest(
    [Required]
    Identifier Identifier,

    [Required]
    string Pass,

    [Required]
    string PassConfirmation,

    [Required]
    string Token);
