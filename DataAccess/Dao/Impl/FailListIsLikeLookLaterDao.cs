using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;
using DataAccess.Models.Enum;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Dao.Impl;

public class FailListIsLikeLookLaterDao : DaoImpl<FailListIsLikeLookLater>, IFailListIsLikeLookLaterDao
{
    public List<FailListIsLikeLookLater> GetInfoList(int position = 0, int take = 100, FailInfoShowType showType = FailInfoShowType.Like)
    {
        return DbSet
            .Skip(position)
            .Where(i => showType == FailInfoShowType.Like ? i.IsLike == 1 : i.LookLater != 0)
            .Take(take)
            .ToList();
    }

    public FailListIsLikeLookLater? GetByPickCode(string pickCode)
    {
        return DbSet.Find(pickCode);
    }
}