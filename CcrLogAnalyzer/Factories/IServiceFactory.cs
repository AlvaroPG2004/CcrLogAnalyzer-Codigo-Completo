using MVVM.Generic.Services;
using MVVM.Generic.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CcrLogAnalyzer.Factories
{
    public interface IServiceFactory
    {
        IFileExplorerDialog GetFileExplorerDialog();
    }
}
