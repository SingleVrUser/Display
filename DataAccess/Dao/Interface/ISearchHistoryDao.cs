using DataAccess.Models.Entity;

namespace DataAccess.Dao.Interface;

public interface ISearchHistoryDao
{
    /// <summary>
    /// 通过id删除
    /// </summary>
    /// <param name="id"></param>
    void ExecuteRemoveById(long id);
    
}