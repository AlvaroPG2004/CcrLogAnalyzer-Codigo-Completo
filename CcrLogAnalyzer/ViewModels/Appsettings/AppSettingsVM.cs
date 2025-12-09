using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CcrLogAnalyzer.ViewModels.Appsettings
{
    public class AppSettingsVM
    {
        public List<ConfigEntry> History { get; set; } = new();
    }

    public class ConfigEntry
    {
        public string LastFolder { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }

        public string DisplayText => $"{LastFolder}  |  {StartTime} -> {EndTime}";

    }
}
