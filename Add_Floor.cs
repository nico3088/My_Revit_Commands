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
    public class Add_Floor : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Obtener la aplicación y el documento
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;

            try
            {
                // Obtener los cuartos seleccionados por el usuario
                IList<Reference> pickedRefs = uiApp.ActiveUIDocument.Selection.PickObjects(ObjectType.Element, new RoomPickFilter(), "Seleccione los cuartos");

                if (pickedRefs.Count > 0)
                {
                    using (Transaction transaction = new Transaction(doc, "Agregar Pisos"))
                    {
                        transaction.Start();

                        foreach (Reference pickedRef in pickedRefs)
                        {
                            Room room = doc.GetElement(pickedRef) as Room;
                            AddDefaultFloorToRoom(doc, room);
                        }

                        transaction.Commit();

                        TaskDialog.Show("Éxito", "Se agregaron los pisos a los cuartos seleccionados.");
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

        public void AddDefaultFloorToRoom(Document doc, Room room)
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

            // Crear el suelo (floor)
            doc.Create.NewFloor(curveArray, false);
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
