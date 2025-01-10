using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspShowcase.Controllers
{
    /// <summary>
    /// SayHelloController reagiert auf alles,
    /// das die Adresse /api/sayhello/... hat. 
    /// </summary>
    [ApiController]                 // ASP.NET soll diese Klasse berücksichtigen
    [Route("api/[controller]")]    // Adresse ist /api/sayhello da der Controller 
                                    // SayHelloController heißt
                                    // (controller wird weggeschnitten)
    public class SayhelloController : ControllerBase
    {
        /// <summary>
        /// /api/sayhello
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetHelloString()
        {
            return Ok("Hello World");
        }
        /// <summary>
        /// /api/sayhello/michael
        /// </summary>
        [HttpGet("{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetHelloString(string name)
        {
            return Ok($"Hello {name}");
        }
    }
}
