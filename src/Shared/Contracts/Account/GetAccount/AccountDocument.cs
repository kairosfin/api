namespace Kairos.Shared.Contracts.Account.GetAccountInfo;

public sealed record AccountDocument(string Number)
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

            if (digits.Length is 11)
            {
                return Convert.ToUInt64(digits).ToString(@"###\.###\.###\-##");
            }

            if (digits.Length is 14)
            {
                return Convert.ToUInt64(digits).ToString(@"##\.###\.###\/####\-##");
            }

            return Number;
        }
    }
}