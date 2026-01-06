
// File: DoorParameterModel.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Assignment13.Commands
{
    /// <summary>
    /// Represents a parameter to set (Name + Value) and whether to apply it.
    /// </summary>
    public class DoorParameterModel : INotifyPropertyChanged
    {
        private string _name;
        private string _value;
        private bool _enabled = true;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string Value
        {
            get => _value;
            set { _value = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// If false, this row will be skipped when applying.
        /// </summary>
        public bool Enabled
        {
            get => _enabled;
            set { _enabled = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string prop = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
