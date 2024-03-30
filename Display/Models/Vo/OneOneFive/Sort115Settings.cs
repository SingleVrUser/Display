using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Display.Models.Dto.OneOneFive;
using Display.Models.Enums;
using Display.Models.Records;

namespace Display.Models.Vo.OneOneFive;

public partial class Sort115Settings : ObservableObject
{
    public enum NumNameCapFormat
    {
        Original, Upper, Lower
    }

    public static List<Tuple<string, PartNumCombo>> PartNumCombos =
    [
        new Tuple<string, PartNumCombo>("中文数字", new PartNumCombo(PartNum.Chinese, "一、二、三")),
        new Tuple<string, PartNumCombo>("阿拉伯数字", new PartNumCombo(PartNum.Arabic, "1、2、3")),
        new Tuple<string, PartNumCombo>("罗马数字", new PartNumCombo(PartNum.Roman, "I、II、III")),
        new Tuple<string, PartNumCombo>("英文字母", new PartNumCombo(PartNum.English, "a、b、c")),
        new Tuple<string, PartNumCombo>("大写英文字母", new PartNumCombo(PartNum.CapsEnglish, "A、B、C"))
    ];

    public static readonly List<Tuple<string, NumNameCapFormat>> NumNameFormatCombos =
    [
        new Tuple<string, NumNameCapFormat>("原样", NumNameCapFormat.Original),
        new Tuple<string, NumNameCapFormat>("大写", NumNameCapFormat.Upper),
        new Tuple<string, NumNameCapFormat>("小写", NumNameCapFormat.Lower)
    ];

    [ObservableProperty]
    private ExplorerItem _singleVideoSaveExplorerItem;

    [ObservableProperty]
    private ExplorerItem _multipleVideoSaveExplorerItem;
}