using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using CcrLogAnalyzer.Factories;
using CcrLogAnalyzer.ViewModels.Main;
using CcrLogAnalyzer.ViewModels.Topbar;
using MVVM.Generic.Services;
using MVVM.Generic.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CcrLogAnalyzer.Ioc.Installers
{
    public class ServicesInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IFileExplorerDialog>().
                ImplementedBy<FileExplorerDialog>()
                .Named(nameof(FileExplorerDialog)),
                Component.For<IServiceFactory>()
                    .AsFactory()
                );
        }
    }
}