using DataAccess.Models.Entity;

namespace DataAccess.Dao.Interface;

public interface IFailListIsLikeLookLaterDao : IDao<FailListIsLikeLookLater>
{
    FailListIsLikeLookLater? GetByPickCode(string pickCode);
}