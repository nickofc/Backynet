namespace Backynet.Abstraction;

public interface IGroupRepository
{
    Task Add(Group group, CancellationToken cancellationToken = default);
    Task<Group?> Get(string groupName, CancellationToken cancellationToken = default);
    Task Update(string groupName, Group group, CancellationToken cancellationToken = default);
}