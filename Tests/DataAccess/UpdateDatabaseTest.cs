using DataAccess.Context;

namespace Tests.DataAccess;

[TestClass]
public class UpdateDatabaseTest
{
    private BaseContext _baseContext = new();
    
    [TestMethod]
    public void UpdateTest()
    {
        _baseContext.Database.EnsureDeleted();
        _baseContext.Database.EnsureCreated();
    }
}