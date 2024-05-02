using System.Linq.Expressions;

namespace DataAccess.Dao.Interface;

public interface IBaseDao<TEntity>
{
    TEntity? GetById(params object?[]? keyValues);
    
    /// <summary>
    /// 添加并保存
    /// </summary>
    /// <param name="entity"></param>
    void ExecuteAdd(TEntity entity);

    /// <summary>
    /// 更新并保存
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="updateAction"></param>
    void ExecuteUpdate(Expression<Func<TEntity, bool>> predicate, Action<TEntity> updateAction);

    /// <summary>
    /// 初始化数据库
    /// </summary>
    void InitData();

}