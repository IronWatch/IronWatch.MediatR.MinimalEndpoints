using IronWatch.MediatR.MinimalEndpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Example.Endpoints;

public class GetWeather : IRequest<IResult>
{
    [FromRoute]
    public required string Location { get; set; }

    [FromQuery]
    public required string State { get; set; }

    [ApiGet("/weather/{Location}")]
    public class Handler : IRequestHandler<GetWeather, IResult>
    {
        public Task<IResult> Handle(GetWeather request, CancellationToken cancellationToken)
        {
            return Task.FromResult(
                Results.Ok(
                    new Response()
                    {
                        Weather = $"It is {request.State} in {request.Location}"
                    }));
        }
    }

    public class Response
    {
        public required string Weather { get; set; }
    }
}
