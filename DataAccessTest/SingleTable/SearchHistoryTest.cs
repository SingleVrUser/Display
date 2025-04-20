using AutoFixture.NUnit3;
using DataAccess.Dao.Impl;
using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;

namespace DataAccessTest.SingleTable;

public class SearchHistoryTest
{
    private readonly ISearchHistoryDao _searchHistoryDao = new SearchHistoryDao();

    [Test, AutoData]
    public void AddTest(SearchHistory history)
    {
        _searchHistoryDao.ExecuteAdd(history);
    }

    [Test]
    public void DeleteTest()
    {
        var searchHistory = _searchHistoryDao.GetById(10L);
        if (searchHistory == null) return;
        _searchHistoryDao.ExecuteRemove(searchHistory);
    }


    [Test]
    public void ListTest()
    {
        var searchHistory = _searchHistoryDao.List(0,10);
    }
}