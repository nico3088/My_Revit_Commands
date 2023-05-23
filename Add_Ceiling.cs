using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;

namespace My_Revit_Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Add_Ceiling: IExternalCommand
    {
        private List<string> ceilingTypes = new List<string>()
        {
            "Type 1",
            "Type 2",
            "Type 3"
        };

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Obtener la aplicación y el documento
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;

            try
            {
                // Solicitar al usuario que seleccione uno o varios cuartos
                IList<Reference> pickedRefs = uiApp.ActiveUIDocument.Selection.PickObjects(ObjectType.Element, new RoomPickFilter(), "Seleccione uno o varios cuartos");

                if (pickedRefs.Count > 0)
                {
                    using (TransactionGroup group = new TransactionGroup(doc, "Agregar Cielo Raso"))
                    {
                        group.Start();

                        TaskDialog taskDialog = new TaskDialog("Seleccionar tipo de cielo raso");
                        taskDialog.MainInstruction = "Seleccione un tipo de cielo raso:";
                        for (int i = 0; i < ceilingTypes.Count; i++)
                        {
                            taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1 + i, ceilingTypes[i]);
                        }
                        TaskDialogResult result = taskDialog.Show();
                        if (result == TaskDialogResult.CommandLink1)
                        {
                            string selectedType = ceilingTypes[0];
                            foreach (Reference pickedRef in pickedRefs)
                            {
                                Room room = doc.GetElement(pickedRef) as Room;
                                FloorType ceilingType = GetCeilingType(selectedType, doc);
                                if (ceilingType != null)
                                    AddCeilingToRoom(doc, room, ceilingType);
                            }
                        }
                        else if (result == TaskDialogResult.CommandLink2)
                        {
                            string selectedType = ceilingTypes[1];
                            foreach (Reference pickedRef in pickedRefs)
                            {
                                Room room = doc.GetElement(pickedRef) as Room;
                                FloorType ceilingType = GetCeilingType(selectedType, doc);
                                if (ceilingType != null)
                                    AddCeilingToRoom(doc, room, ceilingType);
                            }
                        }
                        else if (result == TaskDialogResult.CommandLink3)
                        {
                            string selectedType = ceilingTypes[2];
                            foreach (Reference pickedRef in pickedRefs)
                            {
                                Room room = doc.GetElement(pickedRef) as Room;
                                FloorType ceilingType = GetCeilingType(selectedType, doc);
                                if (ceilingType != null)
                                    AddCeilingToRoom(doc, room, ceilingType);
                            }
                        }

                        group.Assimilate();

                        TaskDialog.Show("Éxito", "Se ha agregado el cielo raso a los cuartos seleccionados.");
                    }
                }
                else
                {
                    TaskDialog.Show("Error", "No se seleccionaron cuartos.");
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        public void AddCeilingToRoom(Document doc, Room room, FloorType ceilingType)
        {
            // Obtener el nivel del cuarto
            Level level = doc.GetElement(room.LevelId) as Level;

            // Obtener los segmentos de límite del cuarto
            IList<IList<BoundarySegment>> boundarySegments = room.GetBoundarySegments(new SpatialElementBoundaryOptions());

            // Crear el objeto CurveArray
            CurveArray curveArray = new CurveArray();

            foreach (IList<BoundarySegment> segments in boundarySegments)
            {
                foreach (BoundarySegment segment in segments)
                {
                    Curve curve = segment.GetCurve();
                    curveArray.Append(curve);
                }
            }

            // Crear el cielo raso (ceiling)
            using (Transaction transaction = new Transaction(doc, "Agregar Cielo Raso"))
            {
                transaction.Start();

                doc.Create.NewFloor(curveArray, false);
                ElementId ceilingTypeId = ceilingType.Id;
                doc.ActiveView.get_Parameter(BuiltInParameter.VIEW_PHASE).Set(ceilingTypeId);

                transaction.Commit();
            }
        }

        public FloorType GetCeilingType(string ceilingTypeName, Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(FloorType));

            foreach (FloorType floorType in collector)
            {
                if (floorType.Name.Equals(ceilingTypeName, StringComparison.OrdinalIgnoreCase))
                    return floorType;
            }
            return null;
        }

        public class RoomPickFilter : ISelectionFilter
        {
            public bool AllowElement(Element element)
            {
                return element is Room;
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                return false;
            }
        }
    }
}
