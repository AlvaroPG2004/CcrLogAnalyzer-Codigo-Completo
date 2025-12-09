using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MVVM.Generic.VM;
using MVVM.Generic.Wins;

namespace CcrLogAnalyzer.Factories
{
    public interface IWindowFactory
    {
        IWindow GetShellWindow(IViewModel viewModel);
    }
}
