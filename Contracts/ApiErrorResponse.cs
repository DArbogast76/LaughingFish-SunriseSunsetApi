namespace LaughingFish.SunriseSunsetApi.Contracts;

public sealed class ApiErrorResponse
{
    public required string Error { get; init; }
    public required string Message { get; init; }
    public string? InvocationId { get; init; }
}
