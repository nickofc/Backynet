namespace Backynet.Core.Abstraction;


public readonly struct JobDescriptor : IJobDescriptor
{
    public IMethod Method { get; }
    public IReadOnlyCollection<IArgument> Arguments { get; }

    public JobDescriptor(IMethod method, IReadOnlyCollection<IArgument> arguments)
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
    public string TypeName { get; init; }
    public object Instance { get; init; }

    public Argument(string typeName, object instance)
    {
        TypeName = typeName;
        Instance = instance;
    }
}