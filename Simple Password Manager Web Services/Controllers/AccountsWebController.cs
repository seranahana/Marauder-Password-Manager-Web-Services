using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using SimplePM.WebAPI.Library;
using SimplePM.WebAPI.Library.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Threading.Tasks;
using SimplePM.WebAPI.Library.Processing;

namespace SimplePM.WebAPI.Controllers
{
    [ApiController]
    [Route("api/v1/accounts")]
    public class AccountsWebController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;
        private readonly IAccountProcessor _processor;

        public AccountsWebController(ILogger logger, IHttpClientFactory httpClientFactory, IAccountProcessor processor)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _processor = processor;
        }

        [Route("login/availability")]
        [HttpGet]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CheckLoginAvailabilityAsync([FromHeader, Required] string encryptedLogin)
        {
            if (string.IsNullOrWhiteSpace(encryptedLogin))
            {
                return BadRequest(DefaultMessagesProvider.GetCorruptedOrMissingMessage(Params.Username));
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                bool isAvailable = await _processor.IsLoginAvailableAsync(encryptedLogin, Program.PrivateKey);
                return Ok(isAvailable);
            }
            catch (Exception ex) when (ex is FormatException || ex is System.Security.Cryptography.CryptographicException)
            {
                return BadRequest(DefaultMessagesProvider.EncryptionRequired);
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, ex.GetType().ToString());
                return Problem(DefaultMessagesProvider.InternalServerError);
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(UserData), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AuthenticateAsync([FromHeader, Required] string encryptedLogin, [FromHeader, Required] string encryptedPassword)
        {
            if (string.IsNullOrWhiteSpace(encryptedLogin))
            {
                return BadRequest(DefaultMessagesProvider.GetCorruptedOrMissingMessage(Params.Username));
            }
            if (string.IsNullOrWhiteSpace(encryptedPassword))
            {
                return BadRequest(DefaultMessagesProvider.GetCorruptedOrMissingMessage(Params.AccountPassword));
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                UserData authData = await _processor.AuthenticateAsync(encryptedLogin, encryptedPassword, Program.PrivateKey);
                return Ok(authData);
            }
            catch (ArgumentException ex)
            {
                switch (ex.ParamName)
                {
                    case nameof(encryptedLogin):
                        return NotFound();
                    case nameof(encryptedPassword):
                        var (Key, Value) = DefaultHeadersProvider.AuthorizationHeader;
                        HttpContext.Response.Headers.Add(Key, Value);
                        return Unauthorized(DefaultMessagesProvider.GetUnauthorizedIncorrectMessage(Params.AccountPassword));
                    default:
                        _logger.Fatal(ex, ex.GetType().ToString());
                        return Problem(DefaultMessagesProvider.InternalServerError);
                }
            }
            catch (Exception ex) when (ex is FormatException || ex is System.Security.Cryptography.CryptographicException)
            {
                return BadRequest(DefaultMessagesProvider.EncryptionRequired);
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, ex.GetType().ToString());
                return Problem(DefaultMessagesProvider.InternalServerError);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(UserData), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegisterAsync([FromBody, Required] UserData encryptedAccountData)
        {
            if (encryptedAccountData is null)
            {
                return BadRequest(DefaultMessagesProvider.GetCorruptedOrMissingMessage(Params.UserDataModel));
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                UserData newAccountData = await _processor.RegisterAsync(encryptedAccountData, Program.PrivateKey);
                _logger.Information("User {NewUserID} created", newAccountData.ID);
                return Created("api/v1/accounts", newAccountData);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                if (ex.InnerException.Message.Contains("UNIQUE constraint failed") && ex.InnerException.Message.Contains("login"))
                {
                    return BadRequest("Username already occupied. Please enter a different user name.");
                }
                else
                {
                    _logger.Fatal(ex, ex.GetType().ToString());
                    return Problem(DefaultMessagesProvider.InternalServerError);
                }
            }
            catch (Exception ex) when (ex is FormatException || ex is System.Security.Cryptography.CryptographicException)
            {
                return BadRequest(DefaultMessagesProvider.EncryptionRequired);
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, ex.GetType().ToString());
                return Problem(DefaultMessagesProvider.InternalServerError);
            }
        }

        [HttpPatch]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangeAccountPasswordAsync([FromHeader, Required] string encryptedNewPassword, 
            [FromHeader, Required] string encryptedCurrentLogin,
            [FromHeader, Required] string encryptedCurrentPassword)
        {
            if (string.IsNullOrWhiteSpace(encryptedCurrentLogin))
            {
                return BadRequest(DefaultMessagesProvider.GetCorruptedOrMissingMessage(Params.Username));
            }
            if (string.IsNullOrWhiteSpace(encryptedNewPassword))
            {
                return BadRequest(DefaultMessagesProvider.GetCorruptedOrMissingMessage(Params.NewAccountPassword));
            }
            if (string.IsNullOrWhiteSpace(encryptedCurrentPassword))
            {
                return BadRequest(DefaultMessagesProvider.GetCorruptedOrMissingMessage(Params.AccountPassword));
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                await _processor.UpdateAccountPasswordAsync(encryptedCurrentLogin, encryptedNewPassword, encryptedCurrentPassword, Program.PrivateKey);
                return new NoContentResult();
            }
            catch (ArgumentException ex)
            {
                switch (ex.ParamName)
                {
                    case nameof(encryptedCurrentLogin):
                        return NotFound();
                    case nameof(encryptedCurrentPassword):
                        var (Key, Value) = DefaultHeadersProvider.AuthorizationHeader;
                        HttpContext.Response.Headers.Add(Key, Value);
                        return Unauthorized(DefaultMessagesProvider.GetUnauthorizedIncorrectMessage(Params.AccountPassword));
                    default:
                        _logger.Fatal(ex, ex.GetType().ToString());
                        return Problem(DefaultMessagesProvider.InternalServerError);
                }
            }
            catch (Exception ex) when (ex is FormatException || ex is System.Security.Cryptography.CryptographicException)
            {
                return BadRequest(DefaultMessagesProvider.EncryptionRequired);
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, ex.GetType().ToString());
                return Problem(DefaultMessagesProvider.InternalServerError);
            }
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveUserAsync([FromHeader, Required] string encryptedLogin, [FromHeader, Required] string encryptedPassword)
        {
            if (string.IsNullOrWhiteSpace(encryptedLogin))
            {
                return BadRequest(DefaultMessagesProvider.GetCorruptedOrMissingMessage(Params.Username));
            }
            if (string.IsNullOrWhiteSpace(encryptedPassword))
            {
                return BadRequest(DefaultMessagesProvider.GetCorruptedOrMissingMessage(Params.AccountPassword));
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                await _processor.DeleteAccountAsync(encryptedLogin, encryptedPassword, Program.PrivateKey);
                return new NoContentResult();
            }
            catch (ArgumentException ex)
            {
                switch (ex.ParamName)
                {
                    case nameof(encryptedLogin):
                        return NotFound();
                    case nameof(encryptedPassword):
                        var (Key, Value) = DefaultHeadersProvider.AuthorizationHeader;
                        HttpContext.Response.Headers.Add(Key, Value);
                        return Unauthorized(DefaultMessagesProvider.GetUnauthorizedIncorrectMessage(Params.AccountPassword));
                    default:
                        _logger.Fatal(ex, ex.GetType().ToString());
                        return Problem(DefaultMessagesProvider.InternalServerError);
                }
            }
            catch (Exception ex) when (ex is FormatException || ex is System.Security.Cryptography.CryptographicException)
            {
                return BadRequest(DefaultMessagesProvider.EncryptionRequired);
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, ex.GetType().ToString());
                return Problem(DefaultMessagesProvider.InternalServerError);
            }
        }

        #region MasterPassword

        [Route("master")]
        [HttpGet]
        [ProducesResponseType(typeof(UserData), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMasterPasswordAsync([FromHeader, Required] string encryptedLogin)
        {
            if (string.IsNullOrWhiteSpace(encryptedLogin))
            {
                return BadRequest(DefaultMessagesProvider.GetCorruptedOrMissingMessage(Params.Username));
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var existing = await _processor.RetrieveMasterPasswordAsync(encryptedLogin, Program.PrivateKey);
                return Ok(existing);
            }
            catch (ArgumentException ex)
            {
                switch (ex.ParamName)
                {
                    case nameof(encryptedLogin):
                        return NotFound();
                    default:
                        _logger.Fatal(ex, ex.GetType().ToString());
                        return Problem(DefaultMessagesProvider.InternalServerError);
                }
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, ex.GetType().ToString());
                return Problem(DefaultMessagesProvider.InternalServerError);
            }
        }

        [Route("master")]
        [HttpPost]
        [ProducesResponseType(typeof(UserData), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SetNewMasterPasswordAsync([FromHeader, Required] string encryptedCurrentLogin,
            [FromHeader, Required] string encryptedCurrentMasterPassOrOperationCode,
            [FromHeader, Required] string encryptedNewMasterPass)
        {
            if (string.IsNullOrWhiteSpace(encryptedCurrentLogin))
            {
                return BadRequest(DefaultMessagesProvider.GetCorruptedOrMissingMessage(Params.Username));
            }
            if (string.IsNullOrWhiteSpace(encryptedNewMasterPass))
            {
                return BadRequest(DefaultMessagesProvider.GetCorruptedOrMissingMessage(Params.NewMasterPassword));
            }
            if (string.IsNullOrWhiteSpace(encryptedCurrentMasterPassOrOperationCode))
            {
                return BadRequest(DefaultMessagesProvider.GetCorruptedOrMissingMessage(Params.MasterPasswordOrOperationCode));
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                UserData newMasterPassData = await _processor.SetNewMasterPasswordAsync(encryptedCurrentLogin, encryptedCurrentMasterPassOrOperationCode, encryptedNewMasterPass, Program.PrivateKey);
                return Created("api/v1/accounts/master", newMasterPassData);
            }
            catch (ArgumentException ex)
            {
                switch (ex.ParamName)
                {
                    case nameof(encryptedCurrentLogin):
                        return NotFound();
                    case nameof(encryptedCurrentMasterPassOrOperationCode):
                        var (Key, Value) = DefaultHeadersProvider.AuthorizationHeader;
                        HttpContext.Response.Headers.Add(Key, Value);
                        return Unauthorized(DefaultMessagesProvider.GetUnauthorizedIncorrectMessage(Params.MasterPasswordOrOperationCode));
                    default:
                        _logger.Fatal(ex, ex.GetType().ToString());
                        return Problem(DefaultMessagesProvider.InternalServerError);
                }
            }
            catch (Exception ex) when (ex is FormatException || ex is System.Security.Cryptography.CryptographicException)
            {
                return BadRequest(DefaultMessagesProvider.EncryptionRequired);
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, ex.GetType().ToString());
                return Problem(DefaultMessagesProvider.InternalServerError);
            }
        }


        [Route("master/reset")]
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResetMasterPasswordAsync([FromHeader, Required] string encryptedLogin,
            [FromHeader, Required] string encryptedPassword,
            [FromHeader, Required] string rsaPublicKey)
        {
            if (string.IsNullOrWhiteSpace(encryptedLogin))
            {
                return BadRequest(DefaultMessagesProvider.GetCorruptedOrMissingMessage(Params.Username));
            }
            if (string.IsNullOrWhiteSpace(rsaPublicKey))
            {
                return BadRequest(DefaultMessagesProvider.GetCorruptedOrMissingMessage(Params.RsaPublicKey));
            }
            if (string.IsNullOrWhiteSpace(encryptedPassword))
            {
                return BadRequest(DefaultMessagesProvider.GetCorruptedOrMissingMessage(Params.AccountPassword));
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                string encryptedOperationCode = await _processor.ResetMasterPasswordAsync(encryptedLogin, encryptedPassword, rsaPublicKey, Program.PrivateKey);
                return Accepted(encryptedOperationCode);
            }
            catch (ArgumentException ex)
            {
                switch (ex.ParamName)
                {
                    case nameof(encryptedLogin):
                        return NotFound();
                    case nameof(encryptedPassword):
                        var (Key, Value) = DefaultHeadersProvider.AuthorizationHeader;
                        HttpContext.Response.Headers.Add(Key, Value);
                        return Unauthorized(DefaultMessagesProvider.GetUnauthorizedIncorrectMessage(Params.AccountPassword));
                    default:
                        _logger.Fatal(ex, ex.GetType().ToString());
                        return Problem(DefaultMessagesProvider.InternalServerError);
                }
            }
            catch (Exception ex) when (ex is FormatException || ex is System.Security.Cryptography.CryptographicException)
            {
                return BadRequest(DefaultMessagesProvider.EncryptionRequired);
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, ex.GetType().ToString());
                return Problem(DefaultMessagesProvider.InternalServerError);
            }
        }

        #endregion
    }
}
