using IronWatch.MediatR.MinimalEndpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Example.Endpoints;

[ApiPost("/grain/{GrainType?}")]
public class PostGrain : IRequestHandler<PostGrainRequest, IResult>
{
    public Task<IResult> Handle(PostGrainRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Results.Ok(new PostGrainResponse() { 
            GrainType = request.GrainType,
            DesiredQuantity = request.Body.DesiredQuantity
        }));
    }
}

public class PostGrainRequest : IRequest<IResult>
{
    [FromRoute]
    public string? GrainType { get; set; }

    [FromBody]
    public required PostGrainRequestBody Body { get; set; }
}

public class PostGrainRequestBody
{
    public required int DesiredQuantity { get; set; }
}

public class PostGrainResponse
{
    public string? GrainType { get; set; }
    public required int DesiredQuantity { get; set; }
}