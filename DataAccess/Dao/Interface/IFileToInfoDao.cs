using DataAccess.Models.Entity;

namespace DataAccess.Dao.Interface;

public interface IFileToInfoDao : IDao<FileToInfo>
{
    void UpdateIsSuccessByTrueName(string trueName, int isSuccess);
}