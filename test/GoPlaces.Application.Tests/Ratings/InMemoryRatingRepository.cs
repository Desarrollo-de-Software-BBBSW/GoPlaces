using GoPlaces.Ratings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;

namespace GoPlaces.Tests.Ratings;

/// <summary>
/// Implementación MUY sencilla de IRepository para tests.
/// Guarda los ratings en una lista en memoria.
/// </summary>
public class InMemoryRatingRepository : IRepository<Rating, Guid>
{
    private readonly List<Rating> _items = new();

    // ---------- MÉTODOS QUE REALMENTE USAMOS EN RatingAppService ----------

    // CORRECCIÓN AQUÍ: Usamos .AsAsyncQueryable() en lugar de .AsQueryable()
    // Esto permite que .AnyAsync() funcione sin errores.
    public Task<IQueryable<Rating>> GetQueryableAsync()
        => Task.FromResult(_items.AsAsyncQueryable());

    public Task<Rating> InsertAsync(
        Rating entity,
        bool autoSave = false,
        CancellationToken cancellationToken = default)
    {
        _items.Add(entity);
        return Task.FromResult(entity);
    }

    public Task<List<Rating>> GetListAsync(
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_items.ToList());
    }

    // ---------------------------------------------------------------------
    // El resto de los miembros de IRepository los dejamos con throw
    // ---------------------------------------------------------------------

    public Task<Rating> GetAsync(
        Guid id,
        bool includeDetails = true,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<Rating> FindAsync(
        Guid id,
        bool includeDetails = true,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<long> GetCountAsync(
        CancellationToken cancellationToken = default)
        => Task.FromResult((long)_items.Count);

    public Task<List<Rating>> GetListAsync(
        Expression<Func<Rating, bool>> predicate,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
        => Task.FromResult(_items.AsQueryable().Where(predicate).ToList());

    public Task<Rating> UpdateAsync(
        Rating entity,
        bool autoSave = false,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task DeleteAsync(
        Rating entity,
        bool autoSave = false,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task DeleteAsync(
        Guid id,
        bool autoSave = false,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task DeleteAsync(
        Expression<Func<Rating, bool>> predicate,
        bool autoSave = false,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public IQueryable<Rating> WithDetails()
        => _items.AsQueryable();

    public IQueryable<Rating> WithDetails(params Expression<Func<Rating, object>>[] propertySelectors)
        => _items.AsQueryable();

    public IAsyncQueryableExecuter AsyncExecuter => throw new NotImplementedException();

    public bool? IsChangeTrackingEnabled => throw new NotImplementedException();

    public Task EnsureCollectionLoadedAsync<TCollection>(
        Rating entity,
        Expression<Func<Rating, IEnumerable<TCollection>>> collectionExpression,
        CancellationToken cancellationToken = default)
        where TCollection : class
        => throw new NotImplementedException();

    public Task<Rating?> FindAsync(Expression<Func<Rating, bool>> predicate, bool includeDetails = true, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Rating> GetAsync(Expression<Func<Rating, bool>> predicate, bool includeDetails = true, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteDirectAsync(Expression<Func<Rating, bool>> predicate, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IQueryable<Rating>> WithDetailsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IQueryable<Rating>> WithDetailsAsync(params Expression<Func<Rating, object>>[] propertySelectors)
    {
        throw new NotImplementedException();
    }

    public Task DeleteManyAsync(IEnumerable<Guid> ids, bool autoSave = false, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task InsertManyAsync(IEnumerable<Rating> entities, bool autoSave = false, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateManyAsync(IEnumerable<Rating> entities, bool autoSave = false, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteManyAsync(IEnumerable<Rating> entities, bool autoSave = false, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<Rating>> GetPagedListAsync(int skipCount, int maxResultCount, string sorting, bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}