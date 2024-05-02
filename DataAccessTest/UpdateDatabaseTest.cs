﻿using DataAccess.Context;

namespace DataAccessTest.DataAccess;

public class UpdateDatabaseTest
{
    private readonly BaseContext _baseContext = new();
    
    [Test]
    public void UpdateTest()
    {
        _baseContext.Database.EnsureDeleted();
        _baseContext.Database.EnsureCreated();
    }
}