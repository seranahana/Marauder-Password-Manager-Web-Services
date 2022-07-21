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
                return BadRequest(DefaultMessages.GetCorruptedOrMissingMessage("username"));
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
                return BadRequest(DefaultMessages.EncryptionRequired);
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, ex.GetType().ToString());
                return Problem(DefaultMessages.InternalServerError);
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
                return BadRequest(DefaultMessages.GetCorruptedOrMissingMessage($"{nameof(UserData)} model"));
            }
            if (string.IsNullOrWhiteSpace(encryptedPassword))
            {
                var (Key, Value) = DefaultHeaders.AuthorizationHeader;
                HttpContext.Response.Headers.Add(Key, Value);
                return Unauthorized(DefaultMessages.GetUnauthorizedRequiredMessage("account password"));
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
                        var (Key, Value) = DefaultHeaders.AuthorizationHeader;
                        HttpContext.Response.Headers.Add(Key, Value);
                        return Unauthorized(DefaultMessages.GetUnauthorizedIncorrectMessage("password"));
                    default:
                        _logger.Fatal(ex, ex.GetType().ToString());
                        return Problem(DefaultMessages.InternalServerError);
                }
            }
            catch (Exception ex) when (ex is FormatException || ex is System.Security.Cryptography.CryptographicException)
            {
                return BadRequest(DefaultMessages.EncryptionRequired);
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, ex.GetType().ToString());
                return Problem(DefaultMessages.InternalServerError);
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
                return BadRequest(DefaultMessages.GetCorruptedOrMissingMessage($"{nameof(UserData)} model"));
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                string newUserID = await _processor.RegisterAsync(encryptedAccountData, Program.PrivateKey);
                _logger.Information("User {NewUserID} created", newUserID);
                return Created("api/v1/accounts", newUserID);
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
                    return Problem(DefaultMessages.InternalServerError);
                }
            }
            catch (Exception ex) when (ex is FormatException || ex is System.Security.Cryptography.CryptographicException)
            {
                return BadRequest(DefaultMessages.EncryptionRequired);
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, ex.GetType().ToString());
                return Problem(DefaultMessages.InternalServerError);
            }
        }

        [HttpPatch]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAccountDataAsync([FromHeader] string encryptedNewLogin, 
            [FromHeader] string encryptedNewPassword, 
            [FromHeader, Required] string encryptedCurrentLogin,
            [FromHeader, Required] string encryptedCurrentPassword)
        {
            if (string.IsNullOrWhiteSpace(encryptedCurrentLogin))
            {
                return BadRequest(DefaultMessages.GetCorruptedOrMissingMessage("current login"));
            }
            if (string.IsNullOrWhiteSpace(encryptedNewLogin) && string.IsNullOrWhiteSpace(encryptedNewPassword))
            {
                return BadRequest("No valid data have been recieved. Please verify your entry and try again.");
            }
            if (string.IsNullOrWhiteSpace(encryptedCurrentPassword))
            {
                var (Key, Value) = DefaultHeaders.AuthorizationHeader;
                HttpContext.Response.Headers.Add(Key, Value);
                return Unauthorized(DefaultMessages.GetUnauthorizedRequiredMessage("account password"));
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {

                if (!string.IsNullOrWhiteSpace(encryptedNewLogin))
                {
                    await _processor.UpdateAccountLoginAsync(encryptedCurrentLogin, encryptedNewLogin, encryptedCurrentPassword, Program.PrivateKey);
                }
                if (!string.IsNullOrWhiteSpace(encryptedNewPassword))
                {
                    await _processor.UpdateAccountPasswordAsync(encryptedCurrentLogin, encryptedNewPassword, encryptedCurrentPassword, Program.PrivateKey);
                }
                return new NoContentResult();
            }
            catch (ArgumentException ex)
            {
                switch (ex.ParamName)
                {
                    case nameof(encryptedCurrentLogin):
                        return NotFound();
                    case nameof(encryptedCurrentPassword):
                        var (Key, Value) = DefaultHeaders.AuthorizationHeader;
                        HttpContext.Response.Headers.Add(Key, Value);
                        return Unauthorized(DefaultMessages.GetUnauthorizedIncorrectMessage("password"));
                    default:
                        _logger.Fatal(ex, ex.GetType().ToString());
                        return Problem(DefaultMessages.InternalServerError);
                }
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
                    return Problem(DefaultMessages.InternalServerError);
                }
            }
            catch (Exception ex) when (ex is FormatException || ex is System.Security.Cryptography.CryptographicException)
            {
                return BadRequest(DefaultMessages.EncryptionRequired);
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, ex.GetType().ToString());
                return Problem(DefaultMessages.InternalServerError);
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
                return BadRequest(DefaultMessages.GetCorruptedOrMissingMessage("username"));
            }
            if (string.IsNullOrWhiteSpace(encryptedPassword))
            {
                var authorizationHeader = DefaultHeaders.AuthorizationHeader;
                HttpContext.Response.Headers.Add(authorizationHeader.Key, authorizationHeader.Value);
                return Unauthorized(DefaultMessages.GetUnauthorizedRequiredMessage("account password"));
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
                        var (Key, Value) = DefaultHeaders.AuthorizationHeader;
                        HttpContext.Response.Headers.Add(Key, Value);
                        return Unauthorized(DefaultMessages.GetUnauthorizedIncorrectMessage("password"));
                    default:
                        _logger.Fatal(ex, ex.GetType().ToString());
                        return Problem(DefaultMessages.InternalServerError);
                }
            }
            catch (Exception ex) when (ex is FormatException || ex is System.Security.Cryptography.CryptographicException)
            {
                return BadRequest(DefaultMessages.EncryptionRequired);
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, ex.GetType().ToString());
                return Problem(DefaultMessages.InternalServerError);
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
                return BadRequest(DefaultMessages.GetCorruptedOrMissingMessage("login"));
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
                        return Problem(DefaultMessages.InternalServerError);
                }
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, ex.GetType().ToString());
                return Problem(DefaultMessages.InternalServerError);
            }
        }

        [Route("master")]
        [HttpPost]
        [ProducesResponseType(typeof(UserData), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SetMasterPasswordAsync([FromHeader, Required] string encryptedCurrentLogin,
            [FromHeader, Required] string encryptedOperationCode,
            [FromHeader, Required] string encryptedNewMasterPass)
        {
            if (string.IsNullOrWhiteSpace(encryptedCurrentLogin))
            {
                return BadRequest(DefaultMessages.GetCorruptedOrMissingMessage("login"));
            }
            if (string.IsNullOrWhiteSpace(encryptedNewMasterPass))
            {
                return BadRequest(DefaultMessages.GetCorruptedOrMissingMessage("new master password"));
            }
            if (string.IsNullOrWhiteSpace(encryptedOperationCode))
            {
                var (Key, Value) = DefaultHeaders.AuthorizationHeader;
                HttpContext.Response.Headers.Add(Key, Value);
                return Unauthorized(DefaultMessages.GetUnauthorizedRequiredMessage("operation code"));
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                UserData newMasterPassData = await _processor.SetMasterPasswordAsync(encryptedCurrentLogin, encryptedOperationCode, encryptedNewMasterPass, Program.PrivateKey);
                return Created("api/v1/accounts/master", newMasterPassData);
            }
            catch (ArgumentException ex)
            {
                switch (ex.ParamName)
                {
                    case nameof(encryptedCurrentLogin):
                        return NotFound();
                    case nameof(encryptedOperationCode):
                        var (Key, Value) = DefaultHeaders.AuthorizationHeader;
                        HttpContext.Response.Headers.Add(Key, Value);
                        return Unauthorized(DefaultMessages.GetUnauthorizedIncorrectMessage("operation code"));
                    default:
                        _logger.Fatal(ex, ex.GetType().ToString());
                        return Problem(DefaultMessages.InternalServerError);
                }
            }
            catch (Exception ex) when (ex is FormatException || ex is System.Security.Cryptography.CryptographicException)
            {
                return BadRequest(DefaultMessages.EncryptionRequired);
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, ex.GetType().ToString());
                return Problem(DefaultMessages.InternalServerError);
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
                return BadRequest(DefaultMessages.GetCorruptedOrMissingMessage("username"));
            }
            if (string.IsNullOrWhiteSpace(rsaPublicKey))
            {
                return BadRequest(DefaultMessages.GetCorruptedOrMissingMessage("RSA public key"));
            }
            if (string.IsNullOrWhiteSpace(encryptedPassword))
            {
                var (Key, Value) = DefaultHeaders.AuthorizationHeader;
                HttpContext.Response.Headers.Add(Key, Value);
                return Unauthorized(DefaultMessages.GetUnauthorizedRequiredMessage("account password"));
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
                        var (Key, Value) = DefaultHeaders.AuthorizationHeader;
                        HttpContext.Response.Headers.Add(Key, Value);
                        return Unauthorized(DefaultMessages.GetUnauthorizedIncorrectMessage("password"));
                    default:
                        _logger.Fatal(ex, ex.GetType().ToString());
                        return Problem(DefaultMessages.InternalServerError);
                }
            }
            catch (Exception ex) when (ex is FormatException || ex is System.Security.Cryptography.CryptographicException)
            {
                return BadRequest(DefaultMessages.EncryptionRequired);
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, ex.GetType().ToString());
                return Problem(DefaultMessages.InternalServerError);
            }
        }

        #endregion
    }
}
