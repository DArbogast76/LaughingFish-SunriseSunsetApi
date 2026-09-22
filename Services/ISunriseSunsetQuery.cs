using LaughingFish.SunriseSunsetApi.Contracts;

namespace LaughingFish.SunriseSunsetApi.Services;

public interface ISunriseSunsetQuery
{
    SunriseSunsetResponse Execute(string invocationId, SunriseSunsetRequest request);
}
