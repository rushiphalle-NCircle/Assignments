
// Assignment11/ViewModel/MyViewModel.cs
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Assignment11.Model;

namespace Assignment11.ViewModel
{
    public class MyViewModel : INotifyPropertyChanged
    {
        private readonly Document _doc;

        private ObservableCollection<LevelNode> _levels;
        public ObservableCollection<LevelNode> Levels
        {
            get => _levels;
            set { _levels = value; OnPropertyChanged(); }
        }

        public ICommand RefreshCommand { get; }

        public MyViewModel(Document doc)
        {
            _doc = doc;
            Levels = TreeBuilder.Build(_doc);
            RefreshCommand = new RelayCommand(_ => Levels = TreeBuilder.Build(_doc));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    /// <summary>
    /// Minimal RelayCommand implementation.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly System.Action<object> _execute;
        private readonly System.Predicate<object> _canExecute;
        public RelayCommand(System.Action<object> execute, System.Predicate<object> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }
        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object parameter) => _execute(parameter);
        public event System.EventHandler CanExecuteChanged { add { } remove { } }
    }
}
