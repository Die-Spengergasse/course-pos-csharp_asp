using Fitnesscenter.Application.Commands;
using Fitnesscenter.Application.Services;
using Fitnesscenter.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Fitnesscenter.Test;

public class Lab02GradingTests : IDisposable
{
    private readonly TestWebApplicationFactory _factory;
    private readonly FitnessContext _db;
    private readonly FitnessService _service;
    public Lab02GradingTests()
    {
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 2, 28, 16, 30, 0, TimeSpan.Zero));
        _factory = new TestWebApplicationFactory();
        _db = GetSeededDbContext();
        _service = new FitnessService(_db, true, timeProvider);
    }
    public void Dispose()
    {
        _factory.Dispose();
        _db.Dispose();
    }

    private FitnessContext GetSeededDbContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder()
            .UseSqlite(connection)
            .Options;

        var db = new FitnessContext(options);
        db.Database.EnsureCreated();
        db.Seed();
        db.ChangeTracker.Clear();
        return db;
    }

    private string GetErrorMessage(JsonElement element)
    {
        // Errors produced with return Problem(...).
        if (element.TryGetProperty("detail", out var detail))
            return detail.GetString() ?? string.Empty;
        // Errors produced from validation.
        if (element.TryGetProperty("errors", out var errors))
            return string.Join(
                Environment.NewLine,
                errors.EnumerateObject()
                    .SelectMany(o => o.Value.EnumerateArray().Select(v => v.GetString())));
        return string.Empty;
    }

    /// <summary>
    /// Prüft die Grundfunktionalität des Services.
    /// </summary>
    [Fact]
    public async Task T01_FitnessServiceSmokeTest()
    {
        var member = await _service.CreateMember(new CreateMemberCmd("Fn", "Ln", "x@y.at", "Basic", true));
        Assert.True(member.Id != 0, "The member has not been saved.");
        Assert.True(
            member.ActiveSince == new DateTime(2026, 2, 28, 16, 30, 0, DateTimeKind.Utc),
            $"Wrong value in Member.ActiveSince. Expected: 2026-02-28 16:30, given: {member.ActiveSince:yyyy-MM-dd HH:mm}");
        var member2 = await _service.CreateMember(new CreateMemberCmd("Fn2", "Ln2", "x2@y.at", "Basic", false));
        Assert.True(
            member2.ActiveSince is null,
            $"Wrong value in Member.ActiveSince. Expected: null, given: {member2.ActiveSince:yyyy-MM-dd HH:mm}");

        var participants = await _service.CreateParticipations(new CreateParticipationCmd(4, new List<int> { 6, 9, 15 }));
        Assert.True(participants.Count == 3, "CreateParticipations returned the wrong participations.");
        Assert.True(participants.All(p => p.Id != 0), "The participations have not been saved.");
    }

    /// <summary>
    /// Testet, ob die Controller im Konstruktor ein Interface vom Typ IFitnessService erwarten.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task T02_CreateControllerInstanceTest()
    {
        var membersControllerType = Assembly.Load("Fitnesscenter.Api").GetType("Fitnesscenter.Api.Controllers.MembersController");
        var participationsControllerType = Assembly.Load("Fitnesscenter.Api").GetType("Fitnesscenter.Api.Controllers.ParticipationsController");
        Assert.NotNull(membersControllerType);
        Assert.NotNull(participationsControllerType);

        Assert.True(
            membersControllerType.GetConstructors().SingleOrDefault()?.GetParameters().SingleOrDefault()?.ParameterType.Name == "IFitnessService",
            "MembersController must have one (1) constructor with one (1) parameter of type IFitnessService.");
        Assert.True(
            participationsControllerType.GetConstructors().SingleOrDefault()?.GetParameters().SingleOrDefault()?.ParameterType.Name == "IFitnessService",
            "ParticipationsController must have one (1) constructor with one (1) parameter of type IFitnessService.");
    }

    /// <summary>
    /// Prüft, ob der MembersController die Route POST /api/members bereitstellt und diese funktioniert.
    /// </summary>
    [Fact]
    public async Task T03_CreateMemberReturns201WithIdAndLocationTest()
    {
        var (response, member) = await _factory.PostHttpContent(
            "/api/members", new CreateMemberCmd("Fn", "Ln", "x@y.at", "Basic", true));
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        var memberFromDb = _factory.QueryDatabase(db => db.Members.First(m => m.Email == "x@y.at"));
        Assert.True(
            memberFromDb.Id == member.GetProperty("id").GetInt32(),
            $"Content is not correct. Your result: {member}.");
        var locationHeader = response.Headers.Location?.ToString() ?? string.Empty;
        Assert.True(
            Regex.IsMatch(locationHeader, @"/api/members/\d+", RegexOptions.IgnoreCase),
            $"Wrong location header. Expected end: api/members/(id), you give {locationHeader}");
    }

    /// <summary>
    /// Prüft, ob der ParticipantsController die Route POST /api/participants bereitstellt und diese funktioniert.
    /// </summary>
    [Fact]
    public async Task T04_CreateParticipationsReturns201WithIdAndLocationTest()
    {
        var participationsCountBefore = _factory.QueryDatabase(db => db.Participations.Count());
        var (response, participations) = await _factory.PostHttpContent(
            "/api/participations", new CreateParticipationCmd(4, new List<int> { 6, 9, 15 }));
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        var participationsCountAfter = _factory.QueryDatabase(db => db.Participations.Count());
        Assert.True(participations.GetProperty("ids").EnumerateArray().Count() == 3, $"Wrong data: {participations}");
        Assert.True(
            participationsCountAfter == participationsCountBefore + 3,
            $"Wrong number of participations in database. Actual: {participationsCountAfter}, expected: {participationsCountBefore + 3}.");
        var locationHeader = response.Headers.Location?.ToString() ?? string.Empty;
        Assert.True(
            Regex.IsMatch(locationHeader, @"/api/participations/\d+", RegexOptions.IgnoreCase),
            $"Wrong location header. Expected end: api/participations/(id), you give {locationHeader}");
    }

    /// <summary>
    /// Prüft alle Fehlerzustände, die der MembersController im Endpunkt POST /api/members berücksichtigen soll.
    /// </summary>
    [Theory]
    [InlineData("", "x@y.at", "Basic", "first")]
    [InlineData("Fn", "x@y.at", "xxx", "membership")]
    [InlineData("Fn", "bob@example.com", "Basic", "UNIQUE")]
    public async Task T05_CreateMemberReturns400Tests(
        string firstname, string email, string membershipType,
        string expectedErrorMessage)
    {
        var (response, data) = await _factory.PostHttpContent(
            "/api/members", new CreateMemberCmd(
                firstname, "Ln", email, membershipType, true));
        var errorMessage = GetErrorMessage(data);
        Assert.True(
            errorMessage.Contains(expectedErrorMessage, StringComparison.OrdinalIgnoreCase),
            $"Wrong exception. Expected substring: {expectedErrorMessage}, given: {errorMessage}");
    }

    /// <summary>
    /// Prüft alle Fehlerzustände, die der ParticipantsController im Endpunkt POST /api/participants berücksichtigen soll.
    /// </summary>
    [Theory]
    [InlineData(999, "[6]", "Invalid member id")]
    [InlineData(3, "[6]", "Member is not active")]
    [InlineData(4, "[6,9,999]", "contains at least one invalid id")]
    [InlineData(4, "[6,7,9]", "is already full")]
    public async Task T06_CreateParticipationsReturns400Tests(
        int memberId, string participationIds, string expectedErrorMessage)
    {
        var (response, data) = await _factory.PostHttpContent(
            "/api/participations", new CreateParticipationCmd(
                memberId, JsonSerializer.Deserialize<List<int>>(participationIds)!));
        var errorMessage = GetErrorMessage(data);
        Assert.True(
            errorMessage.Contains(expectedErrorMessage, StringComparison.OrdinalIgnoreCase),
            $"Wrong exception. Expected substring: {expectedErrorMessage}, given: {errorMessage}");
    }
}
