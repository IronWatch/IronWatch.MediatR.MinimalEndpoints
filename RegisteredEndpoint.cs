using System;
using System.Collections.Generic;

namespace IronWatch.MediatR.MinimalEndpoints;

public class RegisteredEndpoint
{
    public required string Route { get; set; }
    public required string Method { get; set; }
    public required Type RequestType { get; set; }
    public required BindType BindType { get; set; }
    public required List<Attribute> Attributes { get; set; }
}
