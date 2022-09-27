using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using SimplePM.WebAPI.Library.Models;
using SimplePM.WebAPI.Library.Processing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimplePM.WebAPI.Controllers
{
    [ApiController]
    [Route("api/v1/entries/sync")]
    public class EntriesWebController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;
        private readonly IEntriesProcessor _processor;

        public EntriesWebController(IEntriesProcessor processor, IHttpClientFactory httpClientFactory, ILogger logger)
        {
            _processor = processor;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        
        [Route("checklist")]
        [HttpGet]
        [ProducesResponseType(typeof(List<Entry>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RetrieveChecklistAsync([FromHeader, Required] string accountID)
        {
            if (string.IsNullOrWhiteSpace(accountID))
            {
                return BadRequest("The account identificator provided is invalid or missing.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                List<Entry> checklist = await _processor.GetChecklistAsync(accountID);
                return Ok(checklist);
            }
            catch (ArgumentException ex)
            {
                switch (ex.ParamName)
                {
                    case nameof(accountID):
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

        [Route("updatelist")]
        [HttpGet]
        [ProducesResponseType(typeof(List<Entry>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RetrieveUpdatelistAsync([FromHeader, Required] string accountID, [FromHeader, Required] string[] idList)
        {
            if (string.IsNullOrWhiteSpace(accountID))
            {
                return BadRequest("The account identificator provided is invalid or missing.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                List<Entry> checklist = _processor.GetUpdatelistAsync(accountID, idList);
                return Ok(checklist);
            }
            catch (ArgumentException ex)
            {
                switch (ex.ParamName)
                {
                    case nameof(accountID):
                        return BadRequest(nameof(accountID));
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

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CommintChangesToDBAsync([FromHeader, Required] string accountID, [FromBody, Required] List<Entry> updatesList)
        {
            if (string.IsNullOrWhiteSpace(accountID))
            {
                return BadRequest("The username provided is invalid or missing.");
            }
            if (updatesList is null)
            {
                return BadRequest("A list of Entry entities that contains data about entries changed on client side is missing.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                if (await _processor.TryCommitChangesAsync(accountID, updatesList))
                {
                    return NoContent();
                }
                else
                {
                    return UnprocessableEntity("Some of entries provided to be updated or deleted were not found." +
                        "It may occur due to deletion of this entries with another client while this client was offline." + 
                        "All entries found have been updated. If the problem persists, please contact software developer.");
                }
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, ex.GetType().ToString());
                return Problem(DefaultMessagesProvider.InternalServerError);
            }
        }


    }
}
