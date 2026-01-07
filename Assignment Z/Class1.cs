
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using System;

namespace A
{
    [Transaction(TransactionMode.Manual)]
    public class AlignPipeEndpoints : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                // 1) Pick two pipes
                Reference r1 = uidoc.Selection.PickObject(
                    ObjectType.Element, new PipeFilter(),
                    "Select FIRST pipe (reference axis & length)");

                Reference r2 = uidoc.Selection.PickObject(
                    ObjectType.Element, new PipeFilter(),
                    "Select SECOND pipe (to stay parallel and match length)");

                Pipe pipe1 = doc.GetElement(r1) as Pipe;
                Pipe pipe2 = doc.GetElement(r2) as Pipe;

                if (pipe1 == null || pipe2 == null)
                {
                    message = "Please select valid Pipe elements.";
                    return Result.Failed;
                }
                if (pipe1.Id == pipe2.Id)
                {
                    message = "Select two different pipes.";
                    return Result.Failed;
                }

                // 2) Get location curves (straight pipes only for this simple version)
                LocationCurve lc1 = pipe1.Location as LocationCurve;
                LocationCurve lc2 = pipe2.Location as LocationCurve;
                if (lc1 == null || lc2 == null)
                {
                    message = "Selected elements must have a LocationCurve.";
                    return Result.Failed;
                }

                Line l1 = lc1.Curve as Line;
                Line l2 = lc2.Curve as Line;
                if (l1 == null || l2 == null)
                {
                    message = "This simple tool supports only straight pipes.";
                    return Result.Failed;
                }

                // 3) Pipe 1 axis / direction / length
                XYZ s1 = l1.GetEndPoint(0);
                XYZ e1 = l1.GetEndPoint(1);
                XYZ v1 = (e1 - s1).Normalize();
                double len1 = s1.DistanceTo(e1);

                // 4) Pipe 2 start point
                XYZ s2 = l2.GetEndPoint(0);

                // 5) Compute perpendicular offset from Pipe 2 start to Pipe 1's infinite axis
                XYZ s2ProjOnL1 = ProjectPointOnLine(s2, l1);
                XYZ offset = s2 - s2ProjOnL1; // preserves side-by-side spacing (no overlap)

                // 6) New Pipe 2 endpoints: start aligned to Pipe 1 axis + preserved offset, same length & direction
                XYZ newStart2 = s1 + offset;
                XYZ newEnd2 = newStart2 + v1 * len1;

                // 7) Apply the new curve to Pipe 2
                using (Transaction tx = new Transaction(doc, "Align parallel, keep offset, match length"))
                {
                    tx.Start();
                    lc2.Curve = Line.CreateBound(newStart2, newEnd2);
                    tx.Commit();
                }

                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (System.Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }

        /// <summary>
        /// Project a point onto an infinite line (defined by Line's endpoints).
        /// </summary>
        private static XYZ ProjectPointOnLine(XYZ point, Line line)
        {
            XYZ a = line.GetEndPoint(0);
            XYZ b = line.GetEndPoint(1);
            XYZ v = (b - a).Normalize();
            double t = v.DotProduct(point - a);
            return a + v.Multiply(t);
        }
    }

    /// Selection filter so user can only pick Pipe elements
    class PipeFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem) => elem is Pipe;
        public bool AllowReference(Reference reference, XYZ position) => true;
    }
}
