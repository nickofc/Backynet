namespace Backynet.Abstraction;

public interface IJobDescriptor
{
    IMethod Method { get; }
    IArgument[] Arguments { get; }
}

public interface IMethod
{
    string TypeName { get; }
    string Name { get; }
}

public interface IArgument
{
    string TypeName { get; }
    object? Value { get; }
}