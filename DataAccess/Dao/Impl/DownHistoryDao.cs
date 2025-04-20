using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;

namespace DataAccess.Dao.Impl;

public class DownHistoryDao : BaseDao<DownHistory>, IDownHistoryDao
{
    public DownHistory? FindByPickCodeAndUa(string pickCode, string ua)
    {
        return CurrentDbSet.FirstOrDefault(i => i.FilePickCode.Equals(pickCode) && i.Ua.Equals(ua));
    }
}