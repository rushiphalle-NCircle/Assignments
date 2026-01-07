using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace Assignments
{
    public class Application : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            Ribbon myRibbon = new Ribbon(application);

            //Creating Panel 1
            myRibbon.CreatePanel("Assignment 1");
            myRibbon.CreateButton("Assignment 1", "View Levels", "Asignment1.Commands.LevelCommand", "1.jpeg");

            //Adding View Levels Button
            myRibbon.CreatePanel("Assignment 2");
            myRibbon.CreateButton("Assignment 2", "View Floors", "Assignments2.Commands.ActiveView", "2.jpeg");

            myRibbon.CreatePanel("Assignment 3");
            myRibbon.CreateButton("Assignment 3", "View Walls", "Assignments3.Commands.ActiveView", "3.jpeg");
            myRibbon.CreateButton("Assignment 3", "MVVM", "Assignment3S.Commands.ActiveView", "3A.jpeg");

            myRibbon.CreatePanel("Assignment 4");
            myRibbon.CreateButton("Assignment 4", "Get Walls\n Info", "Assignments4.Commands.WallInfoFinder", "4.jpeg", "Assignments4.Commands.AVClass.MyAvailabilityClass");

            myRibbon.CreatePanel("Assignment 5");
            myRibbon.CreatePDButton("Assignment 5", "Activate Views", "5.jpeg", new List<(string name, string fullclassname)>() { ("Floor Plan", "Assignment5A.Commands.MyCommand"), ("Ceiling Plan", "Assignments5.Commands.CeilingPlan") });
            myRibbon.CreateButton("Assignment 5", "View Rooms", "Assignment5B.Commands.MyCommand", "5.jpeg");

            myRibbon.CreatePanel("Assignment 6");
            myRibbon.CreateButton("Assignment 6", "Seelct Room \n And View Walls", "Assignment6.Commands.MyCommand", "3.jpeg");

            myRibbon.CreatePanel("Assignment 7");
            myRibbon.CreateButton("Assignment 7", "Show Hirerchy", "Assignment7.Commands.Command", "3.jpeg");

            myRibbon.CreatePanel("Assignment 8");
            myRibbon.CreateButton("Assignment 8", "Show Hirerchy \nand Redirect", "Assignment8.Commands.Command", "3.jpeg");

            myRibbon.CreatePanel("Assignment 9");
            myRibbon.CreateButton("Assignment 9", "Show All Walls", "Assignment9.Commands.MyCommand", "3.jpeg");

            myRibbon.CreatePanel("Assignment 10");
            myRibbon.CreateButton("Assignment 10", "Show Openings", "Assignment10.Commands.GetWallOpeningsCommand", "3.jpeg");

            myRibbon.CreatePanel("Assignment 11");
            myRibbon.CreateButton("Assignment 11", "Show Openings", "Assignment11.Commands.MyExternalCommandWpf", "3.jpeg");

            myRibbon.CreatePanel("Assignment 12");
            myRibbon.CreateButton("Assignment 12", "Show Hirerchy", "Assignment12.Commands.MyExternalCommandWpf", "3.jpeg");

            myRibbon.CreatePanel("Assignment 13");
            myRibbon.CreateButton("Assignment 13", "Show Hirerchy", "Assignment13.Commands.SetDoorParametersCommand", "3.jpeg");

            myRibbon.CreatePanel("Assignment 14");
            myRibbon.CreateButton("Assignment 14", "Create new \n View", "Assignment14.Commands.CreateViewsCommand", "3.jpeg");

            myRibbon.CreatePanel("Assignment 15");
            myRibbon.CreateButton("Assignment 15", "Get Folders", "FamilyLoader.Commands.LoadFamiliesCommand", "3.jpeg");

                myRibbon.CreatePanel("Assignment Z");
            myRibbon.CreateButton("Assignment Z", "Align Pipes", "A.AlignPipeEndpoints", "3.jpeg");

            return Result.Succeeded;
        }
    }
    



    public class Ribbon
    {
        private UIControlledApplication application;
        private string tabName = "My Assignments";
        Dictionary<string, RibbonPanel> panels = new Dictionary<string, RibbonPanel>();
        public Ribbon(UIControlledApplication application)
        {
            this.application = application;
            try { application.CreateRibbonTab(tabName); } catch (Exception e) { }
        }
        public void CreatePanel(string panelName)
        {
            try
            {
                RibbonPanel p = application.CreateRibbonPanel(tabName, panelName);
                panels.Add(panelName, p);
            }
            catch (Exception e)
            {

            }
        }

        public void CreateButton(string panelName, string buttonName, string fullclassname, string img, string AvailabilityClassName = null)
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData b = new PushButtonData(buttonName + "Id", buttonName, assemblyPath, fullclassname);
            b.LargeImage = new System.Windows.Media.Imaging.BitmapImage(new Uri(@"C:\Users\rushikesh.phalle\Desktop\Revit_Assignments\Assignments\images\" + img));
            if(AvailabilityClassName!=null) b.AvailabilityClassName = AvailabilityClassName;
            try
            {
                panels[panelName].AddItem(b);
            }catch(Exception e){  
            
            }
        }

        public void CreatePDButton(string panelName, string buttonName, string img, List<(string name, string fullclassname)> buttons , string AvailabilityClassName = null)
        {
            var pBtn = new PulldownButtonData(buttonName, buttonName);
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            try
            {
                PulldownButton p =  panels[panelName].AddItem(pBtn) as PulldownButton;
                p.LargeImage = new System.Windows.Media.Imaging.BitmapImage(new Uri(@"C:\Users\rushikesh.phalle\Desktop\Revit_Assignments\Assignments\images\" + img));
                var pushBtns = buttons.Select(t => new PushButtonData(t.name, t.name, assemblyPath, t.fullclassname));
                foreach (var item in pushBtns)
                {
                    p.AddPushButton(item);
                }
            }
            catch (Exception e)
            {

            }
        }
    }
}
