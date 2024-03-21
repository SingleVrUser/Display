using Display.Setting.Models;

namespace Display.Setting.Interfaces;

internal interface ISearchSetting
{
    internal SearcherItem[] Searchers { get; set; }
}