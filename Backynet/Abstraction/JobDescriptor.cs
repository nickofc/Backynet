namespace Backynet.Abstraction;

public readonly struct JobDescriptor
{
    public static JobDescriptor Empty() => JobDescriptorFactory.Create(() => JobDescriptorFactory.Empty.Method());
    public Method Method { get; }
    public Argument[] Arguments { get; }

    public JobDescriptor(Method method, Argument[] arguments)
    {
        Method = method;
        Arguments = arguments;
    }
}

public readonly struct Method
{
    public string TypeName { get; }
    public string Name { get; }

    public Method(string typeName, string name)
    {
        TypeName = typeName;
        Name = name;
    }
}

public readonly struct Argument
{
    public string TypeName { get; }
    public object? Value { get; }

    public Argument(string typeName, object? value)
    {
        TypeName = typeName;
        Value = value;
    }
}