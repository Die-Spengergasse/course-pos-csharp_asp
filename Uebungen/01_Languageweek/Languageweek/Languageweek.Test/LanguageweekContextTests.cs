// =================================================================================================
// Vordefinierte Tests für die Abfragen in LanguageweekContext
// Sie werden zur Korrektur verwendet, hier darf nichts verändert werden.
// =================================================================================================
using Languageweek.Application.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Languageweek.Test;

public class LanguageweekContextTests : IDisposable
{
    private readonly SqliteConnection _conn;
    private readonly LanguageweekContext _db;
    public LanguageweekContextTests()
    {
        _conn = new SqliteConnection("DataSource=:memory:");

        _db = new LanguageweekContext(new DbContextOptionsBuilder().UseSqlite(_conn).Options);
        _db.Database.EnsureDeleted();
        _db.Database.OpenConnection();
        _db.Database.EnsureCreated();
        _db.Seed();
    }

    public void Dispose()
    {
        _db.Dispose();
        _conn.Dispose();
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
                () => _db.GetTeachersWithMinCountOfParticipations(count),
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
            _db.GetClassesWithoutLanguageWeek,
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
            _db.CalcSchoolclassStatistics,
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
            _db.CalcRegistrationRates,
            "CalcRegistrationRates");
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

}
