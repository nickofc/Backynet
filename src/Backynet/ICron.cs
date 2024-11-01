namespace Backynet;

public interface ICron
{
    DateTimeOffset GetNextOccurrence(string value, DateTimeOffset now);
}