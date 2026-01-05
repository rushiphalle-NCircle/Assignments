
// File: MyExternalCommands.cs
using Assignment6.Commands.Model;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Assignment6.Commands
{
    public class MyExternalCommand : IExternalEventHandler
    {
        // Floor Plan (ViewPlan) Id
        public ElementId TargetFloorViewId { get; set; }

        public event Action<IEnumerable<RoomModel>> triggerUpdate;

        public void Execute(UIApplication app)
        {
            try
            {
                UIDocument uiDoc = app.ActiveUIDocument;
                Document doc = uiDoc.Document;

                var viewPlan = doc.GetElement(TargetFloorViewId) as ViewPlan;
                if (viewPlan == null)
                {
                    triggerUpdate?.Invoke(new[]
                    {
                        new RoomModel { Id = ElementId.InvalidElementId, Name = "Invalid floor plan view" }
                    });
                    return;
                }

                // First: rooms visible in this view (respects view filters/phases/design options)
                var roomsInView = new FilteredElementCollector(doc, viewPlan.Id)
                                    .OfCategory(BuiltInCategory.OST_Rooms)
                                    .WhereElementIsNotElementType()
                                    .Cast<Room>()
                                    .Select(r => new RoomModel
                                    {
                                        Id = r.Id,
                                        Name = string.IsNullOrWhiteSpace(r.Name) ? $"Room {r.Number}" : r.Name
                                    })
                                    .ToList();

                if (roomsInView.Count > 0)
                {
                    triggerUpdate?.Invoke(roomsInView);
                    return;
                }

                // Fallback: rooms by the plan's level (in case view filters hide rooms)
                ElementId levelId = viewPlan.GenLevel?.Id;
                var roomsByLevel = (levelId != null && levelId != ElementId.InvalidElementId)
                    ? new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_Rooms)
                        .WhereElementIsNotElementType()
                        .Cast<Room>()
                        .Where(r => r.LevelId == levelId)
                        .Select(r => new RoomModel
                        {
                            Id = r.Id,
                            Name = string.IsNullOrWhiteSpace(r.Name) ? $"Room {r.Number}" : r.Name
                        })
                        .ToList()
                    : new List<RoomModel>();

                if (roomsByLevel.Count > 0)
                {
                    triggerUpdate?.Invoke(roomsByLevel);
                }
                else
                {
                    triggerUpdate?.Invoke(new[]
                    {
                        new RoomModel { Id = ElementId.InvalidElementId, Name = "No rooms visible / found for this plan" }
                    });
                }
            }
            catch (Exception ex)
            {
                triggerUpdate?.Invoke(new[]
                {
                    new RoomModel { Id = ElementId.InvalidElementId, Name = $"Error: {ex.Message}" }
                });
            }
        }

        public string GetName() => "MYEVENT_ROOMS";
    }

    public class MyExternalCommand1 : IExternalEventHandler
    {
        public ElementId TargetFloorViewId { get; set; }
        public ElementId TargetRoomId { get; set; }

        public void Execute(UIApplication app)
        {
            UIDocument uiDoc = app.ActiveUIDocument;
            Document doc = uiDoc.Document;

            var room = doc.GetElement(TargetRoomId) as Room;
            if (room == null)
            {
                TaskDialog.Show("Walls", "Selected room not found.");
                return;
            }

            var opts = new SpatialElementBoundaryOptions
            {
                SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Finish
            };

            var loops = room.GetBoundarySegments(opts);
            var wallIds = new HashSet<ElementId>();

            if (loops != null)
            {
                foreach (IList<BoundarySegment> loop in loops)
                {
                    foreach (var seg in loop)
                    {
                        var elem = doc.GetElement(seg.ElementId);
                        if (elem is Wall w)
                            wallIds.Add(w.Id);
                        // NOTE: boundaries can include non-wall elements (room separation lines, columns, etc.)
                    }
                }
            }

            var wallModels = wallIds
                                .Select(id => doc.GetElement(id) as Wall)
                                .Where(w => w != null)
                                .Select(w => new Walls { Id = w.Id, Name = w.Name })
                                .ToList();

            if (wallModels.Count == 0)
                wallModels.Add(new Walls { Id = ElementId.InvalidElementId, Name = "No bounding walls found" });

            var window = new Assignment6.Commands.Views.MyView2(
                new Assignment6.Commands.ViewModel.MyViewModel2(wallModels)
            );

            window.ShowDialog();
        }

        public string GetName() => "MYEVENT_WALLS";
    }
}
