using DataAccess.Models.Entity;

namespace DataAccess.Dao.Interface;

public interface IDownHistoryDao : IBaseDao<DownHistory>
{
    DownHistory? FindByPickCodeAndUa(string pickCode, string ua);
}