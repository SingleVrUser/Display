using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public static class StaticData
    {
        //115导入数据库 界面的显示地址
        public static string ImportDataAccess_NavigationUrl;

        //public static List
        //导入115数据时是否跳过已导入文件夹（修改时间一致）
        public static bool isJumpExistsFolder = true;
    }
}
