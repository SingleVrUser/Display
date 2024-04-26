using System.Linq.Expressions;
using DataAccess.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Dao;

public interface IDao<T>
{
    void ExecuteAdd(T entity);

    void ExecuteUpdate(Expression<Func<T, bool>> predicate, Action<T> updateAction);
    
    List<T> List();

    void SaveChanges();

    void Delete();

    void Attach(T entity);

    EntityState GetEntityState(T entity);
}