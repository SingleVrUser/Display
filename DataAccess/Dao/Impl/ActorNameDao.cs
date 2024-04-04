using System.Net.Mime;
using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Dao.Impl;

public class ActorNameDao : DaoImpl<ActorName>, IActorNameDao
{
    public void ExecuteRemoveByName(string name)
    {
        // var actorNameList = (from c in DbSet where c.Name == name select c).ToList();
        // actorNameList.ForEach(item => DbSet.Remove(item));
        
        DbSet.Where(i => i.Name == name).ExecuteDelete();
    }
}