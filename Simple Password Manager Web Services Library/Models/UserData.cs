using Newtonsoft.Json;
using SimplePM.WebAPI.Library.Cryptography;
using System;
using System.Collections.Generic;

namespace SimplePM.WebAPI.Library.Models
{
    public class UserData
    {
        [JsonProperty]
        public string ID { get; set; }
        [JsonProperty]
        public string Login { get; set; }
        [JsonProperty]
        public string Password { get; set; }
        [JsonProperty]
        public string Salt { get; set; }
        [JsonProperty]
        public string MasterPassword { get; set; }
        [JsonProperty]
        public string MasterSalt { get; set; }

        [JsonProperty]
        public virtual ICollection<RepositoryEntry> Entries { get; set; }

        public UserData()
        {
            Entries = new HashSet<RepositoryEntry>();
        }

        public UserData(string masterPassword, string masterSalt)
        {
            MasterPassword = masterPassword;
            MasterSalt = masterSalt;
        }

        public UserData(string id, string masterPassword, string masterSalt) : this (masterPassword, masterSalt)
        {
            ID = id;
        }

        public UserData(string id, string login, string password, string salt, string masterPassword, string masterSalt) 
            : this(id, masterPassword, masterSalt)
        {
            Login = login;
            Password = password;
            Salt = salt;
            Entries = new HashSet<RepositoryEntry>();
        }

        public UserData(string id, string login, string password, string salt, string masterPassword, string masterSalt, ICollection<RepositoryEntry> entries)
            : this(id, login, password, salt, masterPassword, masterSalt)
        {
            Entries = entries;
        }

        /// <summary>
        /// Decrypts all data in instance that is not null with RSA private key
        /// </summary>
        /// 
        /// <param name="rsaPrivateKey">RSA private key</param>
        /// 
        /// <exception cref="ArgumentException">Decrypted byte array contains invalid Unicode code points</exception>
        /// 
        /// <exception cref="ArgumentNullException">RSA private key is null
        /// -or- one of RSA private key parameters in null</exception>
        /// 
        /// /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.
        ///     -or- The fOAEP parameter is true and the length of the rgb parameter
        ///     is greater than System.Security.Cryptography.RSACryptoServiceProvider.KeySize.
        ///     -or- The key does not match the encrypted data. However, the exception wording
        ///     may not be accurate. For example, it may say Not enough storage is available
        ///    to process this command.</exception>
        /// 
        /// <exception cref="System.Text.DecoderFallbackException">A fallback occurred (for more information, see Character Encoding in .NET)
        ///     -and- System.Text.Encoding.DecoderFallback is set to System.Text.DecoderExceptionFallback.</exception>
        /// 
        /// <exception cref="FormatException">The length of encrypted string,
        ///     ignoring white-space characters, is not zero or a multiple 4.
        ///     -or- The format of encrypted string is invalid. Encrypted string contains a non-base-64 character, more
        ///     than two padding characters, or a non-white space-character among the padding characters.</exception>
        public void DecryptPrivateData(string rsaPrivateKey)
        {
            ArgumentNullException.ThrowIfNull(rsaPrivateKey);
            if (!string.IsNullOrEmpty(Login))
            {
                string decryptedLogin = CryptographyProvider.RSA.Decrypt(Login, rsaPrivateKey);
                Login = decryptedLogin;
            }
            if (!string.IsNullOrEmpty(Password))
            {
                string decryptedPassword = CryptographyProvider.RSA.Decrypt(Password, rsaPrivateKey);
                Password = decryptedPassword;
            }
            if (!string.IsNullOrEmpty(Salt))
            {
                string decryptedSalt = CryptographyProvider.RSA.Decrypt(Salt, rsaPrivateKey);
                Salt = decryptedSalt;
            }
            if (!string.IsNullOrEmpty(MasterPassword))
            {
                string decryptedMasterPassword = CryptographyProvider.RSA.Decrypt(MasterPassword, rsaPrivateKey);
                MasterPassword = decryptedMasterPassword;
            }
            if (!string.IsNullOrEmpty(MasterSalt))
            {
                string decryptedMasterSalt = CryptographyProvider.RSA.Decrypt(MasterSalt, rsaPrivateKey);
                MasterSalt = decryptedMasterSalt;
            }
        }

        /// <summary>
        /// Computes hash codes only for Password field in UserData instanse and updates it with resulting value. 
        /// If Salt field is null or empty, a new salt will be generated.
        /// </summary>
        /// 
        /// <exception cref="System.Text.EncoderFallbackException">A fallback occurred (for more information, see Character Encoding in .NET) 
        ///     -and- System.Text.Encoding.EncoderFallback is set to System.Text.EncoderExceptionFallback.</exception>
        public void HashAccountPassword()
        {
            if (string.IsNullOrEmpty(Salt))
            {
                string salt = CryptographyProvider.SHA256.GenerateSalt();
                string saltedAndHashedPassword = CryptographyProvider.SHA256.SaltAndHashString(Password, salt);
                Password = saltedAndHashedPassword;
                Salt = salt;
            }
            else
            {
                string saltedAndHashedPassword = CryptographyProvider.SHA256.SaltAndHashString(Password, Salt);
                Password = saltedAndHashedPassword;
            }
        }

        /// <summary>
        /// Computes hash codes only for MasterPassword field in UserData instanse and updates it with resulting value. 
        /// If MasterSalt field is null or empty, a new salt will be generated.
        /// </summary>
        /// 
        /// <exception cref="System.Text.EncoderFallbackException">A fallback occurred (for more information, see Character Encoding in .NET) 
        ///     -and- System.Text.Encoding.EncoderFallback is set to System.Text.EncoderExceptionFallback.</exception>
        public void HashMasterPassword()
        {
            if (string.IsNullOrEmpty(MasterSalt))
            {
                string masterSalt = CryptographyProvider.SHA256.GenerateSalt();
                string saltedAndHashedMasterPassword = CryptographyProvider.SHA256.SaltAndHashString(MasterPassword, masterSalt);
                MasterPassword = saltedAndHashedMasterPassword;
                MasterSalt = masterSalt;
            }
            else
            {
                string saltedAndHashedMasterPassword = CryptographyProvider.SHA256.SaltAndHashString(MasterPassword, MasterSalt);
                MasterPassword = saltedAndHashedMasterPassword;
            }
        }
    }
}
