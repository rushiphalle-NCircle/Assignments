using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;

using System.Collections.ObjectModel;
using System.Windows.Input;
using Assignment5A.Commands.Model;


namespace Assignment5A.Commands.ViewModel
{
    public class MyViewModel
    {
        public ObservableCollection<ViewItem> Views { get; }

        public ViewItem SelectedView { get; set; }

        public ICommand ActivateCommand { get; }

        private readonly ExternalEvent _externalEvent;
        private readonly ChangeViewHandler _handler;

        public event Action closeEvent;

        public MyViewModel(IEnumerable<ViewItem> views, ExternalEvent externalEvent, ChangeViewHandler handler)
        {
            Views = new ObservableCollection<ViewItem>(views);
            _externalEvent = externalEvent;
            _handler = handler;

            ActivateCommand = new RelayCommand(OnActivate);
        }

        private void OnActivate()
        {
            if (SelectedView == null)
                return;

            _handler.TargetViewId = SelectedView.ViewId;
            _externalEvent.Raise();
            closeEvent?.Invoke();
        }
    }

        public class RelayCommand : ICommand
        {
            private readonly Action _execute;
            private readonly Func<bool> _canExecute;

            public RelayCommand(Action execute, Func<bool> canExecute = null)
            {
                _execute = execute
                    ?? throw new ArgumentNullException(nameof(execute));

                _canExecute = canExecute;
            }

            public bool CanExecute(object parameter)
            {
                return _canExecute == null || _canExecute();
            }

            public void Execute(object parameter)
            {
                _execute();
            }

            public event EventHandler CanExecuteChanged;

            /// <summary>
            /// Call this when conditions affecting CanExecute change
            /// </summary>
            public void RaiseCanExecuteChanged()
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    
}
