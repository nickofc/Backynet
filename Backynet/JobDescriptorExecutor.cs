using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Backynet.Abstraction;

namespace Backynet;

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
            var arguments = jobDescriptor.Arguments
                .Select(x => x.Value)
                .ToList(); // todo: reduce gc allocations
            
            var expectedParameters = methodInfo.GetParameters();
            
            if (expectedParameters.Length > 0  && 
                expectedParameters[^1].ParameterType == typeof(CancellationToken))
            {
                arguments.Add(cancellationToken);
            }

            var returnValue = methodInfo.Invoke(null, arguments.ToArray());

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

    private static void ReplaceCancellationToken(object?[] arguments, CancellationToken cancellationToken)
    {
        for (var i = 0; i < arguments.Length; i++)
        {
            if (arguments[i] is CancellationToken)
            {
                arguments[i] = cancellationToken;
                return;
            }
        }
    }
}