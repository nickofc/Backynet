namespace Backynet.Abstraction;

public class JobDescriptorExecutorException : Exception
{
    public JobDescriptorExecutorException()
    {
    }

    public JobDescriptorExecutorException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}