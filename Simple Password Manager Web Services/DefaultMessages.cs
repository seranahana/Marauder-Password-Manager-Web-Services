using System;
using System.Text.RegularExpressions;

namespace SimplePM.WebAPI
{
    internal static class DefaultMessages
    {
        internal const string EncryptionRequired = "Encryption is required. To obtain RSA open key: GET api/v1/rsa";
        internal const string InternalServerError = "An internal server error occurred. If the problem persists, please contact software developer.";

        internal static string GetUnauthorizedIncorrectMessage(string value)
        {
            return $"The {value} provided is incorrect. Please enter a correct one and try again.";
        }

        internal static string GetUnauthorizedRequiredMessage(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                if (Regex.IsMatch(value, @"^[aeiouy]"))
                return $"An {value} required to confirm identity.";

                return $"A {value} required to confirm identity.";
            }
            return "An value required to confirm identity.";
        }

        internal static string GetCorruptedOrMissingMessage(string modelName)
        {
            return $"The {modelName} provided is corrupted or missing.";
        }
    }
}
