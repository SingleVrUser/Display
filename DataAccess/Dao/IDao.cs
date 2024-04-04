namespace DataAccess.Dao;

public interface IDao<T>
{
    void Add(T entity);
    
    List<T> List();

    void SaveChanges();

    void UpdateSingle(T entity);

    void Delete();
}