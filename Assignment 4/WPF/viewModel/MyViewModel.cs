
using Assignments4.Commands.model;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Assignments4.Commands.viewModel
{
    public class MyViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private List<WallInfo> _items;
        public List<WallInfo> Items
        {
            get => _items;
            set { _items = value ?? new List<WallInfo>(); OnPropertyChanged(); }
        }

        private int _currentIndex;
        public int CurrentIndex
        {
            get => _currentIndex;
            private set
            {
                if (value != _currentIndex)
                {
                    _currentIndex = value;
                    OnPropertyChanged();
                    SelectedWall = Items.Count > 0 ? Items[_currentIndex] : null;
                }
            }
        }

        private WallInfo _selectedWall;
        public WallInfo SelectedWall
        {
            get => _selectedWall;
            set
            {
                _selectedWall = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedWallName));
                // Project Dictionary -> List<KeyValuePair<string,string>> for DataGrid
                SelectedWallProperties = _selectedWall?.WallProperties?
                    .Select(kv => new KeyValuePair<string, string>(kv.Key, kv.Value))
                    .ToList() ?? new List<KeyValuePair<string, string>>();
            }
        }

        private List<KeyValuePair<string, string>> _selectedWallProperties;
        public List<KeyValuePair<string, string>> SelectedWallProperties
        {
            get => _selectedWallProperties;
            private set { _selectedWallProperties = value; OnPropertyChanged(); }
        }

        public string SelectedWallName => SelectedWall?.Name ?? string.Empty;

        public ICommand ShowWallCommand { get; }
        public ICommand NextCommand { get; }
        public ICommand PreviousCommand { get; }

        public MyViewModel(List<WallInfo> items)
        {
            Items = items ?? new List<WallInfo>();
            _currentIndex = 0;
            SelectedWall = Items.FirstOrDefault();

            ShowWallCommand = new RelayCommand(
                execute: param =>
                {
                    // Accept either WallInfo or wall name string
                    if (param is WallInfo wall)
                    {
                        SelectedWall = wall;
                        CurrentIndex = Items.IndexOf(wall);
                    }
                    else if (param is string name)
                    {
                        var found = Items.FirstOrDefault(w => string.Equals(w.Name, name, StringComparison.OrdinalIgnoreCase));
                        if (found != null)
                        {
                            SelectedWall = found;
                            CurrentIndex = Items.IndexOf(found);
                        }
                    }
                },
                canExecute: _ => Items.Count > 0
            );

            NextCommand = new RelayCommand(
                execute: _ =>
                {
                    if (Items.Count == 0) return;
                    CurrentIndex = (CurrentIndex + 1 < Items.Count) ? CurrentIndex + 1 : CurrentIndex;
                },
                canExecute: _ => Items.Count > 0 && CurrentIndex < Items.Count - 1
            );

            PreviousCommand = new RelayCommand(
                execute: _ =>
                {
                    if (Items.Count == 0) return;
                    CurrentIndex = (CurrentIndex - 1 >= 0) ? CurrentIndex - 1 : CurrentIndex;
                },
                canExecute: _ => Items.Count > 0 && CurrentIndex > 0
            );
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // Simple RelayCommand implementation
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;
        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object parameter) => _execute(parameter);

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}


