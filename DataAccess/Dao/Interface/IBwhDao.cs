using DataAccess.Models.Entity;

namespace DataAccess.Dao.Interface;

public interface IBwhDao : IDao<Bwh>
{
    Bwh? GetOne(string content);
}