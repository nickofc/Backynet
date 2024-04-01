namespace Backynet.Core.Abstraction;

public interface IJobDescriptor
{
    IMethod Method { get; }
    IReadOnlyCollection<IArgument> Arguments { get; }
}

public interface IMethod
{
    string TypeName { get; }
    string Name { get; }
}

public interface IArgument
{
    string TypeName { get; }
    object Instance { get; }
}