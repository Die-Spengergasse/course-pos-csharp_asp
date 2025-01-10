using AspTestHelpers;
using Languageweek.Application.Infrastructure;
using Languageweek.Application.Services;
using NSubstitute;

namespace Languageweek.Test;

public class MockingTests
{
    private readonly TestWebApplicationFactory<LanguageweekContext> _factory;
    private readonly ILanguageweekService _serviceMock = Substitute.For<ILanguageweekService>();
    public MockingTests()
    {
        _factory = new TestWebApplicationFactory<LanguageweekContext>();
        _factory.SubstituteService<ILanguageweekService>(provider => _serviceMock);
    }
    private void GenerateFixtures()
    {
        // TODO: Generate some fixtures for your tests by
        //       mocking your IQueryable in ILanguageweekService.
    }

    // TODO: Add your tests.
}
