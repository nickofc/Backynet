namespace Backynet.Core.Abstraction;

public readonly struct JobDescriptor : IJobDescriptor
{
    public static readonly IJobDescriptor Empty = JobDescriptorFactory.Create(
        () => JobDescriptorFactory.Empty.Method());

    public IMethod Method { get; }
    public IArgument[] Arguments { get; }

    public JobDescriptor(IMethod method, IArgument[] arguments)
    {
        Method = method;
        Arguments = arguments;
    }
}

public readonly struct Method : IMethod
{
    public string TypeName { get; init; }
    public string Name { get; init; }

    public Method(string typeName, string name)
    {
        TypeName = typeName;
        Name = name;
    }
}

public readonly struct Argument : IArgument
{
    public string? TypeName { get; init; }
    public object? Value { get; init; }

    public Argument(string? typeName, object? value)
    {
        TypeName = typeName;
        Value = value;
    }
}