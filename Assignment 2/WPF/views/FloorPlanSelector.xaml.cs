
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Assignment2.Commands.Model;

namespace Assignment2.Commands
{
    public partial class FloorPlanSelector : Window
    {
        private readonly UIDocument _uiDoc;
        private readonly IList<MyModel> _items;

        public FloorPlanSelector(UIDocument uiDoc, List<MyModel> items)
        {
            InitializeComponent();
            _uiDoc = uiDoc;
            _items = items ?? new List<MyModel>();

            // Bind the ComboBox directly to the incoming list
            MyComboBox.ItemsSource = _items;

            // Optional: preselect first item
            if (_items.Count > 0)
                MyComboBox.SelectedIndex = 0;
        }

        private void Activate_Click(object sender, RoutedEventArgs e)
        {
            var selected = MyComboBox.SelectedItem as MyModel;
            if (selected == null)
            {
                MessageBox.Show("Please select a floor plan.", "No selection",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var viewId = new ElementId(selected.Id);
            var view = _uiDoc.Document.GetElement(viewId) as View;
            if (view == null)
            {
                MessageBox.Show("Selected view was not found.", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Ask Revit to switch to that view
            _uiDoc.RequestViewChange(view);



            // Close the dialog after activation (optional)
            this.DialogResult = true;
            this.Close();
        }
    }
}
