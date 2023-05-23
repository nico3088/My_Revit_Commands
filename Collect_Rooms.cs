using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace My_Revit_Commands
{
    [TransactionAttribute(TransactionMode.ReadOnly)]
    public class Collect_Rooms : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Get Document
            Document doc = uidoc.Document;

            // Create Filtered Element Collector
            FilteredElementCollector collector = new FilteredElementCollector(doc);

            // Create Filter
            ElementCategoryFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_Rooms);

            // Apply Filter
            IList<Element> rooms = collector.WherePasses(filter).WhereElementIsNotElementType().ToElements();

            // Get the count of rooms
            int roomCount = rooms.Count;

            // Show the number of rooms in a TaskDialog
            TaskDialog.Show("Room Count", $"Number of Rooms: {roomCount}");

            return Result.Succeeded;
        }
    }
}
