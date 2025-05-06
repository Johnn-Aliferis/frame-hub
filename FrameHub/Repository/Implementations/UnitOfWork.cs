using FrameHub.ContextConfiguration;
using FrameHub.Repository.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace FrameHub.Repository.Implementations;

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