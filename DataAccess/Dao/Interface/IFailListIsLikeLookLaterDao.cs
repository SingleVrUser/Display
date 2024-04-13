using DataAccess.Models.Entity;

namespace DataAccess.Dao.Interface;

public interface IFailListIsLikeLookLaterDao
{
    FailListIsLikeLookLater? GetByPickCode(string pickCode);
}