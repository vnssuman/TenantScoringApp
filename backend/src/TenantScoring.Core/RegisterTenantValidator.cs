using System.Net.Mail;
using System.Text.RegularExpressions;
using TenantScoring.Models;

namespace TenantScoring.Core;

public static partial class RegisterTenantValidator
{
    public static IReadOnlyList<string> Validate(RegisterTenantRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.CountryCode))
        {
            errors.Add("Please select a country code.");
        }

        AddNameError(errors, request.FirstName, "First Name is required.", "First Name should contain only letters.");
        AddNameError(errors, request.LastName, "Last Name is required.", "Last Name should contain only letters.");

        if (string.IsNullOrWhiteSpace(request.MobileNumber))
        {
            errors.Add("Mobile Number is required.");
        }
        else if (!DigitsOnly().IsMatch(request.MobileNumber) || request.MobileNumber.Length != 10)
        {
            errors.Add("Please enter a valid 10-digit mobile number.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors.Add("Email is required.");
        }
        else if (!IsValidEmail(request.Email))
        {
            errors.Add("Please enter a valid email address.");
        }

        if (string.IsNullOrWhiteSpace(request.Pin))
        {
            errors.Add("PIN is required.");
        }
        else if (!DigitsOnly().IsMatch(request.Pin) || request.Pin.Length != 4)
        {
            errors.Add("PIN must be exactly 4 digits.");
        }

        if (string.IsNullOrWhiteSpace(request.ConfirmPin))
        {
            errors.Add("Confirm PIN is required.");
        }
        else if (!string.Equals(request.Pin, request.ConfirmPin, StringComparison.Ordinal))
        {
            errors.Add("PIN and Confirm PIN do not match.");
        }

        return errors;
    }

    private static void AddNameError(ICollection<string> errors, string value, string requiredMessage, string invalidMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(requiredMessage);
        }
        else if (!NameCharactersOnly().IsMatch(value))
        {
            errors.Add(invalidMessage);
        }
    }

    private static bool IsValidEmail(string value)
    {
        try
        {
            var address = new MailAddress(value);
            return string.Equals(address.Address, value, StringComparison.Ordinal);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    [GeneratedRegex("^[0-9]+$")]
    private static partial Regex DigitsOnly();

    [GeneratedRegex("^[\\p{L} -]+$")]
    private static partial Regex NameCharactersOnly();
}
