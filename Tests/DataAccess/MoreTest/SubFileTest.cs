using DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Tests.DataAccess.MoreTest;

[TestClass]
public class SubFileTest
{
    [TestMethod]
    public void FindSubTest()
    {
        string[] srtSuffix = ["srt", "ass", "ssa"];
        var leftName = "mdbk";
        var rightNumber = 273;
        var subArray = Context.Instance.FilesInfos.Where(i=>srtSuffix.Contains(i.Ico )
                                                            &&  EF.Functions.Like(i.Name, "%"+leftName+"%"+rightNumber+"%")).ToArray();
        Assert.IsTrue(subArray.Length == 1);
    }
}