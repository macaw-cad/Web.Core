using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Web.Core.Mvc;
using Web.Core.WebApi.Controllers;

namespace Acme.WebApiLibrary.Data.Controllers
{
    [ApiController]
    [Route("api/data")]
    public class DataController : ApiControllerBase
    {
        private readonly ILogger<DataController> _logger;

        public DataController(ILogger<DataController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Minimal sample implementation of named backend-to-frontend/backend service.
        /// </summary>
        /// <remarks>
        /// Supported arguments: 0 is default value and the Ok case, 1..3 give errors.
        /// 
        /// * Ok result - returns array of two strings
        /// * 1: 500 - InternalServerErrror, some exception
        /// * 2: 400 - BadRequest with title
        /// * 3: 404 - NotFound
        /// </remarks>
        /// <param name="value">A value 0..3 for different error conditions.</param>
        /// <returns>An array with two sample strings.</returns>
        [HttpGet("")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ExceptionProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public IActionResult Data(int value = 0)
        {
            switch (value)
            {
                case 1:
                    throw new ArgumentException("some exception", new ArgumentOutOfRangeException("some param", "another inner exception"));

                case 2:
                    return BadRequest("some title", "more details");

                case 3:
                    return NotFound();
            }

            return Ok(new string[] {
                "Value 1 from Test.WebApp.WebApi version 1",
                "Value 2 from Test.WebApp.WebApi version 1"
            });
        }
    }
}
