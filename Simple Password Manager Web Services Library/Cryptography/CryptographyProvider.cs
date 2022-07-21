using System;
using System.Security.Cryptography;
using System.Text;
using static System.Convert;

namespace SimplePM.WebAPI.Library.Cryptography
{
    public static class CryptographyProvider
    {
        public static class RSA
        {
            /// <summary>
            /// Encrypts specified string using RSA algorithm.
            /// </summary>
            /// 
            /// <param name="plainText">String containing plain text.</param>
            /// <param name="publicKey">RSA public key.</param>
            /// 
            /// <returns>The encrypted string.</returns>
            /// 
            /// <exception cref="ArgumentNullException">cryptoText is null
            /// -or- one of privateKey parameters is null.</exception>
            /// 
            /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. 
            ///     -or- The fOAEP parameter is true and the length of the rgb parameter
            ///     is greater than System.Security.Cryptography.RSACryptoServiceProvider.KeySize.
            ///     -or- The key does not match the encrypted data. However, the exception wording
            ///     may not be accurate. For example, it may say Not enough storage is available
            ///    to process this command.</exception>
            /// 
            /// <exception cref="EncoderFallbackException">A fallback occurred (for more information, see Character Encoding in .NET)
            ///     -and- System.Text.Encoding.EncoderFallback is set to System.Text.EncoderExceptionFallback.</exception>
            /// 
            /// <exception cref="FormatException">The length of encrypted string,
            ///     ignoring white-space characters, is not zero or a multiple 4.
            ///     -or- The format of encrypted string is invalid. Encrypted string contains a non-base-64 character, more
            ///     than two padding characters, or a non-white space-character among the padding characters.</exception>
            public static string Encrypt(string plainText, string publicKey)
            {
                var plainBytes = Encoding.UTF8.GetBytes(plainText);
                using var rsa = new RSACryptoServiceProvider(2048);
                try
                {
                    rsa.FromXmlStringExt(publicKey);
                    var encryptedData = rsa.Encrypt(plainBytes, true);
                    return ToBase64String(encryptedData);
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }

            /// <summary>
            /// Decrypts specified string encrypted with RSA algorithm.
            /// </summary>
            /// 
            /// <param name="cryptoText">String containing text encrypted with RSA algorithm.</param>
            /// <param name="privateKey">RSA private key.</param>
            /// 
            /// <returns>String containing plain text before encryption.</returns>
            /// 
            /// <exception cref="ArgumentException">Decrypted byte array contains invalid Unicode code points.</exception>
            /// 
            /// <exception cref="ArgumentNullException">cryptoText is null
            /// -or- one of privateKey parameters is null.</exception>
            /// 
            /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. 
            ///     -or- The fOAEP parameter is true and the length of the rgb parameter
            ///     is greater than System.Security.Cryptography.RSACryptoServiceProvider.KeySize.
            ///     -or- The key does not match the encrypted data. However, the exception wording
            ///     may not be accurate. For example, it may say Not enough storage is available
            ///    to process this command.</exception>
            /// 
            /// <exception cref="DecoderFallbackException">A fallback occurred (for more information, see Character Encoding in .NET)
            ///     -and- System.Text.Encoding.DecoderFallback is set to System.Text.DecoderExceptionFallback.</exception>
            /// 
            /// <exception cref="FormatException">The length of encrypted string,
            ///     ignoring white-space characters, is not zero or a multiple 4.
            ///     -or- The format of encrypted string is invalid. Encrypted string contains a non-base-64 character, more
            ///     than two padding characters, or a non-white space-character among the padding characters.</exception>
            public static string Decrypt(string cryptoText, string privateKey)
            {
                using var rsa = new RSACryptoServiceProvider(2048);
                try
                {
                    rsa.FromXmlStringExt(privateKey);
                    var resultBytes = FromBase64String(cryptoText);
                    var decryptedBytes = rsa.Decrypt(resultBytes, true);
                    var decryptedData = Encoding.UTF8.GetString(decryptedBytes);
                    return decryptedData.ToString();
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }
        public static class SHA256
        {
            /// <summary>
            /// Generates 16-byte base 64 string that can be used as salt for hashing.
            /// </summary>
            /// 
            /// <returns>Salt as base 64 string.</returns>
            public static string GenerateSalt()
            {
                var rng = RandomNumberGenerator.Create();
                var saltBytes = new byte[16];
                rng.GetBytes(saltBytes);
                return ToBase64String(saltBytes);
            }

            /// <summary>
            /// Concats salt provided with password and computes the hash value for resulting string with SHA256 algorithm.
            /// </summary>
            /// 
            /// <param name="plainText"></param>
            /// <param name="salt"></param>
            /// 
            /// <returns>The computed hash value.</returns>
            /// 
            /// <exception cref="ArgumentNullException">Both password and salt is null.</exception>
            /// 
            /// <exception cref="EncoderFallbackException">A fallback occurred (for more information, see Character Encoding in .NET) -and-
            ///     System.Text.Encoding.EncoderFallback is set to System.Text.EncoderExceptionFallback.</exception>
            public static string SaltAndHashString(string plainText, string salt)
            {
                var sha = System.Security.Cryptography.SHA256.Create();
                var saltedPassword = $"{plainText}{salt}";
                return ToBase64String(sha.ComputeHash(Encoding.Unicode.GetBytes(saltedPassword)));
            }
        }
    }
}
