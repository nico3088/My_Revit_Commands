using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace My_Revit_Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Add_Ceiling_Or_Floor : IExternalCommand
    {
        private List<string> floorTypes = new List<string>()
        {
            "Type 1",
            "Type 2",
            "Type 3"
        };

        private List<string> ceilingTypes = new List<string>()
        {
            "Type A",
            "Type B",
            "Type C"
        };

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Obtener la aplicación y el documento
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;

            try
            {
                // Obtener todas las habitaciones del documento
                List<Room> rooms = GetAllRooms(doc);

                if (rooms.Count == 0)
                {
                    TaskDialog.Show("Error", "No se encontraron habitaciones en el documento.");
                    return Result.Failed;
                }

                // Mostrar el cuadro de diálogo de selección de habitaciones
                List<Room> selectedRooms = ShowRoomSelectionDialog(rooms);

                if (selectedRooms.Count == 0)
                {
                    TaskDialog.Show("Información", "Se ha omitido la adición del suelo. Continuando con la creación de los cielos rasos.");
                    return Result.Succeeded;
                }

                // Solicitar al usuario que seleccione el tipo de elemento a crear (suelo o cielo raso)
                ElementType selectedElementType = GetElementTypeFromDialog(uiApp);

                if (selectedElementType == ElementType.Floor)
                {
                    // Obtener el tipo de suelo desde el diálogo
                    string selectedFloorType = GetFloorTypeFromDialog(uiApp);
                    if (selectedFloorType == null)
                    {
                        TaskDialog.Show("Información", "Se ha omitido la adición del suelo. Continuando con la creación de los cielos rasos.");
                    }
                    else
                    {
                        using (TransactionGroup group = new TransactionGroup(doc, "Agregar Suelo"))
                        {
                            group.Start();

                            foreach (Room room in selectedRooms)
                            {
                                FloorType floorType = GetFloorType(selectedFloorType, doc);
                                if (floorType != null)
                                    AddFloorToRoom(doc, room, floorType);
                            }

                            group.Assimilate();
                        }

                        TaskDialog.Show("Éxito", "Se ha agregado el suelo a las habitaciones seleccionadas.");
                    }
                }
                else if (selectedElementType == ElementType.Ceiling)
                {
                    // Obtener el tipo de cielo raso desde el diálogo
                    string selectedCeilingType = GetCeilingTypeFromDialog(uiApp);
                    if (selectedCeilingType == null)
                    {
                        TaskDialog.Show("Información", "Se ha omitido la adición del cielo raso.");
                    }
                    else
                    {
                        using (TransactionGroup group = new TransactionGroup(doc, "Agregar Cielo Raso"))
                        {
                            group.Start();

                            foreach (Room room in selectedRooms)
                            {
                                FloorType ceilingType = GetCeilingType(selectedCeilingType, doc);
                                if (ceilingType != null)
                                    AddCeilingToRoom(doc, room, ceilingType);
                            }

                            group.Assimilate();
                        }

                        TaskDialog.Show("Éxito", "Se ha agregado el cielo raso a las habitaciones seleccionadas.");
                    }
                }
                else
                {
                    TaskDialog.Show("Información", "No se seleccionó ningún tipo de elemento.");
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        public List<Room> GetAllRooms(Document doc)
        {
            List<Room> rooms = new List<Room>();

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> elements = collector.OfCategory(BuiltInCategory.OST_Rooms).WhereElementIsNotElementType().ToElements();

            foreach (Element element in elements)
            {
                Room room = element as Room;
                if (room != null)
                {
                    rooms.Add(room);
                }
            }

            return rooms;
        }

        public List<Room> ShowRoomSelectionDialog(List<Room> rooms)
        {
            List<Room> selectedRooms = new List<Room>();

            // Crear un formulario de Windows Forms
            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            form.Text = "Seleccionar habitaciones";
            form.Size = new System.Drawing.Size(300, 300);
            form.StartPosition = FormStartPosition.CenterScreen;

            // Crear un control ListBox para mostrar las habitaciones
            ListBox listBox = new ListBox();
            listBox.SelectionMode = SelectionMode.MultiSimple;
            listBox.Dock = DockStyle.Fill;

            foreach (Room room in rooms)
            {
                listBox.Items.Add(room.Name);
            }

            // Agregar el control ListBox al formulario
            form.Controls.Add(listBox);

            // Crear un botón "Aceptar" para confirmar la selección
            Button okButton = new Button();
            okButton.Text = "Aceptar";
            okButton.DialogResult = DialogResult.OK;
            okButton.Dock = DockStyle.Bottom;

            // Agregar el botón "Aceptar" al formulario
            form.Controls.Add(okButton);

            // Mostrar el formulario y esperar la selección del usuario
            DialogResult result = form.ShowDialog();

            // Obtener las habitaciones seleccionadas
            if (result == DialogResult.OK)
            {
                foreach (int selectedIndex in listBox.SelectedIndices)
                {
                    selectedRooms.Add(rooms[selectedIndex]);
                }
            }

            // Cerrar el formulario
            form.Dispose();

            return selectedRooms;
        }

        public ElementType GetElementTypeFromDialog(UIApplication uiApp)
        {
            TaskDialog taskDialog = new TaskDialog("Seleccionar tipo de elemento");
            taskDialog.MainInstruction = "Seleccione un tipo de elemento:";
            taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Suelo");
            taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Cielo Raso");

            TaskDialogResult result = taskDialog.Show();

            if (result == TaskDialogResult.CommandLink1)
            {
                return ElementType.Floor;
            }
            else if (result == TaskDialogResult.CommandLink2)
            {
                return ElementType.Ceiling;
            }

            return ElementType.None;
        }

        public string GetFloorTypeFromDialog(UIApplication uiApp)
        {
            TaskDialog taskDialog = new TaskDialog("Seleccionar tipo de suelo");
            taskDialog.MainInstruction = "Seleccione un tipo de suelo:";
            for (int i = 0; i < floorTypes.Count; i++)
            {
                taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1 + i, floorTypes[i]);
            }
            taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink4, "Omitir");

            TaskDialogResult result = taskDialog.Show();

            if (result == TaskDialogResult.CommandLink4)
            {
                return null;
            }
            else if (result >= TaskDialogResult.CommandLink1 && result <= TaskDialogResult.CommandLink1 + floorTypes.Count)
            {
                int selectedIndex = (int)result - (int)TaskDialogResult.CommandLink1;
                return floorTypes[selectedIndex];
            }

            return null;
        }

        public string GetCeilingTypeFromDialog(UIApplication uiApp)
        {
            TaskDialog taskDialog = new TaskDialog("Seleccionar tipo de cielo raso");
            taskDialog.MainInstruction = "Seleccione un tipo de cielo raso:";
            for (int i = 0; i < ceilingTypes.Count; i++)
            {
                taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1 + i, ceilingTypes[i]);
            }
            taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink4, "Omitir");

            TaskDialogResult result = taskDialog.Show();

            if (result == TaskDialogResult.CommandLink4)
            {
                return null;
            }
            else if (result >= TaskDialogResult.CommandLink1 && result <= TaskDialogResult.CommandLink1 + ceilingTypes.Count)
            {
                int selectedIndex = (int)result - (int)TaskDialogResult.CommandLink1;
                return ceilingTypes[selectedIndex];
            }

            return null;
        }

        public void AddFloorToRoom(Document doc, Room room, FloorType floorType)
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
            using (Transaction transaction = new Transaction(doc, "Agregar Suelo"))
            {
                transaction.Start();

                // Crear un nuevo suelo utilizando el tipo de suelo especificado
                Floor floor = doc.Create.NewFloor(curveArray, floorType, level, false);

                // Realizar cualquier otra operación que necesites en el suelo creado (por ejemplo, establecer propiedades adicionales)

                transaction.Commit();
            }
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

    // Crear el techo (ceiling)
    using (Transaction transaction = new Transaction(doc, "Agregar Techo"))
    {
        transaction.Start();

        // Obtener el símbolo de techo a partir del tipo de piso (floor type) proporcionado
        ElementId ceilingTypeId = ceilingType.Id;
        Element ceilingTypeElement = doc.GetElement(ceilingTypeId);
        FamilySymbol ceilingSymbol = ceilingTypeElement as FamilySymbol;

        // Obtener el techo por su categoría
        FilteredElementCollector collector = new FilteredElementCollector(doc);
        ICollection<Element> ceilingCollection = collector.OfCategory(BuiltInCategory.OST_Ceilings).ToElements();

        // Crear un nuevo techo utilizando el símbolo de techo apropiado
        FamilyInstance ceiling = null;
        foreach (Element element in ceilingCollection)
        {
            FamilyInstance familyInstance = element as FamilyInstance;
            if (familyInstance.Symbol.Name == ceilingSymbol.Name)
            {
                Curve curve = curveArray.get_Item(0); // Obtener la primera curva del CurveArray
                ceiling = doc.Create.NewFamilyInstance(curve, ceilingSymbol, level, StructuralType.NonStructural);
                break;
            }
        }

        if (ceiling == null)
        {
            // Si no se encuentra el techo en la colección existente, se puede crear un nuevo tipo de techo y utilizarlo
            ceilingSymbol.Name = "Nuevo Techo";
            Curve curve = curveArray.get_Item(0); // Obtener la primera curva del CurveArray
            ceiling = doc.Create.NewFamilyInstance(curve, ceilingSymbol, level, StructuralType.NonStructural);
        }

        // Realizar cualquier otra operación que necesites en el techo creado (por ejemplo, establecer propiedades adicionales)

        transaction.Commit();
    }
}





        public FloorType GetFloorType(string floorTypeName, Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> elements = collector.OfClass(typeof(FloorType)).ToElements();

            foreach (Element element in elements)
            {
                FloorType floorType = element as FloorType;
                if (floorType != null && floorType.Name == floorTypeName)
                {
                    return floorType;
                }
            }

            return null;
        }

        public FloorType GetCeilingType(string ceilingTypeName, Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> elements = collector.OfClass(typeof(FloorType)).ToElements();

            foreach (Element element in elements)
            {
                FloorType ceilingType = element as FloorType;
                if (ceilingType != null && ceilingType.Name == ceilingTypeName)
                {
                    return ceilingType;
                }
            }

            return null;
        }
    }

    public enum ElementType
    {
        None,
        Floor,
        Ceiling
    }
}
