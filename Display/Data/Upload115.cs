using Aliyun.OSS;
using Aliyun.OSS.Common;
using Display.Helper;
using Display.Models;
using K4os.Compression.LZ4;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using HttpHeaders = Aliyun.OSS.Util.HttpHeaders;

namespace Display.Data
{
    public class Upload115
    {
        //private const string Md5Salt = "Qclm8MGWUv59TnrR0XPg";
        //private static readonly byte[] CrcSalt = "^j>WD3Kr?J2gLFjD4W2y@"u8.ToArray();


        //private const string UserAgent = Const.;
        //private const string AppVer = Const.UploadAppVersion;

        //private static Upload115 _upload115;
        //public static Upload115 SingleUpload115
        //{
        //    get
        //    {
        //        if (_upload115 != null) return _upload115;

        //        _upload115 = new Upload115();

        //        return _upload115;
        //    }
        //}

        //public Upload115()
        //{
        //    // client
        //    Client = GetInfoFromNetwork.CreateClient(new Dictionary<string, string> { { "user-agent", UserAgent } });
        //    var cookie = AppSettings._115_Cookie;
        //    //cookie不为空且可用
        //    if (!string.IsNullOrEmpty(cookie))
        //    {
        //        Client.DefaultRequestHeaders.Add("Cookie", cookie);
        //    }
        //}

        //public HttpClient Client;

        ////public async Task UploadTo115(string filePath, string cid, int userId, string userKey)
        ////{
        ////    var upload115Result = await FastUpload(filePath, cid, userId, userKey);

        ////    // 需要上传
        ////    if (!string.IsNullOrEmpty(upload115Result._object))
        ////    {
        ////        await OssUploadFile(filePath, upload115Result);
        ////    }
        ////}
    }
}
