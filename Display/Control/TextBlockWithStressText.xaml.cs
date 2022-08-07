using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Control
{
    public sealed partial class TextBlockWithStressText : UserControl
    {
        //public string Text { get; set; }
        //public string StressText { get; set; }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public string StressText
        {
            get { return (string)GetValue(StressTextProperty); }
            set
            {
                SetValue(StressTextProperty, value);
                showTextContent();

            }
        }

        //依赖属性
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TextBlockWithStressText), null);

        //依赖属性
        public static readonly DependencyProperty StressTextProperty =
            DependencyProperty.Register("StressText", typeof(string), typeof(TextBlockWithStressText), null);


        public TextBlockWithStressText()
        {
            this.InitializeComponent();
        }

        private void showTextContent()
        {
            MatchCollection mc = Regex.Matches(Text, @"(.*)(" + StressText + ")(.*)");
            if (mc.Count > 0)
            {
                Content1.Text = mc[0].Groups[1].Value;
                StressContent.Text = mc[0].Groups[2].Value;
                Content2.Text = mc[0].Groups[3].Value;
            }
            Text.Split(StressText);

        }
    }
}
