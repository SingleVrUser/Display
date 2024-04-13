using System.Net.Mime;
using DataAccess.Context;
using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Dao.Impl;

public class ActorNameDao : IActorNameDao
{
    private readonly ActorContext _actorContext = new();
    
    public void ExecuteRemoveByName(string name)
    {
        _actorContext.ActorNames.Where(i => i.Name == name).ExecuteDelete();
    }

}