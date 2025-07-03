namespace FrameHub.Modules.Shared.Application.Interface;

public interface IUnitOfWork
{
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}