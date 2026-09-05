using TenantScoring.Models;

namespace TenantScoring.Core;

public static class AuthenticationValidator
{
    public static IReadOnlyList<string> ValidateLogin(LoginRequest request)
    {
        var errors = new List<string>();
        AddMobileErrors(errors, request.MobileNumber);
        if (string.IsNullOrWhiteSpace(request.Pin))
        {
            errors.Add("PIN is required.");
        }
        else if (!IsFourDigitPin(request.Pin))
        {
            errors.Add("PIN must be exactly 4 digits.");
        }

        return errors;
    }

    public static IReadOnlyList<string> ValidateReset(ResetPinRequest request)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(request.MobileNumber))
        {
            errors.Add("Mobile Number is required to reset PIN.");
        }
        else if (!IsTenDigitNumber(request.MobileNumber))
        {
            errors.Add("Please enter a valid 10-digit mobile number.");
        }

        if (string.IsNullOrWhiteSpace(request.NewPin))
        {
            errors.Add("New PIN is required.");
        }
        else if (!IsFourDigitPin(request.NewPin))
        {
            errors.Add("PIN must be exactly 4 digits.");
        }

        if (!string.IsNullOrWhiteSpace(request.ConfirmNewPin) &&
            !string.Equals(request.NewPin, request.ConfirmNewPin, StringComparison.Ordinal))
        {
            errors.Add("New PIN and Confirm PIN do not match.");
        }

        return errors;
    }

    private static void AddMobileErrors(ICollection<string> errors, string mobileNumber)
    {
        if (string.IsNullOrWhiteSpace(mobileNumber))
        {
            errors.Add("Mobile Number is required.");
        }
        else if (!IsTenDigitNumber(mobileNumber))
        {
            errors.Add("Please enter a valid 10-digit mobile number.");
        }
    }

    private static bool IsTenDigitNumber(string value) =>
        value.Length == 10 && value.All(character => character is >= '0' and <= '9');

    private static bool IsFourDigitPin(string value) =>
        value.Length == 4 && value.All(character => character is >= '0' and <= '9');
}
