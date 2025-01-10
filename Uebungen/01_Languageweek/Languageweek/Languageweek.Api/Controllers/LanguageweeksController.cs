using Languageweek.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Languageweek.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LanguageweeksController(ILanguageweekService languageweekService) : ControllerBase
{
    // TODO: Add your implementation.
}
