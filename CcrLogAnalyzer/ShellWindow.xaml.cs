using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MVVM.Generic.VM;
using MVVM.Generic.Wins;

namespace CcrLogAnalyzer
{
    /// <summary>
    /// Lógica de interacción para ShellWindow.xaml
    /// </summary>
    public partial class ShellWindow : Window, IWindow
    {
        public ShellWindow(IViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
