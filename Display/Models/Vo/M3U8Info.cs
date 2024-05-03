using System;
using System.Collections.Generic;
using System.Linq;
using Display.Models.Dto.OneOneFive;

namespace Display.Models.Vo;

public class M3U8Info(string name, string bandwidth, string resolution, string url)
{
    public static M3U8Info CreateInstance(string name, string bandwidth, string resolution, string url)
    {
        return new M3U8Info(name, bandwidth, resolution, url);
    }

    public string Name { get; set; } = name;
    public string Bandwidth { get; set; } = bandwidth;
    public string Resolution { get; set; } = resolution;
    public string Url { get; set; } = url;

    public double TotalSecond => TsInfoList.Sum(x => x.Second);

    public string BaseUrl
    {
        get
        {
            var urlInfo = new Uri(Url);

            return $"{urlInfo.Scheme}://{urlInfo.Host}";
        }
    }

    public List<TsInfo> TsInfoList = [];

}