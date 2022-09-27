using SimplePM.WebAPI.Library.Cryptography;
using SimplePM.WebAPI.Library.Models;
using SimplePM.WebAPI.Library.Repositories;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SimplePM.WebAPI.Library.Processing
{
    public class AccountProcessor : IAccountProcessor
    {
        private readonly IAccountRepository _repository;

        public AccountProcessor(IAccountRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> IsLoginAvailableAsync(string encryptedLogin, string rsaPrivateKey)
        {
            string login = CryptographyProvider.RSA.Decrypt(encryptedLogin, rsaPrivateKey);

            var existing = await _repository.RetrieveAsync(login);
            if (existing is not null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public async Task<UserData> AuthenticateAsync(string encryptedLogin, string encryptedPassword, string rsaPrivateKey,
            [CallerArgumentExpression("encryptedLogin")] string firstParamName = null,
            [CallerArgumentExpression("encryptedPassword")] string secondParamName = null)
        {
            string login = CryptographyProvider.RSA.Decrypt(encryptedLogin, rsaPrivateKey);
            string password = CryptographyProvider.RSA.Decrypt(encryptedPassword, rsaPrivateKey);

            var existing = await _repository.RetrieveAsync(login);
            if (existing is null)
            {
                throw new ArgumentException("", firstParamName);
            }

            string passwordHash = CryptographyProvider.SHA256.SaltAndHashString(password, existing.Salt);
            if (passwordHash == existing.Password)
            {
                UserData authData = new(existing.ID, existing.MasterPassword, existing.MasterSalt);
                return authData;
            }
            else
            {
                throw new ArgumentException("", secondParamName);
            }
        }

        public async Task<UserData> RegisterAsync(UserData encryptedAccountData, string rsaPrivateKey)
        {
            string newUserID = Guid.NewGuid().ToString("N");
            encryptedAccountData.DecryptPrivateData(rsaPrivateKey);

            // Actually no longer encrypted
            encryptedAccountData.ID = newUserID;
            encryptedAccountData.HashAccountPassword();
            encryptedAccountData.HashMasterPassword();
            UserData newAccountData = await _repository.CreateAsync(encryptedAccountData);
            
            return new UserData(newAccountData.ID, newAccountData.MasterPassword, newAccountData.MasterSalt);
        }

        public async Task UpdateAccountPasswordAsync(string encryptedNewPassword, 
            string encryptedCurrentLogin,
            string encryptedCurrentPassword, 
            string rsaPrivateKey,
            [CallerArgumentExpression("encryptedCurrentLogin")] string loginParamName = null,
            [CallerArgumentExpression("encryptedCurrentPassword")] string passwordParamName = null)
        {
            string newPassword = CryptographyProvider.RSA.Decrypt(encryptedNewPassword, rsaPrivateKey);
            string currentLogin = CryptographyProvider.RSA.Decrypt(encryptedCurrentLogin, rsaPrivateKey);
            string currentPassword = CryptographyProvider.RSA.Decrypt(encryptedCurrentPassword, rsaPrivateKey);

            var existing = await _repository.RetrieveAsync(currentLogin);
            if (existing is null)
            {
                throw new ArgumentException("", loginParamName);
            }

            string saltedAndHashedCurrentPassword = CryptographyProvider.SHA256.SaltAndHashString(currentPassword, existing.Salt);
            if (existing.Password == saltedAndHashedCurrentPassword)
            {
                existing.Password = newPassword;
                existing.Salt = null;
                existing.HashAccountPassword();
                if (await _repository.UpdateAsync(existing.ID, existing) is null)
                {
                    throw new IntermediateStorageException();
                }
            }
            else
            {
                throw new ArgumentException("", passwordParamName);
            }
        }

        public async Task DeleteAccountAsync(string encryptedLogin, string encryptedPassword, string rsaPrivateKey,
            [CallerArgumentExpression("encryptedLogin")] string firstParamName = null,
            [CallerArgumentExpression("encryptedPassword")] string secondParamName = null)
        {
            string login = CryptographyProvider.RSA.Decrypt(encryptedLogin, rsaPrivateKey);
            string password = CryptographyProvider.RSA.Decrypt(encryptedPassword, rsaPrivateKey);

            var existing = await _repository.RetrieveAsync(login);
            if (existing is null)
            {
                throw new ArgumentException("", firstParamName);
            }

            string saltedAndHashedCurrentPassword = CryptographyProvider.SHA256.SaltAndHashString(password, existing.Salt);
            if (existing.Password == saltedAndHashedCurrentPassword)
            {
                bool deleted = await _repository.DeleteAsync(existing.ID);
                if (!deleted)
                {
                    throw new IntermediateStorageException();
                }
            }
            else
            {
                throw new ArgumentException("", secondParamName);
            }
        }

        #region MasterPassword

        public async Task<UserData> RetrieveMasterPasswordAsync(string encryptedLogin,
            string rsaPrivateKey,
            [CallerArgumentExpression("encryptedLogin")] string paramName = null)
        {
            string login = CryptographyProvider.RSA.Decrypt(encryptedLogin, rsaPrivateKey);

            var existing = await _repository.RetrieveAsync(login);
            if (existing is null)
            {
                throw new ArgumentException("", paramName);
            }

            UserData masterPassData = new(existing.MasterPassword, existing.MasterSalt);
            return masterPassData;
        }

        public async Task<UserData> SetNewMasterPasswordAsync(string encryptedLogin,
            string encryptedCurrentMasterPassOrOperationCode,
            string encryptedNewMasterPass,
            string rsaPrivateKey,
            [CallerArgumentExpression("encryptedLogin")] string loginParamName = null,
            [CallerArgumentExpression("encryptedCurrentMasterPassOrOperationCode")] string masterPasswordParamName = null)
        {
            string currentLogin = CryptographyProvider.RSA.Decrypt(encryptedLogin, rsaPrivateKey);
            string currentMasterPass = CryptographyProvider.RSA.Decrypt(encryptedCurrentMasterPassOrOperationCode, rsaPrivateKey);
            string newMasterPass = CryptographyProvider.RSA.Decrypt(encryptedNewMasterPass, rsaPrivateKey);

            var existing = await _repository.RetrieveAsync(currentLogin);
            if (existing is null)
            {
                throw new ArgumentException("", loginParamName);
            }

            if (existing.MasterPassword == currentMasterPass)
            {
                existing.MasterPassword = newMasterPass;
                existing.MasterSalt = null;
                existing.HashMasterPassword();
                var updatedMasterPassData = await _repository.UpdateAsync(existing.ID, existing);
                if (updatedMasterPassData is not null)
                {
                    return new UserData(updatedMasterPassData.MasterPassword, updatedMasterPassData.MasterSalt);
                }
                else
                {
                    throw new IntermediateStorageException();
                }
            }
            else
            {
                throw new ArgumentException("", masterPasswordParamName);
            }
        }

        public async Task<string> ResetMasterPasswordAsync(string encryptedLogin, string encryptedPassword, string rsaPublicKey, string rsaPrivateKey,
            [CallerArgumentExpression("encryptedLogin")] string firstParamName = null,
            [CallerArgumentExpression("encryptedPassword")] string secondParamName = null)
        {
            string login = CryptographyProvider.RSA.Decrypt(encryptedLogin, rsaPrivateKey);
            string password = CryptographyProvider.RSA.Decrypt(encryptedPassword, rsaPrivateKey);

            var existing = await _repository.RetrieveAsync(login);
            if (existing is null)
            {
                throw new ArgumentException("", firstParamName);
            }

            string saltedAndHashedPassword = CryptographyProvider.SHA256.SaltAndHashString(password, existing.Salt);
            if (saltedAndHashedPassword == existing.Password)
            {
                string operationCode = Guid.NewGuid().ToString("N");
                existing.MasterPassword = operationCode;
                existing.MasterSalt = string.Empty;
                if (await _repository.UpdateAsync(existing.ID, existing) is not null)
                {
                    return CryptographyProvider.RSA.Encrypt(operationCode, rsaPublicKey);
                }
                else
                {
                    throw new IntermediateStorageException();
                }
            }
            else
            {
                throw new ArgumentException("", secondParamName);
            }
        }

        #endregion
    }
}
