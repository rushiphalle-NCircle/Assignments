
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using FamilyLoader.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel; // <-- add this
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace FamilyLoader.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged // <-- implement
    {
        private readonly Document _doc;

        private string _folderPath;
        private string _statusText = "Select a folder containing .rfa files.";

        public string FolderPath
        {
            get => _folderPath;
            set
            {
                if (_folderPath != value)
                {
                    _folderPath = value;
                    OnPropertyChanged(nameof(FolderPath));
                    // Re-evaluate CanExecute for LoadCommand whenever the path changes
                    (LoadCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string StatusText
        {
            get => _statusText;
            private set
            {
                if (_statusText != value)
                {
                    _statusText = value;
                    OnPropertyChanged(nameof(StatusText));
                }
            }
        }

        public ICommand BrowseCommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand CloseCommand { get; }

        public Action RequestClose { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public MainViewModel(Document doc)
        {
            _doc = doc ?? throw new ArgumentNullException(nameof(doc));
            BrowseCommand = new RelayCommand(BrowseFolder);
            LoadCommand = new RelayCommand(LoadFamilies, CanLoadFamilies);
            CloseCommand = new RelayCommand(() => RequestClose?.Invoke());
        }

        private void BrowseFolder()
        {
            // Revit TaskDialog (message-only) — leave as-is, but DO NOT rely on it to set the path
            TaskDialog td = new TaskDialog("Select Folder");
            td.MainInstruction = "Paste the folder path into the textbox.";
            td.MainContent =
                "Revit does not provide a built-in folder picker in TaskDialog.\n" +
                "Copy the full path to the folder containing .rfa files and paste it\n" +
                "into the FolderPath textbox, then click 'Load Families'.";
            td.CommonButtons = TaskDialogCommonButtons.Close;
            td.Show();
        }

        private bool CanLoadFamilies()
        {
            return !string.IsNullOrWhiteSpace(FolderPath) && Directory.Exists(FolderPath);
        }

        private void LoadFamilies()
        {
            var request = new FamilyLoadRequest { FolderPath = FolderPath, Recursive = true };
            var rfaPaths = GetRfaFiles(request);
            if (!rfaPaths.Any())
            {
                SetStatus("No .rfa files found.");
                return;
            }

            int loaded = 0, failed = 0;
            var errors = new List<string>();
            var opts = new SimpleFamilyLoadOptions();

            try
            {
                using (var t = new Transaction(_doc, "Load Families"))
                {
                    t.Start();

                    foreach (var path in rfaPaths)
                    {
                        try
                        {
                            Family fam;
                            bool ok = _doc.LoadFamily(path, opts, out fam);
                            if (ok && fam != null)
                                loaded++;
                            else
                            {
                                failed++;
                                errors.Add($"Failed: {Path.GetFileName(path)}");
                            }
                        }
                        catch (Exception ex)
                        {
                            failed++;
                            errors.Add($"{Path.GetFileName(path)}: {ex.Message}");
                        }
                    }

                    t.Commit();
                }
            }
            catch (Exception ex)
            {
                SetStatus("Transaction error: " + ex.Message);
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Loaded: {loaded}");
            sb.AppendLine($"Failed: {failed}");
            if (errors.Count > 0)
            {
                sb.AppendLine("Errors:");
                foreach (var e in errors.Take(10))
                    sb.AppendLine(" - " + e);
                if (errors.Count > 10)
                    sb.AppendLine($" ...and {errors.Count - 10} more.");
            }

            SetStatus(sb.ToString());
            TaskDialog.Show("Family Loader", $"Completed.\nLoaded: {loaded}\nFailed: {failed}");
        }

        private IEnumerable<string> GetRfaFiles(FamilyLoadRequest request)
        {
            var option = request.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            try
            {
                return Directory.EnumerateFiles(request.FolderPath, "*.rfa", option);
            }
            catch
            {
                return Enumerable.Empty<string>();
            }
        }

        private void SetStatus(string msg)
        {
            StatusText = msg; // now notifies the view
        }
    }
}
