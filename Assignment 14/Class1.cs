
using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace Assignment14.Commands
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class CreateViewsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // === Configure names/templates here ===
            string configuredLevelName = "Level 1";          // optional; will fallback if not found
            string floorPlanTemplateName = "T-FloorPlan";    // optional
            string ceilingPlanTemplateName = "T-Ceiling";    // optional
            string view3DTemplateName = "T-3D-Isometric";    // optional

            string floorPlanNamePattern = "{0} - Floor";
            string ceilingPlanNamePattern = "{0} - Ceiling";
            string view3DName = "3D - New Isometric";

            try
            {
                // Load templates (if they exist)
                View floorTpl = GetTemplateByName(doc, floorPlanTemplateName);
                View ceilingTpl = GetTemplateByName(doc, ceilingPlanTemplateName);
                View v3dTpl = GetTemplateByName(doc, view3DTemplateName);

                // Try configured name first
                Level level = GetLevelByName(doc, configuredLevelName);

                if (level == null)
                {
                    // Offer user choices
                    TaskDialog td = new TaskDialog("Level not found");
                    td.MainInstruction = $"Level '{configuredLevelName}' was not found.";
                    td.MainContent = "Choose how to proceed:";
                    td.CommonButtons = TaskDialogCommonButtons.Cancel;
                    td.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Use active view's level (if this is a plan)");
                    td.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Pick a Level");
                    td.AddCommandLink(TaskDialogCommandLinkId.CommandLink3, "Create for ALL Levels");
                    TaskDialogResult tdr = td.Show();

                    if (tdr == TaskDialogResult.CommandLink1)
                    {
                        Level avLevel = GetActiveViewLevel(doc.ActiveView);
                        if (avLevel == null)
                        {
                            message = "Active view has no associated level. Open a Floor/Ceiling Plan and try again.";
                            return Result.Failed;
                        }
                        level = avLevel;

                        using (TransactionGroup tg = new TransactionGroup(doc, $"Create Views - {level.Name}"))
                        {
                            tg.Start();
                            CreateViewsForLevel(doc, level, floorTpl, ceilingTpl, v3dTpl,
                                floorPlanNamePattern, ceilingPlanNamePattern, view3DName);
                            tg.Assimilate();
                        }
                        return Result.Succeeded;
                    }
                    else if (tdr == TaskDialogResult.CommandLink2)
                    {
                        // Pick a Level
                        Reference picked = uidoc.Selection.PickObject(ObjectType.Element, new LevelSelectionFilter(), "Select a Level");
                        level = doc.GetElement(picked.ElementId) as Level;
                        if (level == null)
                        {
                            message = "No valid Level selected.";
                            return Result.Failed;
                        }

                        using (TransactionGroup tg = new TransactionGroup(doc, $"Create Views - {level.Name}"))
                        {
                            tg.Start();
                            CreateViewsForLevel(doc, level, floorTpl, ceilingTpl, v3dTpl,
                                floorPlanNamePattern, ceilingPlanNamePattern, view3DName);
                            tg.Assimilate();
                        }
                        return Result.Succeeded;
                    }
                    else if (tdr == TaskDialogResult.CommandLink3)
                    {
                        // All levels
                        var allLevels = new FilteredElementCollector(doc)
                            .OfClass(typeof(Level)).Cast<Level>()
                            .OrderBy(l => l.Elevation).ToList();

                        if (!allLevels.Any())
                        {
                            message = "No levels found in the model.";
                            return Result.Failed;
                        }

                        using (TransactionGroup tg = new TransactionGroup(doc, "Create Views - All Levels"))
                        {
                            tg.Start();
                            foreach (var lv in allLevels)
                            {
                                CreateViewsForLevel(doc, lv, floorTpl, ceilingTpl, v3dTpl,
                                    floorPlanNamePattern, ceilingPlanNamePattern, view3DName);
                            }
                            tg.Assimilate();
                        }
                        return Result.Succeeded;
                    }
                    else
                    {
                        message = "Command cancelled.";
                        return Result.Cancelled;
                    }
                }
                else
                {
                    // Configured level was found
                    using (TransactionGroup tg = new TransactionGroup(doc, $"Create Views - {level.Name}"))
                    {
                        tg.Start();
                        CreateViewsForLevel(doc, level, floorTpl, ceilingTpl, v3dTpl,
                            floorPlanNamePattern, ceilingPlanNamePattern, view3DName);
                        tg.Assimilate();
                    }
                    return Result.Succeeded;
                }
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                // User cancelled a pick
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }

        // ---------- Helpers ----------

        private static void CreateViewsForLevel(
            Document doc,
            Level level,
            View floorTpl,
            View ceilingTpl,
            View v3dTpl,
            string floorPlanNamePattern,
            string ceilingPlanNamePattern,
            string view3DName)
        {
            // Floor Plan
            using (Transaction tx = new Transaction(doc, $"Create Floor Plan - {level.Name}"))
            {
                tx.Start();
                var planType = GetViewFamilyType(doc, ViewFamily.FloorPlan)
                    ?? throw new InvalidOperationException("No ViewFamilyType for FloorPlan found.");
                string floorViewName = EnsureUniqueViewName(doc, string.Format(floorPlanNamePattern, level.Name));
                ViewPlan floorView = ViewPlan.Create(doc, planType.Id, level.Id);
                floorView.Name = floorViewName;
                if (floorTpl != null) floorView.ViewTemplateId = floorTpl.Id;
                tx.Commit();
            }

            // Ceiling Plan
            using (Transaction tx = new Transaction(doc, $"Create Ceiling Plan - {level.Name}"))
            {
                tx.Start();
                var ceilingType = GetViewFamilyType(doc, ViewFamily.CeilingPlan)
                    ?? throw new InvalidOperationException("No ViewFamilyType for CeilingPlan found.");
                string ceilingViewName = EnsureUniqueViewName(doc, string.Format(ceilingPlanNamePattern, level.Name));
                ViewPlan ceilingView = ViewPlan.Create(doc, ceilingType.Id, level.Id);
                ceilingView.Name = ceilingViewName;
                if (ceilingTpl != null) ceilingView.ViewTemplateId = ceilingTpl.Id;
                tx.Commit();
            }

            // 3D Isometric (one per run; if you want one per level, adjust naming accordingly)
            using (Transaction tx = new Transaction(doc, $"Create 3D View"))
            {
                tx.Start();
                var v3dType = GetViewFamilyType(doc, ViewFamily.ThreeDimensional)
                    ?? throw new InvalidOperationException("No ViewFamilyType for ThreeDimensional found.");
                string unique3DName = EnsureUniqueViewName(doc, view3DName);
                View3D v3d = View3D.CreateIsometric(doc, v3dType.Id);
                v3d.Name = unique3DName;
                if (v3dTpl != null) v3d.ViewTemplateId = v3dTpl.Id;
                tx.Commit();
            }
        }

        private static ViewFamilyType GetViewFamilyType(Document doc, ViewFamily family)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>()
                .FirstOrDefault(vft => vft.ViewFamily == family);
        }

        private static View GetTemplateByName(Document doc, string templateName)
        {
            if (string.IsNullOrWhiteSpace(templateName)) return null;
            return new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .FirstOrDefault(v => v.IsTemplate && v.Name.Equals(templateName, StringComparison.OrdinalIgnoreCase));
        }

        private static Level GetLevelByName(Document doc, string levelName)
        {
            if (string.IsNullOrWhiteSpace(levelName)) return null;
            return new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .FirstOrDefault(l => l.Name.Equals(levelName, StringComparison.OrdinalIgnoreCase));
        }

        private static Level GetActiveViewLevel(View activeView)
        {
            if (activeView is ViewPlan vp) return vp.GenLevel;
            return null;
        }

        private static string EnsureUniqueViewName(Document doc, string desired)
        {
            int suffix = 2;
            string candidate = desired;
            while (ViewNameExists(doc, candidate))
            {
                candidate = $"{desired} ({suffix})";
                suffix++;
            }
            return candidate;
        }

        private static bool ViewNameExists(Document doc, string name)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Any(v => !v.IsTemplate && v.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        // Filter only Level elements for selection
        class LevelSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem) => elem is Level;
            public bool AllowReference(Reference reference, XYZ position) => false;
        }
    }
}
