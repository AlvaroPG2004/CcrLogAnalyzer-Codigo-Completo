using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MVVM.Generic.VM;

namespace CcrLogAnalyzer.Factories
{
    public interface IVMFactory
    {
        BaseViewModel GetShellVM();
        BaseViewModel GetMainVM();
        BaseViewModel GetTopbarVM();
    }
}
