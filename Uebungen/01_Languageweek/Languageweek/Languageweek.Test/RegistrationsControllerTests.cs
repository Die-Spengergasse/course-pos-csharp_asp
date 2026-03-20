using AspTestHelpers;
using Languageweek.Application.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using System;

namespace Languageweek.Test;

public class RegistrationsControllerTests
{
    private readonly TestWebApplicationFactory<LanguageweekContext> _factory;
    private readonly TimeProvider _timeProvider;
    public RegistrationsControllerTests()
    {
        _factory = new TestWebApplicationFactory<LanguageweekContext>("Testing");
        // TODO: Set a specific date that fits your test data.
        _timeProvider = new FakeTimeProvider(new DateTimeOffset(new DateTime(2026, 1, 1, 12, 0, 0)));
        _factory.SubstituteService<TimeProvider>(provider => _timeProvider, ServiceLifetime.Singleton);
    }
    private void GenerateFixtures()
    {
        _factory.InitializeDatabase(db =>
        {
            // TODO: Add minimal test data
        });
    }

    // TODO: Add your tests
}
