using System.Reflection;
using Backynet.Abstraction;
using Microsoft.Extensions.DependencyInjection;

namespace Backynet;

internal sealed class ScopedJobDescriptorExecutor : IJobDescriptorExecutor
{
    private readonly IServiceProvider _serviceProvider;

    public ScopedJobDescriptorExecutor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Execute(IJobDescriptor jobDescriptor, CancellationToken cancellationToken = default)
    {
        var type = Type.GetType(jobDescriptor.Method.TypeName);

        if (type == null)
        {
            throw new InvalidOperationException("Type was not found.");
        }

        var methodInfo = type.GetMethod(jobDescriptor.Method.Name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (methodInfo == null)
        {
            throw new InvalidOperationException("Method was not found.");
        }

        object? instance = null;
        IServiceScope? serviceScope = null;

        try
        {
            if (methodInfo.IsStatic == false)
            {
                serviceScope = _serviceProvider.CreateScope();
                instance = serviceScope.ServiceProvider.GetRequiredService(type);
            }

            try
            {
                var arguments = jobDescriptor.Arguments
                    .Select(x => x.Value)
                    .ToList();

                var expectedParameters = methodInfo.GetParameters();

                if (expectedParameters.Length > 0 &&
                    expectedParameters[^1].ParameterType == typeof(CancellationToken))
                {
                    arguments.Add(cancellationToken);
                }

                var returnValue = methodInfo.Invoke(instance, arguments.ToArray());

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
        finally
        {
            serviceScope?.Dispose();
        }
    }
}