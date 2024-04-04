using DataAccess.Models.Entity;

namespace DataAccess.Dao.Interface;

public interface IActorNameDao : IDao<ActorName>
{
    void ExecuteRemoveByName(string name);
}