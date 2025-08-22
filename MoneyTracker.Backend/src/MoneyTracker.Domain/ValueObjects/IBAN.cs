using MoneyTracker.Domain.Common;
using System.Text.RegularExpressions;

namespace MoneyTracker.Domain.ValueObjects;

public class IBAN : ValueObject
{
    public string Value { get; private set; }
    public string CountryCode => Value.Substring(0, 2);
    public string CheckDigits => Value.Substring(2, 2);
    public string BasicBankAccountNumber => Value.Substring(4);

    private IBAN() { } // EF Core

    public IBAN(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("IBAN cannot be null or empty", nameof(value));
        
        var normalizedIban = NormalizeIban(value);
        
        if (!IsValidIban(normalizedIban))
            throw new ArgumentException($"Invalid IBAN format: {value}", nameof(value));
        
        Value = normalizedIban;
    }

    private static string NormalizeIban(string iban)
    {
        // Remove spaces and convert to uppercase
        return Regex.Replace(iban.ToUpper(), @"\s+", "");
    }

    private static bool IsValidIban(string iban)
    {
        // Basic format check: 2 letters + 2 digits + up to 30 alphanumeric characters
        if (!Regex.IsMatch(iban, @"^[A-Z]{2}\d{2}[A-Z0-9]+$"))
            return false;
        
        // Length check (minimum 15, maximum 34)
        if (iban.Length < 15 || iban.Length > 34)
            return false;
        
        // German IBAN specific validation (22 characters total)
        if (iban.StartsWith("DE") && iban.Length != 22)
            return false;
        
        // Mod-97 check digit validation
        return ValidateCheckDigits(iban);
    }

    private static bool ValidateCheckDigits(string iban)
    {
        try
        {
            // Move first 4 characters to end
            string rearranged = iban.Substring(4) + iban.Substring(0, 4);
            
            // Convert letters to numbers (A=10, B=11, ..., Z=35)
            string numericString = "";
            foreach (char c in rearranged)
            {
                if (char.IsLetter(c))
                {
                    numericString += (c - 'A' + 10).ToString();
                }
                else
                {
                    numericString += c;
                }
            }
            
            // Calculate mod 97
            return Mod97(numericString) == 1;
        }
        catch
        {
            return false;
        }
    }

    private static int Mod97(string numericString)
    {
        string remainder = "";
        
        foreach (char digit in numericString)
        {
            remainder += digit;
            
            if (remainder.Length >= 9)
            {
                int temp = int.Parse(remainder);
                remainder = (temp % 97).ToString();
            }
        }
        
        return int.Parse(remainder) % 97;
    }

    public string ToDisplayFormat()
    {
        // Format as: DE89 3704 0044 0532 0130 00
        if (string.IsNullOrEmpty(Value) || Value.Length < 4)
            return Value;
        
        var formatted = "";
        for (int i = 0; i < Value.Length; i += 4)
        {
            if (i > 0) formatted += " ";
            formatted += Value.Substring(i, Math.Min(4, Value.Length - i));
        }
        return formatted;
    }

    public bool IsGermanIban => Value.StartsWith("DE");
    
    public string GetBankCode()
    {
        // For German IBANs, bank code is positions 4-11 (8 digits)
        if (IsGermanIban && Value.Length >= 12)
            return Value.Substring(4, 8);
        
        return string.Empty;
    }

    public string GetAccountNumber()
    {
        // For German IBANs, account number is positions 12-21 (10 digits)
        if (IsGermanIban && Value.Length >= 22)
            return Value.Substring(12, 10);
        
        return string.Empty;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return ToDisplayFormat();
    }

    public static implicit operator string(IBAN iban) => iban.Value;
}