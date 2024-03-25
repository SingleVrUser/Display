using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;
using DataAccess.Models.Enum;

namespace DataAccess.Dao.Impl;

public class FailListIsLikeLookLaterDao : DaoImpl<FailListIslikeLookLater>, IFailListIsLikeLookLaterDao
{
    public List<FailListIslikeLookLater> GetInfoList(int position = 0, int take = 100, FailInfoShowType showType = FailInfoShowType.Like)
    {
        return DbSet
            .Skip(position)
            .Where(i => showType == FailInfoShowType.Like ? i.IsLike == 1 : i.LookLater != 0)
            .Take(take)
            .ToList();
    }


}