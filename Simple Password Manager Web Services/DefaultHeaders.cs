using System;

namespace SimplePM.WebAPI
{
    internal static class DefaultHeaders
    {
        internal static readonly (string Key, string Value) AuthorizationHeader = ("WWW-Authenticate",
            "User's account current password encrypted with RSA open key. To obtain RSA open key: GET api/v1/rsa");
    }
}
