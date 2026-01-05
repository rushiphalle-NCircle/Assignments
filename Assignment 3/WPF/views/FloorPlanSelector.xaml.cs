
using Assignment3.Commands.Model;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace Assignment3.Commands
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

            MyComboBox.ItemsSource = _items;

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

            
            _uiDoc.RequestViewChange(view);

            
            var wallIds = new FilteredElementCollector(_uiDoc.Document, view.Id)
                .OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsNotElementType()
                .ToElementIds();
            _uiDoc.Selection.SetElementIds(wallIds);


            this.DialogResult = true;
            this.Close();
        }
    }
}
