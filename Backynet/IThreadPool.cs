namespace Backynet;

public interface IThreadPool
{
    void Post(Func<Task> func);
}