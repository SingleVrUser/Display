using System.Data;
using System.Diagnostics;
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
        try
        {
            Context.SaveChanges();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(typeof(T).Name + "添加数据时错误:" + ex.Message);
        }
    }

    public List<T> List() => DbSet.ToList();
    public void SaveChanges()
    {
        Context.SaveChanges();
    }

    public List<T> List(Expression<Func<T, bool>> predicate)
    {
        return DbSet.Where(predicate).ToList();
    }

    public void UpdateSingle(T entity)
    {
        DbSet.Update(entity);
        Context.SaveChanges();
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
}