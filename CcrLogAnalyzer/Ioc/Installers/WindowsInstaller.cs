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
using MVVM.Generic.Wins;

namespace CcrLogAnalyzer.Ioc.Installers
{
    public class WindowsInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IWindow>().
                ImplementedBy<ShellWindow>()
                .Named(nameof(ShellWindow))
                .LifestyleTransient(),
                Component.For<IWindowFactory>()
                    .AsFactory()
                );
        }
    }
}
