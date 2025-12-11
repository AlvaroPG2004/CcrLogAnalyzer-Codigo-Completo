using CcrLogAnalyzer.Factories;
using CcrLogAnalyzer.Models;
using CcrLogAnalyzer.ViewModels.Appsettings;
using MVVM.Generic.Commands;
using MVVM.Generic.Services;
using MVVM.Generic.VM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace CcrLogAnalyzer.ViewModels.Main
{
    // Información de cada archivo o línea filtrada
    public class LogFileEventInfo
    {
        public string NameLog { get; set; }
        public string FilePath { get; set; }
        public int EventCount { get; set; }
        public string LineContent { get; set; }
        public string EventType { get; set; }
        public bool IsVisible { get; set; } = true;


    }

    public class MainVM : BaseViewModel
    {
        private readonly IFileExplorerDialog _fileExplorerDialog;
        private ICommand _browseCommand;

        // Comando para filtrar por tiempo
        public ICommand FilterCommand { get; }

        // Opciones del ComboBox (archivo o carpeta)
        public ObservableCollection<string> ComboOptions { get; }
        public ObservableCollection<string> DistinctEvents { get; set; } = new();



        private string _selectedEvent;
        public string SelectedEvent
        {
            get => _selectedEvent;
            set
            {
                _selectedEvent = value;
                RaisePropertyChanged();
                ApplyEventFilter();
            }
        }
        // Filtro de los eventos al selecionar desde el comboBox
        private void ApplyEventFilter()
        {
            if (SelectedEvent == null || SelectedEvent == "Todos")
            {
                foreach (var item in LogFileEventCounts)
                    item.IsVisible = true;
            }
            else
            {
                foreach (var item in LogFileEventCounts)
                    item.IsVisible = item.EventType == SelectedEvent;
            }
            FilteredEventsView.Refresh();
            RaisePropertyChanged(nameof(LogFileEventCounts));

        }


        private string _selectedOption;
        public string SelectedOption
        {
            get => _selectedOption;
            set
            {
                _selectedOption = value;
                RaisePropertyChanged();
                // Actualiza el comando browse según la opción
                OnOptionSelected(value);
            }
        }



        // Configuración seleccionada del historial
        private ConfigEntry _selectedConfig;
        public ConfigEntry SelectedConfig
        {
            get => _selectedConfig;
            set
            {
                _selectedConfig = value;

                // Si el usuario elige una configuración previa, carga sus datos
                if (value != null)
                {
                    BrowsePath = value.LastFolder;
                    StartTime = value.StartTime;
                    EndTime = value.EndTime;

                    // Actualizar los textbox
                    RaisePropertyChanged(nameof(StartTime));
                    RaisePropertyChanged(nameof(EndTime));
                    RaisePropertyChanged(nameof(BrowsePath));

                    // Cargar archivo o carpeta automáticamente
                    _ = LoadFromConfigAsync();
                }

                RaisePropertyChanged();
            }
        }

        // Carga automática cuando se selecciona un elemento del historial
        private async Task LoadFromConfigAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(BrowsePath))
                    return;

                LogFileEventCounts.Clear();

                // Cargar archivo
                if (File.Exists(BrowsePath))
                {
                    await LoadSingleFileAsync(BrowsePath);
                }
                // Cargar carpeta
                else if (Directory.Exists(BrowsePath))
                {
                    await Task.Run(() => LoadLogFilesAndCountEvents(BrowsePath));
                }

                // Aplicar filtro después de cargar
                await FilterAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error cargando desde configuración: {ex.Message}");
            }
        }
        // Cargar automaticamente la ruta cuando se pone en el textbox
        private async Task AutoLoadAsync()
        {
            if (string.IsNullOrEmpty(BrowsePath))
                return;

            if (File.Exists(BrowsePath))
            {
                await LoadSingleFileAsync(BrowsePath);
                UpdateDistinctEvents();
            }
            else if (Directory.Exists(BrowsePath))
            {
                await Task.Run(() => LoadLogFilesAndCountEvents(BrowsePath));
                UpdateDistinctEvents();
            }
        }


        public ICommand BrowseCommand => _browseCommand;

        // Lista mostrada en el DataGrid
        public ObservableCollection<LogFileEventInfo> LogFileEventCounts { get; set; } = new();
        // Coleccion de eventos Filtrados por el comboBox
        public ICollectionView FilteredEventsView { get; set; }

        private string _browsePath;
        // BrowserPath Setter y Getter
        public string BrowsePath
        {
            get => _browsePath;
            set
            {
                _browsePath = value;
                RaisePropertyChanged();
                _ = AutoLoadAsync();
            }
        }

        // Campos para el filtro por tiempo
        public string StartTime { get; set; } = "0:0:0:000";
        public string EndTime { get; set; } = "0:0:10:000";

        // Expresión regular para detectar eventos
        private readonly Regex _eventPattern =
            new(@"(ERROR|EVENT|WARN|REMOTE|LOCAL|TIME|0/0/0)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Expresión regular para detectar timestamps
        private readonly Regex _timeRegex =
            new(@"^\d+/\d+/\d+\s+(\d+:\d+:\d+:\d+)", RegexOptions.Compiled);

        // Constructor principal del ViewModel
        public MainVM(string name, IVMFactory vmFactory, IFileExplorerDialog fileExplorerDialog)
            : base(name)
        {
            _fileExplorerDialog = fileExplorerDialog;

            ComboOptions = new ObservableCollection<string>
            {
                "Archivos",
                "Carpetas"
            };

            SelectedOption = ComboOptions.First();

            // Comandos principales
            _browseCommand = new DelegateCommand<string>(async path => await ExecuteBrowseAsync(path));
            FilterCommand = new DelegateCommand(async () => await FilterAsync());

            // Cargar historial
            LoadSettings();

            // Crear vista filtrada de eventos especificos para el DataGrid 
            FilteredEventsView = CollectionViewSource.GetDefaultView(LogFileEventCounts);
            FilteredEventsView.Filter = item =>
            {
                if (item is LogFileEventInfo log)
                    return log.IsVisible;
                return true;
            };

        }



        // Cuando el usuario cambia entre archivos/carpeta en el buscador
        private void OnOptionSelected(string option)
        {
            _browseCommand = new DelegateCommand<string>(async path => await ExecuteBrowseAsync(path));
            RaisePropertyChanged(nameof(BrowseCommand));
        }

        // Ejecuta la búsqueda (archivo o carpeta)
        private async Task ExecuteBrowseAsync(string path)
        {
            switch (SelectedOption)
            {
                case "Archivos":
                    var filePath = _fileExplorerDialog.OpenFileDialog(path);
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        BrowsePath = filePath;
                        await LoadSingleFileAsync(filePath);
                        SaveSettings();
                    }
                    break;

                case "Carpetas":
                    var folderPath = _fileExplorerDialog.OpenDirectoryDialog(path);
                    if (!string.IsNullOrEmpty(folderPath))
                    {
                        BrowsePath = folderPath;
                        await Task.Run(() => LoadLogFilesAndCountEvents(folderPath));
                        SaveSettings();
                    }
                    break;
            }
        }

        // Cargar un único archivo y contar eventos
        private async Task LoadSingleFileAsync(string filePath)
        {
            LogFileEventCounts.Clear();

            if (!new[] { ".grplog" }.Contains(Path.GetExtension(filePath)?.ToLower()))
                return;

            int count = await Task.Run(() => CountEventsInFile(filePath));

            LogFileEventCounts.Add(new LogFileEventInfo
            {
                NameLog = Path.GetFileName(filePath),
                FilePath = filePath,
                EventCount = count
            });
            UpdateDistinctEvents();
        }

        // Cargar una carpeta con múltiples logs
        private void LoadLogFilesAndCountEvents(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                return;

            try
            {
                var logFiles = Directory.GetFiles(folderPath, "*.grplog");
                var items = new List<LogFileEventInfo>();

                foreach (var filePath in logFiles)
                {
                    int count = CountEventsInFile(filePath);
                    items.Add(new LogFileEventInfo
                    {
                        NameLog = Path.GetFileName(filePath),
                        FilePath = filePath,
                        EventCount = count
                    });
                }

                App.Current.Dispatcher.Invoke(() =>
                {
                    LogFileEventCounts.Clear();
                    foreach (var item in items)
                        LogFileEventCounts.Add(item);

                    UpdateDistinctEvents();
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error leyendo carpeta: {ex.Message}");
            }
        }

        // Cuenta eventos dentro de un archivo
        private int CountEventsInFile(string filePath)
        {
            int count = 0;

            try
            {
                foreach (var line in File.ReadLines(filePath))
                {
                    if (_eventPattern.IsMatch(line))
                    {
                        string eventType = DetectEventType(line);
                        count++;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error analizando archivo: {ex.Message}");
            }

            return count;
        }

        // Filtro por tiempo
        private async Task FilterAsync()
        {
            if (string.IsNullOrEmpty(BrowsePath))
                return;

            string filePath = null;

            // Caso 1: archivo individual
            if (File.Exists(BrowsePath) && BrowsePath.EndsWith(".grplog"))
            {
                filePath = BrowsePath;
            }
            else
            {
                // Caso 2: carpeta -> requiere seleccionar un archivo
                if (SelectedLog == null)
                {
                    MessageBox.Show("Selecciona primero un archivo del listado.", "Aviso");
                    return;
                }

                filePath = SelectedLog.FilePath;
            }

            if (filePath == null || !File.Exists(filePath))
                return;

            var filteredLines = await Task.Run(() => FilterLogByTime(filePath, StartTime, EndTime));

            App.Current.Dispatcher.Invoke(() =>
            {
                LogFileEventCounts.Clear();
                foreach (var line in filteredLines)
                {
                    LogFileEventCounts.Add(new LogFileEventInfo
                    {

                        NameLog = Path.GetFileName(filePath),
                        FilePath = filePath,
                        LineContent = line,
                        EventCount = 1,
                        EventType = DetectEventType(line)
                    });

                }

            });
            UpdateDistinctEvents();
            SaveSettings();
        }

        // Aplicar filtro por tiempo en un archivo
        private IEnumerable<string> FilterLogByTime(string filePath, string startTime, string endTime)
        {
            var result = new List<string>();

            TimeSpan start = ParseTime(startTime);
            TimeSpan end = ParseTime(endTime);

            foreach (var line in File.ReadLines(filePath))
            {
                var match = _timeRegex.Match(line);

                if (match.Success)
                {
                    var timeValue = ParseTime(match.Groups[1].Value);

                    if (timeValue >= start && timeValue <= end)
                        result.Add(line);
                }
            }

            return result;
        }

        // Convierte string a TimeSpan
        private TimeSpan ParseTime(string time)
        {
            var parts = time.Split(':');

            if (parts.Length == 4 &&
                int.TryParse(parts[0], out int h) &&
                int.TryParse(parts[1], out int m) &&
                int.TryParse(parts[2], out int s) &&
                int.TryParse(parts[3], out int ms))
            {
                return new TimeSpan(0, h, m, s, ms);
            }

            return TimeSpan.Zero;
        }

        // Tab de vista previa
        private string _filePreviewContent;
        public string FilePreviewContent
        {
            get => _filePreviewContent;
            set
            {
                _filePreviewContent = value;
                RaisePropertyChanged();
            }
        }

        // Archivo seleccionado en el DataGrid
        private LogFileEventInfo _selectedLog;
        public LogFileEventInfo SelectedLog
        {
            get => _selectedLog;
            set
            {
                _selectedLog = value;
                RaisePropertyChanged();

                // Vista previa automática SIN filtrar
                if (_selectedLog != null && File.Exists(_selectedLog.FilePath))
                {
                    try
                    {
                        FilePreviewContent = File.ReadAllText(_selectedLog.FilePath);
                    }
                    catch
                    {
                        FilePreviewContent = "No se pudo leer el archivo.";
                    }
                }
                else
                {
                    FilePreviewContent = string.Empty;
                }
            }
        }


        private const string SettingsFile = "settings.json";

        // Historial de configuraciones
        public ObservableCollection<ConfigEntry> ConfigHistory { get; set; } = new();

        // Cargar historial desde archivo JSON
        private void LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFile))
                {
                    var json = File.ReadAllText(SettingsFile);
                    var settings = JsonSerializer.Deserialize<AppSettingsVM>(json);

                    if (settings != null && settings.History.Any())
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            ConfigHistory.Clear();
                            foreach (var item in settings.History)
                                ConfigHistory.Add(item);
                        });

                        var last = settings.History.Last();

                        BrowsePath = last.LastFolder ?? string.Empty;
                        StartTime = last.StartTime ?? "0:0:0:000";
                        EndTime = last.EndTime ?? "0:0:10:000";
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error cargando configuración: {ex.Message}");
            }
        }

        // Guardar nueva configuración en historial JSON
        private void SaveSettings()
        {
            try
            {
                AppSettingsVM settings;

                if (File.Exists(SettingsFile))
                {
                    var json = File.ReadAllText(SettingsFile);
                    settings = JsonSerializer.Deserialize<AppSettingsVM>(json) ?? new AppSettingsVM();
                }
                else
                {
                    settings = new AppSettingsVM();
                }

                var newEntry = new ConfigEntry
                {
                    LastFolder = BrowsePath,
                    StartTime = StartTime,
                    EndTime = EndTime
                };

                // Evitar duplicados
                if (!settings.History.Any(h =>
                    h.LastFolder == newEntry.LastFolder &&
                    h.StartTime == newEntry.StartTime &&
                    h.EndTime == newEntry.EndTime))
                {
                    settings.History.Add(newEntry);

                    // Limitar a 10 entradas de historial
                    if (settings.History.Count > 10)
                        settings.History.RemoveAt(0);
                }

                var newJson = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsFile, newJson);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error guardando configuración: {ex.Message}");
            }
        }

        // Detección de tipo de evento
        private readonly List<string> _deepEvent = new List<string>
        {
            "Previous Local",
            "Power Loss",
            "STEP",
            "Sensor",
            "REMOTE",
            "Save config",
            "CCR",
            "Voltage",
            "Temperature",
            "HoroMeter",
            "Hours",
            "Minutes",
            "Time",
            "Redundancy",
            "Communication",
            "CTS",
            "LFD",
            "JBUS",
            "Startup"
        };

        // Determinar tipo de evento por texto
        private string DetectEventType(string line)
        {
            foreach (var pattern in _deepEvent)
            {
                if (line.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                    return pattern;
            }
            // Si no esta el evento clasificado se coge el tramo entre">>" y ""changed"
            var match = Regex.Match(line, @">>\s*(.*?)\s*changed", RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Groups[1].Value.Trim();

            return "Unknown";
        }

        // Ordenar los eventos del GridData por la seleccion del nombre del evento
        private void UpdateDistinctEvents()
        {
            var allEvents = LogFileEventCounts
                .Where(x => !string.IsNullOrEmpty(x.EventType))
                .Select(x => x.EventType)
                .Distinct()
                .OrderBy(x => x);

            DistinctEvents.Clear();
            foreach (var e in allEvents)
                DistinctEvents.Add(e);

            DistinctEvents.Insert(0, "Todos");
            SelectedEvent = "Todos";
        }

    }
}
