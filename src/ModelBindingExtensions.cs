using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace IronWatch.MediatR.MinimalEndpoints;

public static class ModelBindingExtensions
{
	/// <summary>
	/// REQUIRES <see cref="MvcCoreServiceCollectionExtensions.AddMvcCore(IServiceCollection)"/> services to be in the DI container
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="httpContext"></param>
	/// <returns></returns>
	public static async Task<T?> BindFromForm<T>(this HttpContext httpContext)
	{
		IServiceProvider serviceProvider = httpContext.RequestServices;
		IModelBinderFactory factory = serviceProvider.GetRequiredService<IModelBinderFactory>();
		IModelMetadataProvider metadataProvider = serviceProvider.GetRequiredService<IModelMetadataProvider>();

		ModelMetadata metadata = metadataProvider.GetMetadataForType(typeof(T));
		IModelBinder modelBinder = factory.CreateBinder(new()
		{
			Metadata = metadata
		});

		DefaultModelBindingContext context = new DefaultModelBindingContext
		{
			ModelMetadata = metadata,
			ModelName = string.Empty,
			ValueProvider = new FormValueProvider(
				BindingSource.Form,
				httpContext.Request.Form,
				CultureInfo.InvariantCulture
			),
			ActionContext = new ActionContext(
				httpContext,
				new RouteData(),
				new ActionDescriptor()),
			ModelState = new ModelStateDictionary()
		};
		await modelBinder.BindModelAsync(context);
		return (T?)context.Result.Model;
	}
}