using FrameHub.Modules.Shared.Application.Interface;
using FrameHub.Modules.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace FrameHub.Modules.Shared.Infrastructure.Repository;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    public async Task BeginTransactionAsync()
    {
        _transaction = await context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        if (_transaction != null)
        {
            await context.SaveChangesAsync();
            await _transaction.CommitAsync();
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
        }
    }
}