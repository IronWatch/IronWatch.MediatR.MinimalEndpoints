using MediatR;
using IronWatch.MediatR.MinimalEndpoints;
using System.Reflection;
using Example.Endpoints;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddMediatR(mediatr =>
{
    mediatr.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});

builder.Services.AddRegisteredEndpointTracker();

var app = builder.Build();

app.BuildEndpoints(Assembly.GetExecutingAssembly());

app.UseSwagger();
app.UseSwaggerUI();

app.Run();