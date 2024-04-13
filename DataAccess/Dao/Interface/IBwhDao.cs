using DataAccess.Models.Entity;

namespace DataAccess.Dao.Interface;

public interface IBwhDao
{
    Bwh? GetOne(string content);
}