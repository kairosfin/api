namespace Kairos.Shared.Contracts.Account.GetAccountInfo;

public sealed record AccountPhone(
    string Number,
    bool Confirmed)
{
    public string FormattedNumber 
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Number))
            {
                return string.Empty;
            }

            var digits = new string(Number.Where(char.IsDigit).ToArray());

            if (digits.Length == 11)
            {
                return Convert.ToUInt64(digits).ToString(@"\(##\) #####-####");
            }

            if (digits.Length == 10)
            {
                return Convert.ToUInt64(digits).ToString(@"\(##\) ####-####");
            }

            return Number;
        }
    }
}