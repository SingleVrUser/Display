using DataAccess.Models.Entity;

namespace DataAccess.Dao.Interface;

public interface IDownHistoryDao
{
    DownHistory? FindByPickCodeAndUa(string pickCode, string ua);
}