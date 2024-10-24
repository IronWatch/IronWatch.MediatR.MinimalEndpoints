using MediatR;
using IronWatch.MediatR.MinimalEndpoints;
using System.Reflection;
using Example.Endpoints;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddMediatR(mediatr =>
{
    mediatr.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});

builder.Services.AddRegisteredEndpointTracker();

var app = builder.Build();

app.BuildEndpoints(Assembly.GetExecutingAssembly());

app.Run();
