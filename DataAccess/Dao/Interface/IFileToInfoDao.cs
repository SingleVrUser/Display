using DataAccess.Models.Entity;

namespace DataAccess.Dao.Interface;

public interface IFileToInfoDao
{
    void UpdateIsSuccessByTrueName(string trueName, bool isSuccess);
}