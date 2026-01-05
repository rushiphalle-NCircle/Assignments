
// File: ViewModels.cs
using Assignment6.Commands.Model;
using Autodesk.Revit.UI;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows; // for Application.Current.Dispatcher

namespace Assignment6.Commands.ViewModel
{
    public class MyViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<FloorModel> Floors { get; private set; }
        public ObservableCollection<RoomModel> Rooms { get; private set; }

        private FloorModel selectedFloor;
        private RoomModel selectedRoom;

        private readonly ExternalEvent _event;
        private readonly Assignment6.Commands.MyExternalCommand _handler;

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand ActivateCommand { get; }

        public FloorModel SelectedFloor
        {
            get => selectedFloor;
            set
            {
                selectedFloor = value;
                if (selectedFloor != null)
                {
                    _handler.TargetFloorViewId = selectedFloor.Id;
                    _event.Raise(); // trigger rooms retrieval
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedFloor)));
            }
        }

        public RoomModel SelectedRoom
        {
            get => selectedRoom;
            set
            {
                selectedRoom = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedRoom)));
            }
        }

        public MyViewModel(IEnumerable<FloorModel> views, ExternalEvent externalEvent, Assignment6.Commands.MyExternalCommand handler)
        {
            Floors = new ObservableCollection<FloorModel>(views);
            Rooms = new ObservableCollection<RoomModel>(); // initialize to avoid null bindings
            _event = externalEvent;
            _handler = handler;

            // Update Rooms on WPF UI thread
            _handler.triggerUpdate += rooms =>
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    Rooms.Clear();
                    foreach (var r in rooms ?? Enumerable.Empty<RoomModel>())
                        Rooms.Add(r);

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Rooms)));
                });
            };

            ActivateCommand = new RelayCommand(_ => HandleActivate(), _ => CanActivate());
        }

        private bool CanActivate()
        {
            return selectedFloor != null && selectedRoom != null;
        }

        private void HandleActivate()
        {
            var handler = new Assignment6.Commands.MyExternalCommand1
            {
                TargetRoomId = selectedRoom.Id,
                TargetFloorViewId = selectedFloor.Id
            };

            var externalEvent = ExternalEvent.Create(handler);
            externalEvent.Raise();
        }
    }

    public class MyViewModel2
    {
        public ObservableCollection<Walls> Items { get; set; }
        public MyViewModel2(IEnumerable<Walls> items)
        {
            Items = new ObservableCollection<Walls>(items);
        }
    }

    // Simple RelayCommand
    public class RelayCommand : ICommand
    {
        private readonly System.Action<object> _execute;
        private readonly System.Predicate<object> _canExecute;

        public RelayCommand(System.Action<object> execute, System.Predicate<object> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);

        public event System.EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
