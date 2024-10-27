using IronWatch.MediatR.MinimalEndpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Example.Endpoints;

[ApiGet("/weather/{Location}")]
public class GetWeather : IRequestHandler<GetWeatherRequest, IResult>
{
    public Task<IResult> Handle(GetWeatherRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(
            Results.Ok(
                new GetWeatherResponse()
                {
                    Weather = $"It is {request.State} in {request.Location}"
                }));
    }
}

public class GetWeatherRequest : IRequest<IResult>
{
    [FromRoute]
    public required string Location { get; set; }

    [FromQuery]
    public required string State { get; set; }

}

public class GetWeatherResponse
{
    public required string Weather { get; set; }
}
