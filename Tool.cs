using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using System.Windows.Forms;

namespace My_Revit_Commands
{
    [Transaction(TransactionMode.Manual)]
    public class ToolFormCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;

            using (ToolForm form = new ToolForm(doc))
            {
                form.ShowDialog();
            }

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class Tool : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            string tabName = "Smart Knock";
            application.CreateRibbonTab(tabName);

            string panelName = "Modeling";
            RibbonPanel panel = application.CreateRibbonPanel(tabName, panelName);

            string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            string buttonName = "Floor/RCP Modeler";
            string buttonDescription = "Creation of floor, CPR from the room.";
            PushButtonData toolFormButtonData = new PushButtonData("ToolFormButton", buttonName, assemblyPath, "My_Revit_Commands.ToolFormCommand");
            toolFormButtonData.ToolTip = buttonDescription;
            PushButton toolFormButton = panel.AddItem(toolFormButtonData) as PushButton;

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {

            return Result.Succeeded;
        }
    }
}