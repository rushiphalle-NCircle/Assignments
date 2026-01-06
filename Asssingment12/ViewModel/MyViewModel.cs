
// Assignment12/ViewModel/MyViewModel.cs
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Assignment12.Model;
using Assignment12.Commands;

namespace Assignment12.ViewModel
{
    public class MyViewModel : INotifyPropertyChanged
    {
        private readonly Document _doc;
        private readonly ExternalEvent _externalEvent;
        private readonly RevitSelectionEventHandler _handler;

        private ObservableCollection<LevelNode> _levels;
        public ObservableCollection<LevelNode> Levels
        {
            get => _levels;
            set { _levels = value; OnPropertyChanged(); }
        }

        public ICommand RefreshCommand { get; }

        // The View (code-behind) forwards TreeView selection here.
        public object SelectedItem
        {
            get => null;
            set => ProcessSelection(value);
        }

        public MyViewModel(Document doc, ExternalEvent externalEvent, RevitSelectionEventHandler handler)
        {
            _doc = doc;
            _externalEvent = externalEvent;
            _handler = handler;

            Levels = TreeBuilder.Build(_doc);
            RefreshCommand = new RelayCommand(_ => Levels = TreeBuilder.Build(_doc));
        }

        private void ProcessSelection(object node)
        {
            if (node == null) return;

            var req = new SelectionRequest();

            if (node is LevelNode levelNode)
            {
                req.LevelId = levelNode.LevelId;
                req.Category = null;
            }
            else if (node is CategoryCountNode catNode)
            {
                req.LevelId = catNode.LevelId;
                req.Category = catNode.Category;
            }
            else
            {
                return;
            }

            _handler.Request = req;
            _externalEvent.Raise();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    /// <summary>
    /// Minimal RelayCommand (kept in same file to maintain minimal structure).
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly System.Action<object> _execute;
        private readonly System.Predicate<object> _canExecute;
        public RelayCommand(System.Action<object> execute, System.Predicate<object> canExecute = null)
        {
            _execute = execute; _canExecute = canExecute;
        }
        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object parameter) => _execute(parameter);
        public event System.EventHandler CanExecuteChanged { add { } remove { } }
    }
}
