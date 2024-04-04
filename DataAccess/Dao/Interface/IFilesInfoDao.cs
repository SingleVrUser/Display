using DataAccess.Models.Dto;
using DataAccess.Models.Entity;

namespace DataAccess.Dao.Interface;

public interface IFilesInfoDao : IDao<FilesInfo>
{
    /// <summary>
    /// 通过name获取列表
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    List<FilesInfo> GetListByTrueName(string name);
    
    void GetVideoInfoInFailStatusList(VideoInfoInFailStatusQueryDto queryDto);

    /// <summary>
    /// 通过目录id查询所有文件列表（包括文件夹）
    /// </summary>
    /// <param name="folderId"></param>
    /// <returns></returns>
    List<FilesInfo> GetAllFilesListByFolderId(long folderId);
    
    /// <summary>
    /// 获取文件夹pid为id的文件夹列表，深度只有一级
    /// </summary>
    /// <param name="pid">文件夹的父级Id</param>
    /// <param name="limit">限制数量</param>
    /// <returns></returns>
    List<FilesInfo> GetPartFolderListByPid(long pid, int? limit = null);
    
    /// <summary>
    /// 通过文件夹的id删除该文件夹以及文件夹下的所有文件和文件夹
    /// </summary>
    /// <param name="folderId">文件id或者文件夹id</param>
    void RemoveAllByFolderId(long folderId);

    /// <summary>
    /// 通过pickCode删除记录
    /// </summary>
    /// <param name="pickCode"></param>
    void RemoveByPickCode(string pickCode);

    /// <summary>
    /// 通过trueName删除记录
    /// </summary>
    /// <param name="trueName"></param>
    void ExecuteRemoveByTrueName(string trueName);
    
    /// <summary>
    /// 通过pickCode查询
    /// </summary>
    /// <param name="pickCode"></param>
    /// <returns></returns>
    FilesInfo? GetOneByPickCode(string pickCode);

    FilesInfo? GetUpperLevelFolderInfoByFolderId(long id);
    List<FilesInfo> GetFolderListToRootByFolderId(long folderCid);
}