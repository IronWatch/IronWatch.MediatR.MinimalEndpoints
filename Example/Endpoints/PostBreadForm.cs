using IronWatch.MediatR.MinimalEndpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Example.Endpoints;

[ApiPost("/bread")]
public class PostBreadForm : IRequestHandler<PostBreadFormRequest, IResult>
{
    public Task<IResult> Handle(PostBreadFormRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Results.Ok(new PostBreadFormResponse()
        {
            BreadSummary = $"{request.Grain} {request.Weight}oz"
        }));
    }
}

[AsForm]
public class PostBreadFormRequest : IRequest<IResult>
{
    public required string Grain { get; set; }
    public required int Weight { get; set; }
}

public class PostBreadFormResponse
{
    public required string BreadSummary { get; set; }
}
