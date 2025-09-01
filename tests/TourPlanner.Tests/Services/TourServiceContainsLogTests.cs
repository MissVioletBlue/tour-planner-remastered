using System.Collections.Generic;
using System.Reflection;
using TourPlanner.Application.Services;
using TourPlanner.Domain.Entities;
using TourPlanner.Tests.Utils;
using Xunit;

namespace TourPlanner.Tests.Services;

public class TourServiceContainsLogTests
{
    private static bool InvokeContains(Tour tour, IReadOnlyList<TourLog> logs, int popularity, double? child, string text)
    {
        var m = typeof(TourService).GetMethod("Contains", BindingFlags.NonPublic | BindingFlags.Static);
        return (bool)(m?.Invoke(null, new object?[] { tour, logs, popularity, child, text }) ?? false);
    }

    [Fact]
    public void Contains_Finds_By_Log_Comment()
    {
        var tour = TestHelper.NewTour("T1", 1);
        var logs = new List<TourLog> { TestHelper.NewLog(tour.Id, "wonderful", 3) };
        Assert.True(InvokeContains(tour, logs, 0, null, "wonder"));
    }

    [Fact]
    public void Contains_Finds_By_Log_Rating()
    {
        var tour = TestHelper.NewTour("T2", 1);
        var logs = new List<TourLog> { TestHelper.NewLog(tour.Id, string.Empty, 5) };
        Assert.True(InvokeContains(tour, logs, 0, null, "5"));
    }
}
