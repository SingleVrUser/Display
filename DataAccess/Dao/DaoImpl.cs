using System.Data;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccess.Dao;

public class DaoImpl<T> : IDao<T> where T : class
{
    protected readonly Context Context;
    protected readonly DbSet<T> DbSet;

    public DaoImpl()
    {
        Context = new Context();
        DbSet = Context.Set<T>();

    }

    public void ExecuteAdd(T entity)
    {
        DbSet.Add(entity);
        Context.SaveChanges();
    }

    public List<T> List() => DbSet.AsNoTracking().ToList();
    public void SaveChanges()
    {
        Context.SaveChanges();
    }

    public List<T> List(Expression<Func<T, bool>> predicate)
    {
        return DbSet.Where(predicate).AsNoTracking().ToList();
    }

    public void ExecuteUpdate(T entity)
    {
        DbSet.Update(entity);
        SaveChanges();
    }

    public void ExecuteRemoveSingle(T entity)
    {
        DbSet.Remove(entity);
        Context.SaveChanges();
    }

    public T? Find(params object[] keyValues) => DbSet.Find(keyValues);

    public T? FirstOrDefault(Expression<Func<T, bool>> predicate)
    {
        return DbSet.FirstOrDefault(predicate);
    }

    public int Count()
    {
        return DbSet.Count();
    }

    public void Delete()
    {
        DbSet.ExecuteDelete();
    }

    public void Attach(T entity)
    {
        DbSet.Attach(entity);
    }

    public EntityState GetEntityState(T entity)
    {
        return Context.Entry(entity).State;
    }

    public void Remove(T entity)
    {
        DbSet.Remove(entity);
    }


}