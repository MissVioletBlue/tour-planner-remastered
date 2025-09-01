using System;
using System.Collections.Generic;
using TourPlanner.Domain.Entities;

namespace TourPlanner.Tests.Utils;

public static class TestHelper
{
    public static Tour NewTour(string name, double distance)
        => new(Guid.NewGuid(), name, null, "A", "B", "car", distance, TimeSpan.Zero,
            new List<(double,double)>(), string.Empty);

    public static TourLog NewLog(Guid tourId, string comment, int rating, int votes = 0)
        => new(Guid.NewGuid(), tourId, DateTime.Today, comment, 1, 0, TimeSpan.Zero, rating, votes);
}
