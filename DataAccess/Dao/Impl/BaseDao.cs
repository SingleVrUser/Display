using System.Linq.Expressions;
using DataAccess.Context;
using DataAccess.Dao.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DataAccess.Dao.Impl;

public abstract class BaseDao<TEntity>: IBaseDao<TEntity> where TEntity : class
{
    protected readonly BaseContext Context = new();

    protected readonly DbSet<TEntity> CurrentDbSet;

    private readonly DatabaseFacade _database;

    protected BaseDao()
    {
        CurrentDbSet = Context.Set<TEntity>();
        _database = Context.Database;
    }

    public TEntity? GetById(params object?[]? keyValues)
        =>CurrentDbSet.Find(keyValues);

    public void ExecuteAdd(TEntity entity)
    {
        CurrentDbSet.Add(entity);
        Context.SaveChanges();
    }

    public void ExecuteUpdate(Expression<Func<TEntity, bool>> predicate, Action<TEntity> updateAction)
    {
        var info = CurrentDbSet.FirstOrDefault(predicate);
        if (info == null) return;

        updateAction.Invoke(info);
        Context.SaveChanges();
    }

    public void ExecuteUpdate(TEntity entity)
    {
        CurrentDbSet.Update(entity);
        Context.SaveChanges();
    }

    public void ExecuteRemove(TEntity entity)
    {
        CurrentDbSet.Remove(entity);
        Context.SaveChanges();
    }


    public void InitData()
    {
        _database.EnsureDeleted();
        _database.EnsureCreated();
    }

    public List<TEntity> List()
    {
        return CurrentDbSet.ToList();
    }

    public List<TEntity> List(int offset, int limit)
    {
        return (from info in CurrentDbSet select info).Skip(offset).Take(limit).ToList();
    }

    public int TotalCount()
    {
        return CurrentDbSet.Count();
    }


    public void ExecuteDeleteAll()
    {
        CurrentDbSet.ExecuteDelete();
    }

}