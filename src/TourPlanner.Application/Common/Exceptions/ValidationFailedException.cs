namespace TourPlanner.Application.Common.Exceptions;

public class ValidationFailedException : Exception
{
    public ValidationFailedException(string message) : base(message) { }
}
