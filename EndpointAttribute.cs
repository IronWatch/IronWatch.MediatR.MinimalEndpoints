using System;

namespace IronWatch.MediatR.MinimalEndpoints;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EndpointAttribute : Attribute
{
    public string Method { get; set; }
	public string? NameOverride { get; set; }
    public string? RouteOverride { get; set; }
    public BindType BindType { get; set; }

    public EndpointAttribute(string method, string? routeOverride, BindType bindType)
    {
        this.Method = method;
		this.RouteOverride = routeOverride;
		this.BindType = bindType;
    }
}

public class Get : EndpointAttribute
{

    public Get() 
        : base("GET", null, BindType.QUERY)
	{}
}

public class Head : EndpointAttribute
{

	public Head()
		: base("HEAD", null, BindType.QUERY)
	{}
}

public class Post : EndpointAttribute
{

	public Post()
		: base("POST", null, BindType.BODY)
	{}
}

public class Put : EndpointAttribute
{

	public Put()
		: base("PUT", null, BindType.BODY)
	{}
}

public class Delete : EndpointAttribute
{

	public Delete()
		: base("DELETE", null, BindType.BODY)
	{}
}

public class Connect : EndpointAttribute
{

	public Connect()
		: base("CONNECT", null, BindType.QUERY)
	{}
}

public class Options : EndpointAttribute
{

	public Options()
		: base("OPTIONS", null, BindType.QUERY)
	{}
}

public class Trace : EndpointAttribute
{

	public Trace()
		: base("TRACE", null, BindType.QUERY)
	{}
}

public class Patch : EndpointAttribute
{

	public Patch()
		: base("PATCH", null, BindType.BODY)
	{}
}