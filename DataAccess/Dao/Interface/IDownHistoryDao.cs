using DataAccess.Models.Entity;

namespace DataAccess.Dao.Interface;

public interface IDownHistoryDao : IDao<DownHistory>
{
    DownHistory? FindByPickCodeAndUa(string pickCode, string ua);
}