using System.Reflection;
using Backynet.Core.Abstraction;

namespace Backynet.Core;

internal sealed class SimpleJobRunner : IJobRunner
{
    // TODO: ioc
    // TODO: cache?

    public async Task Run(Job job)
    {
        var member = Type.GetType(job.Descriptor.Method.TypeName);
        var methodInfo = member.GetMethod(job.Descriptor.Method.Name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        var invokeReturnValue = methodInfo.Invoke(null, job.Descriptor.Arguments);

        if (invokeReturnValue is Task t)
        {
            await t;
        }
    }
}