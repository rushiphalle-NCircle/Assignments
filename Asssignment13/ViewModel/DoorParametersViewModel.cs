
// File: DoorParametersViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Assignment13.Commands
{
    /// <summary>
    /// VM for the door parameters window. Performs the Revit transaction on Apply.
    /// </summary>
    public class DoorParametersViewModel
    {
        private readonly UIDocument _uidoc;
        private readonly Document _doc;
        private readonly ElementId[] _doorIds;

        public ObservableCollection<DoorParameterModel> Parameters { get; } = new ObservableCollection<DoorParameterModel>();

        public int DoorCount => _doorIds?.Length ?? 0;

        public bool ApplyOnlyNonEmpty { get; set; } = true;

        public ICommand AddRowCommand { get; }
        public ICommand RemoveSelectedLastRowCommand { get; }
        public ICommand ApplyCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action CloseRequested;
        public event Action<string> ShowInfoMessage;
        public event Action<string> ShowWarningMessage;
        public event Action<string> ShowErrorMessage;

        public DoorParametersViewModel(UIDocument uidoc, ElementId[] doorIds)
        {
            _uidoc = uidoc ?? throw new ArgumentNullException(nameof(uidoc));
            _doc = uidoc.Document ?? throw new ArgumentNullException(nameof(uidoc));
            _doorIds = doorIds ?? Array.Empty<ElementId>();

            // Provide three common defaults. You can add more rows from UI.
            Parameters.Add(new DoorParameterModel { Name = "Mark", Value = "", Enabled = true });
            Parameters.Add(new DoorParameterModel { Name = "Comments", Value = "", Enabled = true });
            Parameters.Add(new DoorParameterModel { Name = "Fire Rating", Value = "", Enabled = true });

            AddRowCommand = new RelayCommand(_ => Parameters.Add(new DoorParameterModel { Enabled = true }), _ => true);
            RemoveSelectedLastRowCommand = new RelayCommand(_ =>
            {
                if (Parameters.Count > 0) Parameters.RemoveAt(Parameters.Count - 1);
            }, _ => Parameters.Count > 0);

            ApplyCommand = new RelayCommand(_ => Apply(), _ => DoorCount > 0);
            CancelCommand = new RelayCommand(_ => CloseRequested?.Invoke(), _ => true);
        }

        private void Apply()
        {
            if (_doorIds == null || _doorIds.Length == 0)
            {
                ShowWarningMessage?.Invoke("No doors selected.");
                return;
            }

            var rowsToApply = Parameters
                .Where(p => p.Enabled)
                .Where(p => !ApplyOnlyNonEmpty || !string.IsNullOrWhiteSpace(p.Value))
                .ToArray();

            if (rowsToApply.Length == 0)
            {
                ShowWarningMessage?.Invoke("Nothing to apply. Enable rows and/or enter values.");
                return;
            }

            int successCount = 0;
            int skippedCount = 0;
            int errorCount = 0;

            using (var trans = new Transaction(_doc, "Set door parameters"))
            {
                var status = trans.Start();
                if (status != TransactionStatus.Started)
                {
                    ShowErrorMessage?.Invoke("Failed to start transaction.");
                    return;
                }

                try
                {
                    foreach (var id in _doorIds)
                    {
                        var el = _doc.GetElement(id);
                        if (el == null)
                        {
                            skippedCount++;
                            continue;
                        }

                        foreach (var row in rowsToApply)
                        {
                            if (string.IsNullOrWhiteSpace(row.Name))
                            {
                                skippedCount++;
                                continue;
                            }

                            var param = el.LookupParameter(row.Name);
                            if (param == null || param.IsReadOnly)
                            {
                                skippedCount++;
                                continue;
                            }

                            bool ok = SetParameterValue(_doc, param, row.Value);
                            if (ok) successCount++; else errorCount++;
                        }
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    errorCount++;
                    try { trans.RollBack(); } catch { /* ignore */ }
                    ShowErrorMessage?.Invoke("Error applying parameters:\n" + ex.Message);
                    return;
                }
            }

            ShowInfoMessage?.Invoke($"Applied: {successCount}, Skipped: {skippedCount}, Errors: {errorCount}");
            CloseRequested?.Invoke();
        }

        /// <summary>
        /// Sets the parameter value, handling string, integer, double (with unit conversion).
        /// Doubles are interpreted in the project's display units for that parameter spec.
        /// </summary>
        private static bool SetParameterValue(Document doc, Parameter param, string input)
        {
            try
            {
                switch (param.StorageType)
                {
                    case StorageType.String:
                        param.Set(input ?? string.Empty);
                        return true;

                    case StorageType.Integer:
                        {
                            if (string.IsNullOrWhiteSpace(input))
                            {
                                // If empty and integer, set to 0
                                param.Set(0);
                                return true;
                            }

                            if (int.TryParse(input, NumberStyles.Integer, CultureInfo.CurrentCulture, out int i))
                            {
                                param.Set(i);
                                return true;
                            }
                            return false;
                        }

                    case StorageType.Double:
                        {
                            if (string.IsNullOrWhiteSpace(input))
                            {
                                // If empty and double, set to 0.0
                                param.Set(0.0);
                                return true;
                            }

                            // Parse using current culture; user may input in local format
                            if (!double.TryParse(input, NumberStyles.Float, CultureInfo.CurrentCulture, out double val))
                                return false;

#if REVIT2021 || REVIT2022 || REVIT2023 || REVIT2024 || REVIT2025
                            // New Units API: get the parameter spec (ForgeTypeId), use project's FormatOptions to find the UnitTypeId
                            ForgeTypeId spec = param.Definition.GetDataType();
                            Units units = doc.GetUnits();
                            FormatOptions fo = units.GetFormatOptions(spec);
                            ForgeTypeId unitTypeId = fo?.GetUnitTypeId();

                            if (unitTypeId != null)
                            {
                                double internalVal = UnitUtils.ConvertToInternalUnits(val, unitTypeId);
                                param.Set(internalVal);
                                return true;
                            }
                            else
                            {
                                // Fallback: assume value already in internal units
                                param.Set(val);
                                return true;
                            }
#else
                            // Legacy Units API fallback: assume input is already internal units (feet for lengths)
                            param.Set(val);
                            return true;
#endif
                        }

                    case StorageType.ElementId:
                        {
                            if (string.IsNullOrWhiteSpace(input))
                            {
                                param.Set(ElementId.InvalidElementId);
                                return true;
                            }

                            // Try to resolve by Id; otherwise leave unchanged
                            if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out int rawId))
                            {
                                var eid = new ElementId(rawId);
                                param.Set(eid);
                                return true;
                            }
                            return false;
                        }

                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Simple ICommand implementation.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute ?? (_ => true);
        }
        public bool CanExecute(object parameter) => _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);
        public event EventHandler CanExecuteChanged;
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}