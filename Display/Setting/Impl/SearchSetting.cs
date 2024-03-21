using Display.Setting.Interfaces;
using Display.Setting.Models;

namespace Display.Setting.Impl;

internal class SearchSetting : SettingBase, ISearchSetting
{
    public SearchSetting(ISettingProvider provider) : base(provider)
    {
    }

    public SearcherItem[] Searchers
    {
        get => throw new System.NotImplementedException();
        set => throw new System.NotImplementedException();
    }
}
