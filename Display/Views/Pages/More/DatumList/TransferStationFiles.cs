using System.Collections.Generic;
using System.Linq;
using Display.Models.Api.OneOneFive.File;
using Display.Models.Dto.OneOneFive;
using Display.Models.Vo.OneOneFive;

namespace Display.Views.Pages.More.DatumList;

internal class TransferStationFiles
{
    public string Name { get; set; }
    public List<FilesInfo> TransferFiles { get; set; }

    public TransferStationFiles(List<FilesInfo> transferFiles)
    {
        if (transferFiles.Count == 1)
        {
            this.Name = $"{transferFiles.FirstOrDefault()?.Name}";
        }
        else if (transferFiles.Count > 1)
        {
            this.Name = $"{transferFiles.FirstOrDefault()?.Name} 等{transferFiles.Count}个文件";
        }

        this.TransferFiles = transferFiles;

    }

}