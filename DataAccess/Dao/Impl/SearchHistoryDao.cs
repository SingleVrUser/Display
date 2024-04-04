using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Dao.Impl;

public class SearchHistoryDao : DaoImpl<SearchHistory>, ISearchHistoryDao
{
    public void ExecuteRemoveById(long id)
    {
        DbSet.Where(i => i.Id == id).ExecuteDelete();
    }
    
}