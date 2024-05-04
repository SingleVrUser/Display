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
    /// 更新某个字段并保存
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="updateAction"></param>
    void ExecuteUpdate(Expression<Func<TEntity, bool>> predicate, Action<TEntity> updateAction);

    /// <summary>
    /// 更新并保存
    /// </summary>
    /// <param name="entity"></param>
    void ExecuteUpdate(TEntity entity);

    /// <summary>
    /// 删除并保存
    /// </summary>
    /// <param name="entity"></param>
    void ExecuteRemove(TEntity entity);
    
    /// <summary>
    /// 初始化数据库
    /// </summary>
    void InitData();

    /// <summary>
    /// 获取所有的列表
    /// </summary>
    /// <returns></returns>
    List<TEntity> List();

    /// <summary>
    /// 分页查询列表
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    List<TEntity> List(int offset, int limit);
    
    /// <summary>
    /// 查询总数量
    /// </summary>
    /// <returns></returns>
    int TotalCount();

    /// <summary>
    /// 删除所有的数据
    /// </summary>
    void ExecuteDeleteAll();

}