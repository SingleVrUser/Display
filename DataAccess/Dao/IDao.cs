using Microsoft.EntityFrameworkCore;

namespace DataAccess.Dao;

public interface IDao<T>
{
    void ExecuteAdd(T entity);
    void ExecuteUpdate(T entity);

    List<T> List();

    void SaveChanges();

    void Delete();

    void Attach(T entity);

    EntityState GetEntityState(T entity);
}