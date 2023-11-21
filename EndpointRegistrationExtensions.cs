using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;

namespace IronWatch.MediatR.MinimalEndpoints;

public static class EndpointRegistrationExtensions
{
	public static IServiceCollection AddRegisteredEndpointTracker(this IServiceCollection services)
	{
		return services.AddSingleton<RegisteredEndpoints>();
	}
	
	public static IEndpointRouteBuilder BuildEndpoints(this IEndpointRouteBuilder endpointRouteBuilder, Type rootNamespacePlaceholder, string basePath = "")
	{
		string rootNamespace = rootNamespacePlaceholder.Namespace 
			?? throw new ApplicationException("Root Namespace Placeholder class does not resolve a namespace!");
		
		IEnumerable<Type> endpointTypes = rootNamespacePlaceholder.Assembly.GetExportedTypes()
			.Where(x => x.GetCustomAttributes<EndpointAttribute>().Any());

		IEnumerable<Type> responseModelTypes = rootNamespacePlaceholder.Assembly.GetExportedTypes()
			.Where(x => x.GetCustomAttributes<ResponseModelAttribute>().Any());

		MethodInfo? registerEndpointMethodInfo = typeof(EndpointRegistrationExtensions)
					.GetMethod(nameof(RegisterEndpoint), BindingFlags.NonPublic | BindingFlags.Static);
		if (registerEndpointMethodInfo is null)
		{
			throw new ApplicationException($"Could not instantiate the method {nameof(RegisterEndpoint)}");
		}

		RegisteredEndpoints? registeredEndpoints = endpointRouteBuilder.ServiceProvider.GetService<RegisteredEndpoints>();

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
			EndpointAttribute? endpointAttribute = endpointType.GetCustomAttribute<EndpointAttribute>();
			if (endpointAttribute is null)
			{
				throw new ApplicationException($"Endpoint class does not have an endpoint attribute but ended up in the endpointTypes list: {endpointType.Name}");
			}

			List<Attribute> allAttributesInType = endpointType.GetCustomAttributes().ToList();

			string route = "";

			if (endpointAttribute.RouteOverride == null)
			{
				string endpointNamespace = endpointType.Namespace
					?? throw new ApplicationException($"Endpoint class does not resolve a namespace: {endpointType.Name}");

				if (!endpointNamespace.StartsWith(rootNamespace))
				{
					throw new ApplicationException($"Endpoint class is located outside the root namespace: {endpointType.Name}");
				}

				endpointNamespace = endpointNamespace
					.Substring(rootNamespace.Length)
					.Replace('.', '/')
					.ToLower();

				string name = "";
				if (string.IsNullOrWhiteSpace(endpointAttribute.NameOverride))
				{
					name = endpointType.Name;
					if (name.Contains('`'))
					{
						name = name[..name.IndexOf('`')];
					}
				}
				else
				{
					name = endpointAttribute.NameOverride;
				}

				route = RelativePathCombine(basePath, endpointNamespace, name);
			}
			else
			{
				route = RelativePathCombine(basePath, endpointAttribute.RouteOverride);
			}

			RegisteredEndpoint registeredEndpoint = new()
			{
				Method = endpointAttribute.Method,
				Route = route,
				RequestType = endpointType,
				BindType = endpointAttribute.BindType,
				Attributes = allAttributesInType
			};

			_ = registerEndpointMethodInfo.MakeGenericMethod(registeredEndpoint.RequestType)
				.Invoke(null, new object?[]
				{
					endpointRouteBuilder,
					registeredEndpoint,
					responseModelsForRequests.GetValueOrDefault(endpointType)
				});

			if (registeredEndpoints is not null)
			{
				registeredEndpoints.Add(registeredEndpoint);
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

		switch (registeredEndpoint.BindType)
		{
			case BindType.QUERY:
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
				break;
			case BindType.BODY:
				routeHandlerBuilder = endpointRouteBuilder.MapMethods(
					registeredEndpoint.Route,
					new List<string> { registeredEndpoint.Method },
					async (
						CancellationToken cancellationToken,
						IMediator mediator,
						[FromBody] TRequest request
					) =>
					{
						return await mediator.Send(request, cancellationToken);
					});
				break;
			case BindType.FORM:
				routeHandlerBuilder = endpointRouteBuilder.MapMethods(
					registeredEndpoint.Route,
					new List<string> { registeredEndpoint.Method },
					async (
						CancellationToken cancellationToken,
						IMediator mediator,
						[FromForm] TRequest request
					) =>
					{
						return await mediator.Send(request, cancellationToken);
					});
				break;
			case BindType.CUSTOM:
				routeHandlerBuilder = endpointRouteBuilder.MapMethods(
					registeredEndpoint.Route,
					new List<string> { registeredEndpoint.Method },
					async (
						CancellationToken cancellationToken,
						IMediator mediator,
						TRequest request
					) =>
					{
						return await mediator.Send(request, cancellationToken);
					});
				break;
			default:
				throw new ApplicationException("Invalid Bind Type!");
		}

		//Attribute? twilioValidationAttribute = registeredEndpoint.Attributes
		//	.Where(x => x is TwilioValidationAttribute)
		//	.FirstOrDefault();
		//if (twilioValidationAttribute is not null)
		//{
		//	routeHandlerBuilder = routeHandlerBuilder
		//		.WithMetadata(twilioValidationAttribute);
		//}

		List<Attribute> metadataAttributes = registeredEndpoint.Attributes
			.Where(x => typeof(IRouteMetadataAttribute).IsAssignableFrom(x.GetType()))
			.ToList();
		routeHandlerBuilder.WithMetadata(metadataAttributes);

		// Open API pieces
		routeHandlerBuilder = routeHandlerBuilder
			.Produces(200, responseModelType);

		return routeHandlerBuilder;
	}
}
