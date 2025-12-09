using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using CcrLogAnalyzer.Factories;
using CcrLogAnalyzer.ViewModels.Main;
using CcrLogAnalyzer.ViewModels.Topbar;
using MVVM.Generic.VM;

namespace CcrLogAnalyzer.Ioc.Installers
{
    public class ViewModelInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<BaseViewModel>().
                ImplementedBy<ShellVM>()
                .DependsOn(Dependency.OnValue("name", "Shell"))
                .Named(nameof(ShellVM))
                .LifestyleSingleton(),
                Component.For<BaseViewModel>().
                ImplementedBy<MainVM>()
                .DependsOn(Dependency.OnValue("name", "Main"))
                .Named(nameof(MainVM))
                .LifestyleSingleton(),
                Component.For<BaseViewModel>().
                ImplementedBy<TopbarVM>()
                .DependsOn(Dependency.OnValue("name", "Topbar"))
                .Named(nameof(TopbarVM))
                .LifestyleSingleton(),
                Component.For<IVMFactory>()
                    .AsFactory()
                );
        }
    }
}
