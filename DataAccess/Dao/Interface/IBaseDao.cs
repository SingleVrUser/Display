namespace DataAccess.Dao.Interface;

public interface IBaseDao<TEntity>
{
    TEntity? GetById(params object?[]? keyValues);
    
    /// <summary>
    /// 添加并保存
    /// </summary>
    /// <param name="entity"></param>
    void AddAndSaveChanges(TEntity entity);

    /// <summary>
    /// 初始化数据库
    /// </summary>
    void InitData();
}