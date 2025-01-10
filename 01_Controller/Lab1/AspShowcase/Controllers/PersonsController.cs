using AspShowcase.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AspShowcase.Controllers
{
    /// <summary>
    /// Reagiert auf /api/persons
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PersonsController : ControllerBase
    {
        private readonly AspShowcaseContext _db;

        public PersonsController(AspShowcaseContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Reagiert auf GET /api/persons
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAllPersons()
        {
            var data = _db.Persons.ToList();
            return Ok(data);
        }

    }
}
