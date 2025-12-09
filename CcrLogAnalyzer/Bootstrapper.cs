using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Castle.Facilities.Logging;
using Castle.Facilities.TypedFactory;
using Castle.Services.Logging.NLogIntegration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using CcrLogAnalyzer.Factories;
using MVVM.Generic.VM;
using MVVM.Generic.Wins;

namespace CcrLogAnalyzer
{
    /// <summary>
    /// Main class
    /// </summary>
    public class Bootstrapper
    {
        #region FIELDS
        private ILogger _logger;
        private IWindow _shellWin;
        private IWindsorContainer _container = new WindsorContainer();
        #endregion

        /// <summary>
        /// Starts bootstrapper 
        /// </summary>
        /// <returns></returns>
        public async Task Start()
        {
            ConfigureContainer();
            _logger = _container.Resolve<ILogger>();
            IVMFactory vmFactory = _container.Resolve<IVMFactory>();
            IWindowFactory winFactory = _container.Resolve<IWindowFactory>();

            BaseViewModel shellVM = vmFactory.GetShellVM();
            BaseViewModel mainVM = vmFactory.GetMainVM();
            BaseViewModel topbarVM = vmFactory.GetTopbarVM();

            shellVM.SetChildsVM(new List<BaseViewModel> { mainVM });
            shellVM.SetInitialView("Main");

            IWindow win = winFactory.GetShellWindow(shellVM);
            _shellWin = win;

            System.Windows.Application.Current.MainWindow = (System.Windows.Window)win;

            ((System.Windows.Window)win).Closed += (s, e) =>
            {
                Stop();
                System.Windows.Application.Current.Shutdown();
            };

            win.Show();

            _logger.Info("Application started.");
        }
        /// <summary>
        /// Stops bootstrapper
        /// </summary>
        public void Stop()
        {
            _logger.Info("Application closed.");
            _container.Release(_shellWin);           
            _container.Dispose();
        }
        
        private void ConfigureContainer()
        {
            _container.AddFacility<TypedFactoryFacility>();
            _container.AddFacility<LoggingFacility>(f => f.LogUsing<NLogFactory>());
            //_container.Kernel.Resolver.AddSubResolver(new CollectionResolver(_container.Kernel));
            _container
                .Install(
                FromAssembly.This());
        }
    }
}