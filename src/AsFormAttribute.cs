using System;

namespace IronWatch.MediatR.MinimalEndpoints;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class AsFormAttribute : Attribute
{
    public bool EnableAntiForgery { get; set; }

    public AsFormAttribute(bool enableAntiForgery = false)
    {
        EnableAntiForgery = enableAntiForgery;
    }
}
