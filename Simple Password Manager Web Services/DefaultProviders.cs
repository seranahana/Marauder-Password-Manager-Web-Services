using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SimplePM.WebAPI
{
    internal static class DefaultHeadersProvider
    {
        internal static readonly (string Key, string Value) AuthorizationHeader = ("WWW-Authenticate",
            "User's account current password encrypted with RSA open key. To obtain RSA open key: GET api/v1/rsa");
    }

    internal static class DefaultMessagesProvider
    {
        private static readonly Dictionary<Params, string> paramDict = new()
        {
            { Params.AccountIdentificator, "account identificator" },
            { Params.AccountPassword, "account password" },
            { Params.MasterPasswordOrOperationCode, "master password or operation code" },
            { Params.NewAccountPassword, "new account password" },
            { Params.NewMasterPassword, "new master password" },
            { Params.RsaPublicKey, "RSA public key" },
            { Params.UserDataModel, "UserData model" },
            { Params.Username, "username" }
        };

        internal const string EncryptionRequired = "Encryption is required. To obtain RSA open key: GET api/v1/rsa";
        internal const string InternalServerError = "An internal server error occurred. If the problem persists, please contact software developer.";

        internal static string GetUnauthorizedIncorrectMessage(Params incorrectParam)
        {
            string incorrectParamName = paramDict[incorrectParam];
            if (incorrectParamName is not null)
            {
                return $"The {incorrectParamName} provided is incorrect. Please enter a correct one and try again.";
            }
            return "The value provided is incorrect. Please enter a correct one and try again.";
        }

        internal static string GetCorruptedOrMissingMessage(Params missingParam)
        {
            string missingParamName = paramDict[missingParam];
            if (missingParamName is not null)
            {
                return $"The {missingParamName} provided is corrupted or missing.";
            }
            return "The value provided is corrupted or missing";
        }
    }
}
