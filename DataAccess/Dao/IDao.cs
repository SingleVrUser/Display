namespace DataAccess.Dao;

internal interface IDao<T>
{
    void Add(T entity);

    List<T> List();
}