using Acme.WebApi.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Web.Core.Mvc;
using Web.Core.WebApi.Controllers;

namespace Acme.WebApi.Controllers
{
    /// <summary>
    /// Controller (v2) to produce some test data and senario's for the WebApi.Core
    /// </summary>
    [ApiController]
    [ApiVersion("2")]
    [Route("api/v{version:apiVersion}/data")]
    public class DataControllerV2 : ApiControllerBase
    {
        private readonly ILogger<DataController> _logger;

        public DataControllerV2(ILogger<DataController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Serveral Error implementations of version 2 of the versioned backend-to-backend service.
        /// </summary>
        /// <remarks>
        /// Supported arguments: 0 is default value and the Ok case, 1..3 give errors.
        /// 
        /// * Ok result - returns array of two strings
        /// * 1: 500 - InternalServerErrror, An uncatched exception
        /// * 2: 500 - InternalServerErrror, An uncatched exception with inner exceptions
        /// * 3: 500 - InternalServerErrror, An uncatched exception with different inner exceptions
        /// </remarks>
        /// <param name="value">A value 1..3 for different error conditions.</param>
        /// <returns>An array with two sample strings.</returns>
        [HttpGet("exceptions/{value}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ExceptionProblemDetails), StatusCodes.Status500InternalServerError)]
        public IActionResult Errors(int value = 0)
        {
            switch (value)
            {
                case 1:
                    throw new ArgumentNullException("some parameter", "parameter can't be null");
                case 2:
                    throw new Exception("Lorem Ipsum 1", new ArgumentException("Honda Magna 2", "argument", new ArgumentNullException("Dolor Sit Amet 3", new AggregateException(new[] { new Exception("Lorem Ipsum 4-1", new ArgumentException("Honda Magna 4-2", "argument", new ArgumentNullException("Dolor Sit Amet 4-3"))), new OutOfMemoryException("my memory is out 5", new Exception("need more mem! 6")), new ArgumentOutOfRangeException("myParam", 7, "some message 7") }))));
                case 3:
                    throw new AggregateException(new[] { new Exception("Lorem Ipsum 1-1", new ArgumentException("Honda Magna 1-2", "argument", new ArgumentNullException("Dolor Sit Amet 1-3"))), new OutOfMemoryException("my memory is out 2", new Exception("need more mem! 2-1")), new ArgumentOutOfRangeException("myParam", 3, "some message 3") });
                default:
                    break;
            }

            return Ok(new string[] {
                "Value 1 from Versioned API version 2",
                "Value 2 from Versioned API version 2"
            });
        }

        /// <summary>
        /// Serveral Bad request implementations of version 2 of the versioned backend-to-backend service.
        /// </summary>
        /// <remarks>
        /// Supported arguments: 0 is default value and the Ok case, 1..2 give errors.
        /// 
        /// * Ok result - returns array of two strings
        /// * 1: 400 - BadRequest with modelstate errors
        /// * 2: 400 - BadRequest with details of `AcmeDataErrorDetails`
        /// </remarks>
        /// <param name="value">A value 1..2 for different error conditions.</param>
        /// <returns>An array with two sample strings.</returns>
        [HttpGet("badrequests/{value}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorProblemDetails<AcmeDataErrorDetails>), StatusCodes.Status400BadRequest)]
        public IActionResult BadRequests(int value = 0)
        {
            switch (value)
            {
                case 1:
                    ModelState.AddModelError("modelState1", "This is a invalid model 1-1 ");
                    ModelState.AddModelError("ModelState1", "This is another invalid model 1-2");
                    ModelState.AddModelError("modelState2", "This is another invalid model 2");

                    return BadRequest(modelState: ModelState);

                case 2:
                    return BadRequest(new AcmeDataErrorDetails
                    {
                        BooleanValue = true,
                        DateValue = DateTime.Now,
                        DecimalValue = 26.3M,
                        IntValue = 42,
                        StringValue = "Lorem Ipsum Honda Magna",
                    });
            }

            return Ok(new string[] {
                "Value 1 from Versioned API version 2",
                "Value 2 from Versioned API version 2"
            });
        }

        /// <summary>
        /// Serveral ErrorDetails implementations of version 2 of the versioned backend-to-backend service.
        /// </summary>
        /// <remarks>
        /// Supported arguments: 0 is default value and the Ok case, 1..2 give errors.
        /// 
        /// * Ok result - returns array of two strings
        /// * 1: 417 - ExpectationFailed with error details of type `AcmeErrorDetails`
        /// * 2: 418 - I'm a Teapot with error details of type `AcmeDataErrorDetails`
        /// </remarks>
        /// <param name="value">A value 1..2 for different error conditions.</param>
        /// <returns>An array with two sample strings.</returns>
        [HttpGet("errordetails/{value}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorProblemDetails<AcmeDataErrorDetails>), StatusCodes.Status418ImATeapot)]
        [ProducesResponseType(typeof(ErrorProblemDetails<AcmeErrorDetails>), StatusCodes.Status417ExpectationFailed)]
        public IActionResult ErrorDetails(int value = 0)
        {
            switch (value)
            {
                case 1:
                    return ErrorDetails(new AcmeErrorDetails
                    {
                        IntValue = 42,
                        StringValue = "Lorem Ipsum Honda Magna",
                    },
                    StatusCodes.Status417ExpectationFailed,
                    "Expectation Failed");

                case 2:
                    return ErrorDetails(new AcmeDataErrorDetails
                    {
                        BooleanValue = true,
                        DateValue = DateTime.Now,
                        DecimalValue = 26.3M,
                        IntValue = 42,
                        StringValue = "Lorem Ipsum Honda Magna",
                    },
                    StatusCodes.Status418ImATeapot,
                    "I'm a teapot......");
            }

            return Ok(new string[] {
                "Value 1 from Versioned API version 2",
                "Value 2 from Versioned API version 2"
            });
        }
    }
}
