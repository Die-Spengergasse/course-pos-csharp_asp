// =================================================================================================
// Vordefinierte Tests für das LanguageweekService.
// Sie werden zur Korrektur verwendet, hier darf nichts verändert werden.
// =================================================================================================
using Languageweek.Application.Commands;
using Languageweek.Application.Infrastructure;
using Languageweek.Application.Model;
using Languageweek.Application.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;

namespace Languageweek.Test;

public class LanguageweekServiceTests : IDisposable
{
    private readonly SqliteConnection _conn;
    private readonly LanguageweekContext _db;
    private readonly FakeTimeProvider _timeProvider;
    private readonly LanguageweekService _service;

    public LanguageweekServiceTests()
    {
        //_conn = new SqliteConnection("DataSource=../../../languageweeks.db");
        _conn = new SqliteConnection("DataSource=:memory:");

        _db = new LanguageweekContext(new DbContextOptionsBuilder().UseSqlite(_conn).Options);
        _db.Database.EnsureDeleted();
        _db.Database.OpenConnection();
        _db.Database.EnsureCreated();
        _db.Seed();
        // Wir fixieren die Zeit auf den 1. Jänner 2026 um 12 Uhr.
        _timeProvider = new FakeTimeProvider(new DateTimeOffset(new DateTime(2026, 1, 1, 12, 0, 0)));
        _service = new LanguageweekService(db: _db, timeProvider: _timeProvider);
    }

    [Fact]
    public void T01_GetTeachersWithMinCountOfParticipationsTest()
    {
        foreach (var count in new int[] { 1, 2, 3 })
        {
            var sql = $@"select * from teacher t 
                         where (select count(*) from languageWeek l where l.teacherId = t.id or l.supportTeacherId = t.id) >= {count}";
            CheckServiceMethodResult(
                _db.Teachers,
                sql,
                () => _service.GetTeachersWithMinCountOfParticipations(count),
                $"GetTeachersWithMinCountOfParticipations({count})");
        }
    }

    [Fact]
    public void T02_GetClassesWithoutLanguageWeekTest()
    {
        var sql = @"select * from schoolclass s
                    where not exists (select * from languageWeek l where l.schoolclassId = s.id)";
        CheckServiceMethodResult(
            _db.Schoolclasses,
            sql,
            _service.GetClassesWithoutLanguageWeek,
            "GetClassesWithoutLanguageWeek");
    }

    [Fact]
    public void T03_CalcSchoolclassStatisticsTest()
    {
        var sql = @"select sc.id as Id, sc.shortname as Shortname, 
                    (select count(*) from student s where s.schoolclassId = sc.id and s.gender = 'Male') as MaleCount,
                    (select count(*) from student s where s.schoolclassId = sc.id and s.gender = 'Female') as FemaleCount
                    from schoolclass sc";
        CheckServiceMethodResult(
            _db,
            sql,
            _service.CalcSchoolclassStatistics,
            "CalcSchoolclassStatistics");
    }

    [Fact]
    public void T04_CalcRegistrationRatesTest()
    {
        var sql = @"
            select l.id as Id, 
                   l.""from"" as ""From"", 
                   l.""to"" as ""To"",
                   (select count(*) from registration r where r.languageWeekId = l.id) * l.pricePerPerson as TotalPrice,
                   l.destinationId as DestinationId, 
                   d.city as DestinationCity, 
                   d.country as DestinationCountry,
                   l.schoolclassId as SchoolclassId, 
                   sc.shortname as SchoolclassShortname,
                   round(CAST((select count(*) from registration r where r.languageWeekId = l.id) AS REAL) / (select count(*) from student s where s.schoolclassId = l.schoolclassId), 1) as Percentage
            from languageWeek l 
            inner join destination d on (l.destinationId = d.id) 
            inner join schoolclass sc on (l.schoolclassId = sc.id)";

        CheckServiceMethodResult(
            _db,
            sql,
            _service.CalcRegistrationRates,
            "CalcRegistrationRates");
    }

    // =================================================================================================
    // CreateLanguageWeekAsync Tests
    // =================================================================================================

    // 6AAIF (Klasse 5, rein männlich) fährt mit Lehrer 1 (m) und 5 (m). Erfolgsfall.
    [Fact]
    public Task T05_CreateLanguageWeek_Success() =>
        ServiceMethodChangeDb<LanguageWeek>(
            async () => await _service.CreateLanguageWeekAsync(
                new CreateLanguageWeekCommand(5, 1, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 10), 1, 1200, 5)),
            lw => lw.Schoolclass.Id == 5 && lw.Destination.Id == 1 && lw.Teacher.Id == 1 && lw.SupportTeacher!.Id == 5 && lw.PricePerPerson == 1200);

    [Theory]
    [InlineData(999, 1, 1, null, "Schoolclass not found")]
    [InlineData(5, 999, 1, null, "Destination not found")] // Klasse 5 hat noch keine Sprachwoche
    [InlineData(5, 1, 999, null, "Teacher not found")]
    [InlineData(1, 1, 1, null, "The class already has a language week planned")] // Klasse 1 hat LW 1
    [InlineData(5, 1, 2, 3, "The class has male students, but only female teachers are assigned")] // Klasse 5 (nur Burschen), Lehrer 2 & 3 (weiblich)
    public Task T06_CreateLanguageWeek_Validation_Throws(int schoolclassId, int destinationId, int teacherId, int? supportTeacherId, string expectedMessage)
    {
        return ServiceMethodThrows<LanguageweekServiceException>(
            async () => await _service.CreateLanguageWeekAsync(
                new CreateLanguageWeekCommand(schoolclassId, destinationId, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 10), teacherId, 1200, supportTeacherId)),
            expectedMessage);
    }

    // =================================================================================================
    // UpdateLanguageWeekAsync Tests
    // =================================================================================================

    // Languageweek 2 (Klasse 1, Mixed) updaten. Lehrer 1 (m) und 2 (w) beibehalten, damit kein Gender-Mismatch entsteht.
    [Fact]
    public Task T12_UpdateLanguageWeek_Success() =>
        ServiceMethodChangeDb<LanguageWeek>(
            async () => await _service.UpdateLanguageWeekAsync(
                new UpdateLanguageWeekCommand(2, 3, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 10), 1, 1500, 2)),
            lw => lw.Id == 2 && lw.Destination.Id == 3 && lw.PricePerPerson == 1500 && lw.To == new DateOnly(2026, 10, 10));

    // Update von Sprachwoche 1 (hat SupportTeacher Id 2). Wir übergeben null, also muss Id 2 erhalten bleiben.
    [Fact]
    public Task T12b_UpdateLanguageWeek_SupportTeacherNull_KeepsOldSupportTeacher() =>
        ServiceMethodChangeDb<LanguageWeek>(
            async () => await _service.UpdateLanguageWeekAsync(
                new UpdateLanguageWeekCommand(1, 5, new DateOnly(2026, 2, 5), new DateOnly(2026, 2, 10), 1, 950, null)),
            lw => lw.Id == 1 && lw.PricePerPerson == 950 && lw.SupportTeacher!.Id == 2);

    // Wenn ein neuer SupportTeacher übergeben wird, soll der bestehende überschrieben werden.
    // Wir nehmen Lehrerin 6 (w), damit es bei Klasse 1 (gemischt) mit Lehrer 1 (m) keine Gender-Probleme gibt.
    [Fact]
    public Task T12c_UpdateLanguageWeek_SupportTeacherNotNull_OverwritesOldSupportTeacher() =>
        ServiceMethodChangeDb<LanguageWeek>(
            async () => await _service.UpdateLanguageWeekAsync(
                new UpdateLanguageWeekCommand(2, 5, new DateOnly(2026, 2, 5), new DateOnly(2026, 2, 10), 1, 950, 6)),
            lw => lw.Id == 2 && lw.PricePerPerson == 950 && lw.SupportTeacher!.Id == 6);

    [Theory]
    [InlineData(999, 1, 1, null, "Language week not found")]
    [InlineData(1, 999, 1, null, "Destination not found")]
    [InlineData(1, 1, 999, null, "Teacher not found")]
    [InlineData(3, 1, 2, 3, "The class has male students, but only female teachers are assigned")] // LW 3 (Klasse 3, nur Burschen) mit Lehrer 2 & 3 (weiblich)
    [InlineData(1, 1, 1, 5, "The class has female students, but only male teachers are assigned")] // LW 1 (Klasse 1, gemischt) mit Lehrer 1 & 5 (männlich)
    public Task T13_UpdateLanguageWeek_Validation_Throws(int id, int destinationId, int teacherId, int? supportTeacherId, string expectedMessage)
    {
        return ServiceMethodThrows<LanguageweekServiceException>(
            async () => await _service.UpdateLanguageWeekAsync(
                new UpdateLanguageWeekCommand(id, destinationId, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 10), teacherId, 1500, supportTeacherId)),
            expectedMessage);
    }

    // =================================================================================================
    // UpdateLanguageWeekPriceAsync Tests
    // =================================================================================================

    [Fact]
    public Task T17_UpdateLanguageWeekPrice_Success() =>
        ServiceMethodChangeDb<LanguageWeek>(
            async () => await _service.UpdateLanguageWeekPriceAsync(
                new UpdateLanguageWeekPriceCommand(1, 1111)),
            lw => lw.Id == 1 && lw.PricePerPerson == 1111m);

    [Fact]
    public Task T18_UpdateLanguageWeekPrice_NotFound_Throws() =>
        ServiceMethodThrows<LanguageweekServiceNotFoundException>(
            async () => await _service.UpdateLanguageWeekPriceAsync(
                new UpdateLanguageWeekPriceCommand(999, 1111)),
            "Language week not found");

    // =================================================================================================
    // DeleteLanguageWeekAsync Tests
    // =================================================================================================

    [Fact]
    public Task T19_DeleteLanguageWeek_NotFound_Throws() =>
        ServiceMethodThrows<LanguageweekServiceNotFoundException>(
            async () => await _service.DeleteLanguageWeekAsync(999),
            "Language week not found");

    // Languageweek 2 liegt im Jahr 2025. Das Fake-Datum ist der 01.01.2026.
    [Fact]
    public Task T20_DeleteLanguageWeek_PastDate_Throws() =>
        ServiceMethodThrows<LanguageweekServiceException>(
            async () => await _service.DeleteLanguageWeekAsync(2),
            "Past language weeks cannot be deleted");

    // Languageweek 4 ist in der Zukunft und hat exakt 0 Anmeldungen im Seed.
    [Fact]
    public async Task T21_DeleteLanguageWeek_Success()
    {
        await _service.DeleteLanguageWeekAsync(4);

        _db.ChangeTracker.Clear();
        Assert.False(_db.Languageweeks.Any(l => l.Id == 4));
    }

    // Languageweek 1 ist in der Zukunft und hat viele Anmeldungen. Ohne ForceDelete muss das fehlschlagen.
    [Fact]
    public Task T22_DeleteLanguageWeek_WithRegistrations_Throws() =>
        ServiceMethodThrows<LanguageweekServiceException>(
            async () => await _service.DeleteLanguageWeekAsync(1, forceDelete: false),
            "The language week cannot be deleted because students are already registered");

    // Languageweek 1 mit Anmeldungen MIT ForceDelete löschen. Auch die Registrations müssen weg sein.
    [Fact]
    public async Task T23_DeleteLanguageWeek_ForceDelete_Success()
    {
        await _service.DeleteLanguageWeekAsync(1, forceDelete: true);

        _db.ChangeTracker.Clear();
        Assert.False(_db.Languageweeks.Any(l => l.Id == 1));
        Assert.False(_db.Registrations.Any(r => r.Languageweek.Id == 1));
    }

    private async Task ServiceMethodThrows<TException>(Func<Task> action, string message) where TException : Exception
    {
        var ex = await Assert.ThrowsAnyAsync<TException>(action);
        Assert.Contains(message, ex.Message, StringComparison.OrdinalIgnoreCase);
    }
    private async Task ServiceMethodChangeDb<T>(Func<Task> action, Expression<Func<T, bool>> predicate) where T : class
    {
        await action();
        _db.ChangeTracker.Clear();
        Assert.True(_db.Set<T>().Any(predicate));
    }
    private void CheckServiceMethodResult<T>(DbSet<T> set, string sql, Func<List<T>> serviceMethod, string methodName) where T : class =>
     CheckServiceMethodResult(set.FromSqlRaw(sql).ToList(), sql, serviceMethod, methodName);
    private void CheckServiceMethodResult<T>(LanguageweekContext db, string sql, Func<List<T>> serviceMethod, string methodName) where T : class =>
        CheckServiceMethodResult(db.Database.SqlQueryRaw<T>(sql).ToList(), sql, serviceMethod, methodName);
    private void CheckServiceMethodResult<T>(List<T> rows, string sql, Func<List<T>> serviceMethod, string methodName) where T : class
    {
        var serviceResult = serviceMethod();
        Assert.True(serviceResult.Count == rows.Count, $"{methodName} row count failed: expected {rows.Count}, got {serviceResult.Count}.");
        foreach (var row in rows)
            Assert.True(serviceResult.Any(m => JsonSerializer.Serialize(m) == JsonSerializer.Serialize(row)),
                $"Test failed. Row not found in your result: {JsonSerializer.Serialize(row)}.");
    }
    public void Dispose()
    {
        _db.Dispose();
        _conn.Dispose();
    }
}
