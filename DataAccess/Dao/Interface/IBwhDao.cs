using DataAccess.Models.Entity;

namespace DataAccess.Dao.Interface;

public interface IBwhDao
{
    BwhInfo? GetOne(string content);
}