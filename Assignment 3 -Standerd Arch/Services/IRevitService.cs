
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Assignment3S.Commands.Model;

namespace Assignment3S.Services
{
    public interface IRevitService
    {
        IList<MyModel> GetFloorPlans();
        View GetViewById(int id);
        void ActivateView(View view);
        void SelectAllWallsInView(View view);
    }
}
