using IronWatch.MediatR.MinimalEndpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.Serialization;

namespace Example.Endpoints;

[ApiPost("/bread/{Grain}")]
public class PostBreadForm : IRequestHandler<PostBreadFormRequest, IResult>
{
    public Task<IResult> Handle(PostBreadFormRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Results.Ok(new PostBreadFormResponse()
        {
            BreadSummary = $"{request.Param.Grain} {request.Form.Weight}oz"
        }));
    }
}

[AsForm(nameof(Form), nameof(Param))]
public class PostBreadFormRequest : IRequest<IResult>
{
    public required PostBreadFormRequestParam Param { get; set; }
    public required PostBreadFormRequestForm Form { get; set; }
}

public class PostBreadFormRequestParam
{
    [FromRoute]
    public required string Grain { get; set; }
}

public class PostBreadFormRequestForm
{
    public required int Weight { get; set; }
}

public class PostBreadFormResponse
{
    public required string BreadSummary { get; set; }
}
