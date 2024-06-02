namespace Backynet.Abstraction;

public enum JobState
{
    Unknown,
    Created,
    Enqueued,
    Scheduled,
    Processing,
    Failed,
    Succeeded,
    Canceled,
    Deleted
}

// Created - job który trzeba zaplanowac (tzn. obliczyć crona)
// Enqueued - czeka na wykonanie 
// Scheduled - zaplanowany w przyszlosci 
// Processing - przetwarzany
// Failed - bład 
// Succeed - wykonany poprawnie