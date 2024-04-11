using System.Reflection;
using Backynet.Core.Abstraction;

namespace Backynet.Core;

internal sealed class JobDescriptorExecutor : IJobDescriptorExecutor
{
    public async Task Execute(IJobDescriptor jobDescriptor, CancellationToken cancellationToken = default)
    {
        var type = Type.GetType(jobDescriptor.Method.TypeName);

        if (type == null)
        {
            throw new InvalidOperationException("Type was not found.");
        }

        var methodInfo = type.GetMethod(jobDescriptor.Method.Name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        if (methodInfo == null)
        {
            throw new InvalidOperationException("Method was not found.");
        }

        try
        {
            var returnValue = methodInfo.Invoke(null, jobDescriptor.Arguments.Select(x => x.Value).ToArray());

            if (returnValue is Task task)
            {
                await task;
            }
        }
        catch (Exception e)
        {
            throw new JobDescriptorExecutorException("Error occured during job code execution.", e);
        }
    }
}