using System.Runtime.Serialization;

namespace Backynet.Core.Abstraction;

public class JobDescriptorExecutorException : Exception
{
    public JobDescriptorExecutorException()
    {
    }

    protected JobDescriptorExecutorException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public JobDescriptorExecutorException(string? message) : base(message)
    {
    }

    public JobDescriptorExecutorException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}