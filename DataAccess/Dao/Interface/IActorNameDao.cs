using DataAccess.Models.Entity;

namespace DataAccess.Dao.Interface;

public interface IActorNameDao
{
    void ExecuteRemoveByName(string name);
}