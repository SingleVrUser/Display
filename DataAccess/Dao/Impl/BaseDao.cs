using DataAccess.Context;
using DataAccess.Dao.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DataAccess.Dao.Impl;

public abstract class BaseDao<TEntity>: IBaseDao<TEntity> where TEntity : class
{
    private readonly BaseContext _baseContext = new();

    private readonly DbSet<TEntity> _currentDbSet;

    private readonly DatabaseFacade _database;

    protected BaseDao()
    {
        _currentDbSet = _baseContext.Set<TEntity>();
        _database = _baseContext.Database;
    }

    public TEntity? GetById(params object?[]? keyValues)
        =>_currentDbSet.Find(keyValues);

    public void AddAndSaveChanges(TEntity entity)
    {
        _currentDbSet.Add(entity);
        _baseContext.SaveChanges();
    }
    
    public void InitData()
    {
        _database.EnsureDeleted();
        _database.EnsureCreated();
    }
}