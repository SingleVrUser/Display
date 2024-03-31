using System.Diagnostics;
using Display.Models.Api.OneOneFive.File;
using Newtonsoft.Json;

namespace Tests;

[TestClass]
public class JsonTest
{
    [TestMethod]
    public void JsonImageInfoTest()
    {
        var content =
            "{\"state\":true,\"data\":{\"url\":\"https:\\/\\/thumb.115.com\\/thumb\\/1\\/57\\/80\\/15780C9FD8C04E11690C3C847DF2DBA7DBDE1DEA_1440?s=DXmqdOAPNdsfVBZUPG2Uiw&t=1711888237\",\"all_url\":[\"https:\\/\\/thumb.115.com\\/thumb\\/1\\/57\\/80\\/15780C9FD8C04E11690C3C847DF2DBA7DBDE1DEA_100?s=DXmqdOAPNdsfVBZUPG2Uiw&t=1711888237\",\"https:\\/\\/thumb.115.com\\/thumb\\/1\\/57\\/80\\/15780C9FD8C04E11690C3C847DF2DBA7DBDE1DEA_480?s=DXmqdOAPNdsfVBZUPG2Uiw&t=1711888237\",\"https:\\/\\/thumb.115.com\\/thumb\\/1\\/57\\/80\\/15780C9FD8C04E11690C3C847DF2DBA7DBDE1DEA_800?s=DXmqdOAPNdsfVBZUPG2Uiw&t=1711888237\"],\"origin_url\":\"https:\\/\\/thumb.115.com\\/thumb\\/1\\/57\\/80\\/15780C9FD8C04E11690C3C847DF2DBA7DBDE1DEA_0?s=DXmqdOAPNdsfVBZUPG2Uiw&t=1711888237\",\"source_url\":\"https:\\/\\/cdnfhnfile.115.com\\/6506e198378da9e02ccb0cd8238a57c61ef6ec26\\/%E4%B8%93%E4%B8%9A%E5%9B%A2%E9%98%9F.jpg?t=1711885588&u=343058794&s=524288000&d=vip-1901339175-cw6ully045i2adbf0-1&c=0&f=1&k=e2700cfeead6f18451ed4ac1c7528369&us=5242880000&uc=10&v=1\",\"file_name\":\"\\u4e13\\u4e1a\\u56e2\\u961f.jpg\",\"file_sha1\":\"15780C9FD8C04E11690C3C847DF2DBA7DBDE1DEA\",\"pick_code\":\"cw6ully045i2adbf0\"}}";

        var info = JsonConvert.DeserializeObject<ImageInfo>(content);
        Debug.WriteLine(info);
    }
}