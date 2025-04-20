using System.Diagnostics;
using DataAccess.Dao.Impl;
using DataAccess.Dao.Interface;

namespace Tests.DataAccess.MoreTest;

[TestClass]
public class ActorInfoTest
{
    private readonly IActorInfoDao _dao = new ActorInfoDao();
    
    [TestMethod]
    [DataRow("AJVR-188")]
    public void GetListByTrueName(string name)
    {
        var listByTrueName = _dao.GetPartListByVideoName(name);
        Debug.WriteLine(listByTrueName);
    }
}