using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace My_Revit_Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RoomDuplicator : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Obtener la aplicación y el documento
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;

            try
            {
                // Mostrar un mensaje solicitando al usuario que seleccione uno o más cuartos a copiar
                TaskDialog mainDialog = new TaskDialog("Copiar cuartos");
                mainDialog.MainInstruction = "Seleccione uno o más cuartos para copiar";
                mainDialog.MainContent = "Mantenga presionada la tecla Ctrl para seleccionar múltiples cuartos.";
                mainDialog.CommonButtons = TaskDialogCommonButtons.Ok;
                mainDialog.DefaultButton = TaskDialogResult.Ok;
                TaskDialogResult result = mainDialog.Show();

                if (result == TaskDialogResult.Ok)
                {
                    // Solicitar al usuario que seleccione los cuartos para duplicar
                    IList<Reference> pickedRefs = uiApp.ActiveUIDocument.Selection.PickObjects(ObjectType.Element, new RoomPickFilter(), "Seleccione los cuartos para duplicar");

                    if (pickedRefs.Count > 0)
                    {
                        using (Transaction transaction = new Transaction(doc, "Duplicar cuartos"))
                        {
                            transaction.Start();

                            foreach (Reference pickedRef in pickedRefs)
                            {
                                Room sourceRoom = doc.GetElement(pickedRef) as Room;

                                // Solicitar al usuario que seleccione la ubicación para la habitación duplicada
                                XYZ targetLocation = uiApp.ActiveUIDocument.Selection.PickPoint("Seleccione la ubicación para la habitación duplicada");

                                // Duplicar la habitación en la ubicación seleccionada
                                Room duplicatedRoom = DuplicateRoom(doc, sourceRoom, targetLocation);

                                if (duplicatedRoom != null)
                                {
                                    TaskDialog.Show("Éxito", "El cuarto ha sido duplicado exitosamente.");
                                }
                                else
                                {
                                    TaskDialog.Show("Error", "No se pudo duplicar el cuarto.");
                                }
                            }

                            transaction.Commit();
                        }
                    }
                    else
                    {
                        TaskDialog.Show("Error", "No se seleccionaron cuartos para duplicar.");
                    }
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        public Room DuplicateRoom(Document doc, Room sourceRoom, XYZ targetLocation)
        {
            // Copiar el cuarto original y obtener el elemento duplicado
            List<ElementId> copiedRoomIds = ElementTransformUtils.CopyElement(doc, sourceRoom.Id, targetLocation).ToList();
            ElementId copiedRoomId = copiedRoomIds[0];
            Room duplicatedRoom = doc.GetElement(copiedRoomId) as Room;

            // Obtener las paredes que conforman el cuarto original
            IList<IList<BoundarySegment>> boundarySegments = sourceRoom.GetBoundarySegments(new SpatialElementBoundaryOptions());

            // Diccionario para mapear las paredes originales con las duplicadas
            Dictionary<Wall, Wall> wallMapping = new Dictionary<Wall, Wall>();
            foreach (IList<BoundarySegment> segments in boundarySegments)
            {
                foreach (BoundarySegment segment in segments)
                {
                    Curve curve = segment.GetCurve();

                    // Crear la pared
                    Wall wall = Wall.Create(doc, curve, sourceRoom.LevelId, false);

                    // Asignar manualmente la pared a la habitación duplicada
                    duplicatedRoom.get_Parameter(BuiltInParameter.ROOM_FINISH_WALL).Set(wall.Id);

                    // Agregar la pared original y duplicada al mapeo
                    wallMapping.Add(wall, wall);
                }
            }

            // Ajustar la posición de las paredes duplicadas
            foreach (var mapping in wallMapping)
            {
                Wall originalWall = mapping.Key;
                Wall duplicatedWall = mapping.Value;

                // Obtener la posición de la pared original
                LocationCurve originalWallCurve = originalWall.Location as LocationCurve;
                XYZ originalWallStart = originalWallCurve.Curve.GetEndPoint(0);
                XYZ originalWallEnd = originalWallCurve.Curve.GetEndPoint(1);

                // Obtener la posición de la pared duplicada
                LocationCurve duplicatedWallCurve = duplicatedWall.Location as LocationCurve;
                XYZ duplicatedWallStart = duplicatedWallCurve.Curve.GetEndPoint(0);
                XYZ duplicatedWallEnd = duplicatedWallCurve.Curve.GetEndPoint(1);

                // Calcular la diferencia de posición entre las paredes
                XYZ positionDifference = originalWallStart - duplicatedWallStart;

                // Mover la pared duplicada a la posición ajustada
                ElementTransformUtils.MoveElement(doc, duplicatedWall.Id, positionDifference);
            }

            return duplicatedRoom;
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
