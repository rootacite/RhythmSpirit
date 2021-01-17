using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RhythmMaker
{
    /// <summary>
    /// Preview.xaml 的交互逻辑
    /// </summary>
    public partial class Preview : Window
    {
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }
        static public Preview StaticSelf = null;
        public Preview()
        {
            InitializeComponent();
            StaticSelf = this;
        }
    }
}
