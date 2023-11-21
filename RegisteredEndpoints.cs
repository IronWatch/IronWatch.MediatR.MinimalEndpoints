using System;
using System.Collections.Generic;
using System.Linq;

namespace IronWatch.MediatR.MinimalEndpoints;

public class RegisteredEndpoints
{
	private readonly List<RegisteredEndpoint> registeredEndpoints = new();

	public void Add(RegisteredEndpoint registeredEndpoint)
	{
		registeredEndpoints.Add(registeredEndpoint);
	}

	public RegisteredEndpoint? Get(Type type)
	{
		return registeredEndpoints
			.Where(x => x.RequestType == type)
			.FirstOrDefault();
	}

	public RegisteredEndpoint? Get(string route)
	{
		return registeredEndpoints
			.Where(x => x.Route.Equals(route, StringComparison.InvariantCultureIgnoreCase))
			.FirstOrDefault();
	}
}
