using Display.Setting.Interfaces;
using Display.Setting.Models;

namespace Display.Setting.Impl;

internal class SearchSetting(ISettingProvider provider) : SettingBase(provider), ISearchSetting
{
    public SearcherItem[] Searchers
    {
        get => throw new System.NotImplementedException();
        set => throw new System.NotImplementedException();
    }
}
