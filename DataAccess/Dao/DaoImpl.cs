using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccess.Dao;

public class DaoImpl<T> : IDao<T> where T : class
{
    protected readonly Context Context = Context.Instance;
    protected readonly DbSet<T> DbSet = Context.Instance.Set<T>();

    public void Add(T entity)
    {
        DbSet.Add(entity);
        Context.SaveChanges();
    }

    public List<T> List() => DbSet.ToList();

    public List<T> List(Expression<Func<T, bool>> predicate)
    {
        return DbSet.Where(predicate).ToList();
    }

    public void Update(T entity)
    {
        DbSet.Update(entity);
        Context.SaveChanges();
    }

    public void Remove(T entity)
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
}