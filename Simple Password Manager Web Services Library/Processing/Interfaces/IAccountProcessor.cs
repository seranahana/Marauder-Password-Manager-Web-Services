using System;
using SimplePM.WebAPI.Library.Models;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SimplePM.WebAPI.Library.Processing
{
    public interface IAccountProcessor
    {
        /// <summary>
        /// Checks if there is an entry in database with such login and password.
        /// </summary>
        /// 
        /// <param name="encryptedLogin">User account username encrypted with RSA algorithm.</param>
        /// <param name="encryptedPassword">User account password encrypted with RSA algorithm.</param>
        /// <param name="rsaPrivateKey">RSA private key.</param>
        /// 
        /// <returns>Account identificator, master password in salted and hashed view and it's salt.</returns>
        /// 
        /// <exception cref="ArgumentException">Account with such login does not exist or encryptedPassword is incorrect
        ///     -or- decrypted byte array contains invalid Unicode code points. </exception>
        /// 
        /// <exception cref="ArgumentNullException">RSA private key is null
        ///     -or- one of RSA private key parameters in null
        ///     -or- encryptedLogin is null
        ///     -or- encryptedPassword is null.</exception>
        /// 
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.
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
        Task<UserData> AuthenticateAsync(string encryptedLogin, string encryptedPassword, string rsaPrivateKey,
            [CallerArgumentExpression("encryptedLogin")] string firstParamName = null,
            [CallerArgumentExpression("encryptedPassword")] string secondParamName = null);

        /// <summary>
        /// Deletes user data from database.
        /// </summary>
        /// <param name="encryptedLogin">User account username encrypted with RSA algorithm.</param>
        /// <param name="encryptedPassword">User account password encrypted with RSA algorithm.</param>
        /// <param name="rsaPrivateKey">RSA private key.</param>
        /// 
        /// <exception cref="ArgumentException">The accountID parameter value not found or passwords do not match
        ///     -or- decrypted byte array contains invalid Unicode code points.</exception>
        /// 
        /// <exception cref="ArgumentNullException">RSA private key is null
        ///     -or- one of RSA private key parameters in null
        ///     -or- encryptedLogin is null
        ///     -or- encryptedPassword is null.</exception>
        /// 
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.
        ///     -or- The fOAEP parameter is true and the length of the rgb parameter
        ///     is greater than System.Security.Cryptography.RSACryptoServiceProvider.KeySize.
        ///     -or- The key does not match the encrypted data. However, the exception wording
        ///     may not be accurate. For example, it may say Not enough storage is available
        ///    to process this command.</exception>
        /// 
        /// <exception cref="DBOperationException">Unexpected amount of affected string on database operation.</exception>
        /// 
        /// <exception cref="IntermediateStorageException">Operation on intermediate storage failed.</exception>
        /// 
        /// <exception cref="System.Text.DecoderFallbackException">A fallback occurred (for more information, see Character Encoding in .NET)
        ///     -and- System.Text.Encoding.DecoderFallback is set to System.Text.DecoderExceptionFallback.</exception>
        /// 
        /// <exception cref="FormatException">The length of encrypted string,
        ///     ignoring white-space characters, is not zero or a multiple 4.
        ///     -or- The format of encrypted string is invalid. Encrypted string contains a non-base-64 character, more
        ///     than two padding characters, or a non-white space-character among the padding characters.</exception>
        Task DeleteAccountAsync(string encryptedLogin, string encryptedPassword, string rsaPrivateKey,
            [CallerArgumentExpression("encryptedLogin")] string firstParamName = null,
            [CallerArgumentExpression("encryptedPassword")] string secondParamName = null);

        /// <summary>
        /// Checks if there's no entries with such login in database.
        /// </summary>
        /// 
        /// <param name="encryptedLogin">Desirable username encrypted with RSA algorithm.</param>
        /// <param name="rsaPrivateKey">RSA private key.</param>
        /// <returns>True - if login is available, false - if it's occupied.</returns>
        /// 
        /// <exception cref="ArgumentException">Decrypted byte array contains invalid Unicode code points.</exception>
        /// 
        /// <exception cref="ArgumentNullException">RSA private key is null
        ///     -or- one of RSA private key parameters in null
        ///     -or- encryptedLogin is null.</exception>
        /// 
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.
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
        Task<bool> IsLoginAvailableAsync(string encryptedLogin, string rsaPrivateKey);

        /// <summary>
        /// Adds new entry with account data to database.
        /// </summary>
        /// 
        /// <param name="encryptedAccountData">UserData object that includes new account data which should be encrypted with RSA algorithm.</param>
        /// <param name="rsaPrivateKey">RSA private key.</param>
        /// 
        /// <returns>New account GUID</returns>
        /// 
        /// <exception cref="ArgumentException">Decrypted byte array contains invalid Unicode code points.</exception>
        /// 
        /// <exception cref="ArgumentNullException">RSA private key is null
        ///     -or- one of RSA private key parameters in null.
        ///     -or- encryptedAccountData is null.</exception>
        /// 
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.
        ///     -or- The fOAEP parameter is true and the length of the rgb parameter
        ///     is greater than System.Security.Cryptography.RSACryptoServiceProvider.KeySize.
        ///     -or- The key does not match the encrypted data. However, the exception wording
        ///     may not be accurate. For example, it may say Not enough storage is available
        ///    to process this command.</exception>
        /// 
        /// <exception cref="DBOperationException">Unexpected amount of affected string on database operation.</exception>
        /// 
        /// <exception cref="System.Text.DecoderFallbackException">A fallback occurred (for more information, see Character Encoding in .NET)
        ///     -and- System.Text.Encoding.DecoderFallback is set to System.Text.DecoderExceptionFallback.</exception>
        ///     
        /// <exception cref="System.Text.EncoderFallbackException">A fallback occurred (for more information, see Character Encoding in .NET) 
        ///     -and- System.Text.Encoding.EncoderFallback is set to System.Text.EncoderExceptionFallback.</exception>
        /// 
        /// <exception cref="FormatException">The length of encrypted string,
        ///     ignoring white-space characters, is not zero or a multiple 4.
        ///     -or- The format of encrypted string is invalid. Encrypted string contains a non-base-64 character, more
        ///     than two padding characters, or a non-white space-character among the padding characters.</exception>
        Task<string> RegisterAsync(UserData encryptedAccountData, string rsaPrivateKey);

        /// <summary>
        /// Resets master password using login and password to confirm user identity.
        /// </summary>
        /// <param name="encryptedLogin">User account username encrypted with RSA private key.</param>
        /// <param name="encryptedPassword">User account password encrypted with RSA algorithm.</param>
        /// <param name="rsaPublicKey">RSA public key from client<./param>
        /// <param name="rsaPrivateKey">Internal RSA private key.</param>
        /// 
        /// <returns>String contains operation unique identificator encrypted with RSA algorithm. This identificator should be pass with a set request to authenticate operation.</returns>
        /// 
        /// <exception cref="ArgumentException">The login parameter value not found or passwords do not match
        ///     -or- decrypted byte array contains invalid Unicode code points.</exception>
        /// 
        /// <exception cref="ArgumentNullException">RSA private key is null
        ///     -or- one of RSA private key parameters in null
        ///     -or- encryptedLogin is null
        ///     -or- encryptedPassword is null.</exception>
        /// 
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.
        ///     -or- The fOAEP parameter is true and the length of the rgb parameter
        ///     is greater than System.Security.Cryptography.RSACryptoServiceProvider.KeySize.
        ///     -or- The key does not match the encrypted data. However, the exception wording
        ///     may not be accurate. For example, it may say Not enough storage is available
        ///    to process this command.</exception>
        /// 
        /// <exception cref="DBOperationException">Unexpected amount of affected string on database operation.</exception>
        /// 
        /// <exception cref="IntermediateStorageException">Operation on intermediate storage failed.</exception>
        /// 
        /// <exception cref="System.Text.DecoderFallbackException">A fallback occurred (for more information, see Character Encoding in .NET)
        ///     -and- System.Text.Encoding.DecoderFallback is set to System.Text.DecoderExceptionFallback.</exception>
        /// 
        /// <exception cref="FormatException">The length of encrypted string,
        ///     ignoring white-space characters, is not zero or a multiple 4.
        ///     -or- The format of encrypted string is invalid. Encrypted string contains a non-base-64 character, more
        ///     than two padding characters, or a non-white space-character among the padding characters.</exception>
        Task<string> ResetMasterPasswordAsync(string encryptedLogin, string encryptedPassword, string rsaPublicKey, string rsaPrivateKey,
            [CallerArgumentExpression("encryptedLogin")] string firstParamName = null,
            [CallerArgumentExpression("encryptedPassword")] string secondParamName = null);

        /// <summary>
        /// Retrieves user's master password and salt by account identificator
        /// </summary>
        /// 
        /// <param name="encryptedLogin">User account username encrypted with RSA algorithm.</param>
        /// <param name="rsaPrivateKey">RSA private key.</param>
        /// 
        /// <exception cref="ArgumentException">The encryptedLogin parameter value not found
        ///     -or- decrypted byte array contains invalid Unicode code points.</exception>
        /// 
        /// <exception cref="ArgumentNullException">RSA private key is null
        ///     -or- one of RSA private key parameters in null
        ///     -or- encryptedLogin is null.</exception>
        ///     
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.
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
        /// 
        /// <returns>SimplePM.WebAPI.Library.Models.UserData object that includes account identificator, master password in salted and hashed view and it's salt</returns>
        Task<UserData> RetrieveMasterPasswordAsync(string encryptedLogin, string rsaPrivateKey, [CallerArgumentExpression("encryptedLogin")] string paramName = null);

        /// <summary>
        /// Updates login to corresponding database entry
        /// </summary>
        /// 
        /// <param name="encryptedCurrentLogin">User account current username encrypted with RSA algorithm.</param>
        /// <param name="encryptedNewLogin">New username encrypted with RSA algorithm.</param>
        /// <param name="encryptedCurrentPassword">Current account password encrypted with RSA algorithm.</param>
        /// <param name="rsaPrivateKey">RSA private key.</param>
        /// 
        /// <exception cref="ArgumentException">The encryptedLogin parameter value not found or passwords do not match 
        ///     -or- decrypted byte array contains invalid Unicode code points.</exception>
        /// 
        /// <exception cref="ArgumentNullException">RSA private key is null
        ///     -or- one of RSA private key parameters in null
        ///     -or- encryptedNewLogin is null
        ///     -or- encryptedCurrentLogin is null
        ///     -or- encryptedCurrentPassword is null.</exception>
        /// 
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.
        ///     -or- The fOAEP parameter is true and the length of the rgb parameter
        ///     is greater than System.Security.Cryptography.RSACryptoServiceProvider.KeySize.
        ///     -or- The key does not match the encrypted data. However, the exception wording
        ///     may not be accurate. For example, it may say Not enough storage is available
        ///    to process this command.</exception>
        /// 
        /// <exception cref="DBOperationException">Unexpected amount of affected string on database operation.</exception>
        /// 
        /// <exception cref="IntermediateStorageException">Operation on intermediate storage failed.</exception>
        /// 
        /// <exception cref="System.Text.DecoderFallbackException">A fallback occurred (for more information, see Character Encoding in .NET)
        ///     -and- System.Text.Encoding.DecoderFallback is set to System.Text.DecoderExceptionFallback.</exception>
        /// 
        /// <exception cref="FormatException">The length of encrypted string,
        ///     ignoring white-space characters, is not zero or a multiple 4.
        ///     -or- The format of encrypted string is invalid. Encrypted string contains a non-base-64 character, more
        ///     than two padding characters, or a non-white space-character among the padding characters.</exception>
        Task UpdateAccountLoginAsync(string encryptedNewLogin, string encryptedCurrentLogin, string encryptedCurrentPassword, string rsaPrivateKey,
            [CallerArgumentExpression("encryptedCurrentLogin")] string loginParamName = null,
            [CallerArgumentExpression("encryptedCurrentPassword")] string passwordParamName = null);

        /// <summary>
        /// Updates password to corresponding database entry
        /// </summary>
        /// 
        /// <param name="encryptedCurrentLogin">User account username encrypted with RSA algorithm.</param>
        /// <param name="encryptedNewPassword">New password encrypted with RSA algorithm.</param>
        /// <param name="encryptedCurrentPassword">Current account password encrypted with RSA algorithm.</param>
        /// <param name="rsaPrivateKey">RSA private key</param>
        /// 
        /// <exception cref="ArgumentException">The accountID parameter value not found or passwords do not match
        ///     -or- decrypted byte array contains invalid Unicode code points.</exception>
        /// 
        /// <exception cref="ArgumentNullException">RSA private key is null
        ///     -or- one of RSA private key parameters in null
        ///     -or- encryptedNewPassword is null
        ///     -or- encryptedCurrentPassword is null.</exception>
        /// 
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.
        ///     -or- The fOAEP parameter is true and the length of the rgb parameter
        ///     is greater than System.Security.Cryptography.RSACryptoServiceProvider.KeySize.
        ///     -or- The key does not match the encrypted data. However, the exception wording
        ///     may not be accurate. For example, it may say Not enough storage is available
        ///    to process this command.</exception>
        /// 
        /// <exception cref="DBOperationException">Unexpected amount of affected string on database operation.</exception>
        /// 
        /// <exception cref="IntermediateStorageException">Operation on intermediate storage failed.</exception>
        /// 
        /// <exception cref="System.Text.DecoderFallbackException">A fallback occurred (for more information, see Character Encoding in .NET)
        ///     -and- System.Text.Encoding.DecoderFallback is set to System.Text.DecoderExceptionFallback.</exception>
        /// 
        /// <exception cref="FormatException">The length of encrypted string,
        ///     ignoring white-space characters, is not zero or a multiple 4.
        ///     -or- The format of encrypted string is invalid. Encrypted string contains a non-base-64 character, more
        ///     than two padding characters, or a non-white space-character among the padding characters.</exception>
        Task UpdateAccountPasswordAsync(string encryptedNewPassword, string encryptedCurrentLogin, string encryptedCurrentPassword, string rsaPrivateKey,
            [CallerArgumentExpression("encryptedCurrentLogin")] string loginParamName = null,
            [CallerArgumentExpression("encryptedCurrentPassword")] string passwordParamName = null);

        /// <summary>
        /// Updates master password and it's salt to corresponding entry in database
        /// </summary>
        /// 
        /// <param name="encryptedCurrentLogin">User account username encrypted with RSA algorithm.</param>
        /// <param name="encryptedOperationCode">Operation identificator encrypted with RSA algorithm.</param>
        /// <param name="encryptedNewMasterPass">New master password encrypted with RSA algorithm.</param>
        /// <param name="rsaPrivateKey">RSA private key</param>
        /// 
        /// <exception cref="ArgumentException">The accountID parameter value not found or passwords do not match
        ///     -or- decrypted byte array contains invalid Unicode code points.</exception>
        /// 
        /// <exception cref="ArgumentNullException">RSA private key is null
        ///     -or- one of RSA private key parameters in null
        ///     -or- encryptedLogin is null
        ///     -or- encryptedPassword is null.</exception>
        /// 
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.
        ///     -or- The fOAEP parameter is true and the length of the rgb parameter
        ///     is greater than System.Security.Cryptography.RSACryptoServiceProvider.KeySize.
        ///     -or- The key does not match the encrypted data. However, the exception wording
        ///     may not be accurate. For example, it may say Not enough storage is available
        ///    to process this command.</exception>
        /// 
        /// <exception cref="DBOperationException">Unexpected amount of affected string on database operation.</exception>
        /// 
        /// <exception cref="IntermediateStorageException">Operation on intermediate storage failed.</exception>
        /// 
        /// <exception cref="System.Text.DecoderFallbackException">A fallback occurred (for more information, see Character Encoding in .NET)
        ///     -and- System.Text.Encoding.DecoderFallback is set to System.Text.DecoderExceptionFallback.</exception>
        ///     
        /// <exception cref="System.Text.EncoderFallbackException">A fallback occurred (for more information, see Character Encoding in .NET) 
        ///     -and- System.Text.Encoding.EncoderFallback is set to System.Text.EncoderExceptionFallback.</exception>
        /// 
        /// <exception cref="FormatException">The length of encrypted string,
        ///     ignoring white-space characters, is not zero or a multiple 4.
        ///     -or- The format of encrypted string is invalid. Encrypted string contains a non-base-64 character, more
        ///     than two padding characters, or a non-white space-character among the padding characters.</exception>
        Task<UserData> SetMasterPasswordAsync(string encryptedCurrentLogin, string encryptedOperationCode, string encryptedNewMasterPass, string rsaPrivateKey,
            [CallerArgumentExpression("encryptedCurrentLogin")] string loginParamName = null,
            [CallerArgumentExpression("encryptedOperationCode")] string masterPasswordParamName = null);
    }
}