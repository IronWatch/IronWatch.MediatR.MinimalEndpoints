using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace IronWatch.MediatR.MinimalEndpoints;

public static class EndpointRegistrationExtensions
{
	public static IServiceCollection AddRegisteredEndpointTracker(this IServiceCollection services)
	{
		return services.AddSingleton<RegisteredEndpointsService>();
	}
	
	public static IEndpointRouteBuilder BuildEndpoints(this IEndpointRouteBuilder endpointRouteBuilder, Assembly assembly)
	{
		try
		{
			_ = endpointRouteBuilder.ServiceProvider.GetRequiredService<IMediator>();
		}
		catch (InvalidOperationException)
		{
			throw new InvalidOperationException("Critical error, refusing to build endpoitns as an IMediator was not found in the service provider. The endpoints this method would build will not work!");
		}
		
		IEnumerable<Type> endpointTypes = assembly.GetExportedTypes()
			.Where(x => x.GetCustomAttributes<ApiEndpointAttribute>().Any());

		IEnumerable<Type> responseModelTypes = assembly.GetExportedTypes()
			.Where(x => x.GetCustomAttributes<ResponseModelAttribute>().Any());

		MethodInfo? registerEndpointMethodInfo = typeof(EndpointRegistrationExtensions)
					.GetMethod(nameof(RegisterEndpoint), BindingFlags.NonPublic | BindingFlags.Static);
		if (registerEndpointMethodInfo is null)
		{
			throw new ApplicationException($"Could not instantiate the method {nameof(RegisterEndpoint)}");
		}

		RegisteredEndpointsService? registeredEndpointsService = endpointRouteBuilder.ServiceProvider.GetService<RegisteredEndpointsService>();

		Dictionary<Type, Type> responseModelsForRequests = new();

		foreach (Type responseModelType in responseModelTypes)
		{
			ResponseModelAttribute? responseModelAttribute = responseModelType.GetCustomAttribute<ResponseModelAttribute>();
			if (responseModelAttribute is null)
			{
				throw new ApplicationException($"Response model type does not have a response model attribute but ended up in the responseModelTypes list: {responseModelType.Name}");
			}

			responseModelsForRequests.Add(responseModelAttribute.RequestType, responseModelType);
		}

		foreach (Type endpointType in endpointTypes)
		{
			ApiEndpointAttribute? endpointAttribute = endpointType.GetCustomAttribute<ApiEndpointAttribute>();
			if (endpointAttribute is null)
			{
				throw new ApplicationException($"Endpoint class does not have an endpoint attribute but ended up in the endpointTypes list: {endpointType.Name}");
			}

			Type? requestHandlerInterface = endpointType.GetInterfaces()
				.Where(x => x.IsGenericType)
				.Where(x => x.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
				.FirstOrDefault();
			if (requestHandlerInterface is null)
			{
                throw new ApplicationException($"Endpoint class does not inherit {typeof(IRequestHandler<,>).FullName}: {endpointType.Name}");
            }

			List<Type> genericArguments = requestHandlerInterface.GetGenericArguments().ToList();
			if (genericArguments.Count != 2)
			{
                throw new ApplicationException($"Endpoint class does not pass exactly 2 generic type arguments to IRequestHandler: {endpointType.Name}");
            }

			if (genericArguments[1] != typeof(IResult))
			{
                throw new ApplicationException($"Endpoint class does not specify IResult as the TResponse generic type argument in IRequestHandler: {endpointType.Name}");
            }

			List<Attribute> allAttributesOnEndpointClass = endpointType.GetCustomAttributes().ToList();

			RegisteredEndpoint registeredEndpoint = new()
			{
				Method = endpointAttribute.Method,
				Route = endpointAttribute.Route,
				HandlerType = endpointType,
				HandlerAttributes = allAttributesOnEndpointClass,
				RequestType = genericArguments[0]
			};

			_ = registerEndpointMethodInfo.MakeGenericMethod(registeredEndpoint.RequestType)
				.Invoke(null,
                [
                    endpointRouteBuilder,
					registeredEndpoint,
					responseModelsForRequests.GetValueOrDefault(endpointType)
				]);

			if (registeredEndpointsService is not null)
			{
				registeredEndpointsService.Add(registeredEndpoint);
			}
		}

		return endpointRouteBuilder;
	}

	private static string RelativePathCombine(params string[] paths)
	{
		List<string> parts = new();
		foreach (string path in paths)
		{
			parts.AddRange(path
			.Replace('\\', '/')
			.Trim('/')
			.ToLower()
			.Split('/'));
		}

		string result = String.Empty;

		foreach (string part in parts)
		{
			if (string.IsNullOrWhiteSpace(part))
			{
				continue;
			}

			result += $"/{HttpUtility.UrlEncode(part)}";
		}

		return result;
	}

	private static RouteHandlerBuilder RegisterEndpoint<TRequest>(
		IEndpointRouteBuilder endpointRouteBuilder,
		RegisteredEndpoint registeredEndpoint,
		Type? responseModelType) where TRequest : IRequest<IResult>
	{
		RouteHandlerBuilder? routeHandlerBuilder = null;

        routeHandlerBuilder = endpointRouteBuilder.MapMethods(
            registeredEndpoint.Route,
            new List<string> { registeredEndpoint.Method },
            async (
                CancellationToken cancellationToken,
                IMediator mediator,
                [AsParameters] TRequest request
            ) =>
            {
                return await mediator.Send(request, cancellationToken);
            });
		
		List<Attribute> metadataAttributes = registeredEndpoint.HandlerAttributes
			.Where(x => typeof(IRouteMetadataAttribute).IsAssignableFrom(x.GetType()))
			.ToList();
		routeHandlerBuilder.WithMetadata(metadataAttributes);

		// Open API pieces
		routeHandlerBuilder = routeHandlerBuilder
			.Produces(200, responseModelType);

		return routeHandlerBuilder;
	}
}
