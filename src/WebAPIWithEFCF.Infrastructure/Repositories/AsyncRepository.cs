using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebAPIWithEFCF.Application.Interfaces;
using WebAPIWithEFCF.Domain.Common;
using WebAPIWithEFCF.Infrastructure.Data;

namespace WebAPIWithEFCF.Infrastructure.Repositories;

public class AsyncRepository<T> : IAsyncRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _dbContext;

    public AsyncRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<T>().FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<T>().AsNoTracking().ToListAsync(cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<T>().Where(predicate).AsNoTracking().ToListAsync(cancellationToken);
    }

    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<T>().AnyAsync(predicate, cancellationToken);
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        return predicate == null
            ? await _dbContext.Set<T>().CountAsync(cancellationToken)
            : await _dbContext.Set<T>().CountAsync(predicate, cancellationToken);
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<T>().AddAsync(entity, cancellationToken);
        return entity;
    }

    public virtual void Update(T entity)
    {
        _dbContext.Set<T>().Update(entity);
    }

    public virtual void Delete(T entity)
    {
        _dbContext.Set<T>().Remove(entity);
    }
}
