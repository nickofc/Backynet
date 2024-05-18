namespace Backynet.Abstraction;

public readonly struct JobDescriptor : IJobDescriptor
{
    public static IJobDescriptor Empty() => JobDescriptorFactory.Create(
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
    public string TypeName { get; }
    public string Name { get; }

    public Method(string typeName, string name)
    {
        TypeName = typeName;
        Name = name;
    }
}

public readonly struct Argument : IArgument
{
    public string TypeName { get; }
    public object? Value { get; }

    public Argument(string typeName, object? value)
    {
        TypeName = typeName;
        Value = value;
    }
}