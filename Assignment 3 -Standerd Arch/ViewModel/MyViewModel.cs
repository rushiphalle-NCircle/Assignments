
using Assignment3S.Services;

using Assignment3S.Commands.ViewModel;
using Assignment3S.Services;
using Assignment3S.Commands.Model;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Assignment3S.Commands.ViewModel
{
    public class MyViewModel : INotifyPropertyChanged
    {
        private readonly IRevitService _revit;
        private MyModel _selectedItem;

        public ObservableCollection<MyModel> Items { get; private set; }

        public MyModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (!Equals(_selectedItem, value))
                {
                    _selectedItem = value;
                    OnPropertyChanged("SelectedItem");
                    if (ActivateCommand != null)
                    {
                        // Refresh CanExecute state
                        ActivateCommand.RaiseCanExecuteChanged();
                    }
                }
            }
        }

        public RelayCommand ActivateCommand { get; private set; }
        public RelayCommand CloseCommand { get; private set; }

        // View subscribes to this to close the window (MVVM-friendly)
        public event EventHandler RequestClose;

        public MyViewModel(IRevitService revitService)
        {
            if (revitService == null) throw new ArgumentNullException("revitService");
            _revit = revitService;

            Items = new ObservableCollection<MyModel>();

            ActivateCommand = new RelayCommand(
                execute: _ =>
                {
                    if (SelectedItem == null)
                    {
                        MessageBox.Show("Please select a floor plan.", "No selection",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    var view = _revit.GetViewById(SelectedItem.Id);
                    if (view == null)
                    {
                        MessageBox.Show("Selected view was not found.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    _revit.ActivateView(view);
                    _revit.SelectAllWallsInView(view);

                    // Notify the View to close
                    var handler = RequestClose;
                    if (handler != null) handler(this, EventArgs.Empty);
                },
                canExecute: _ => SelectedItem != null
            );

            CloseCommand = new RelayCommand(_ =>
            {
                var handler = RequestClose;
                if (handler != null) handler(this, EventArgs.Empty);
            });
        }

        public void Load()
        {
            Items.Clear();
            var plans = _revit.GetFloorPlans();
            foreach (var fp in plans)
            {
                Items.Add(fp);
            }

            if (Items.Count > 0)
                SelectedItem = Items[0];
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}

