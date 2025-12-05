using System.ComponentModel.DataAnnotations;

namespace Kairos.Gateway.Modules.Account.Request;

internal sealed record SetPasswordRequest(
    [Required]
    string Pass, 

    [Required]
    string PassConfirmation,

    [Required]
    string Token);
