using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

namespace My_Revit_Commands
{
    public partial class ToolForm : System.Windows.Forms.Form
    {
        private Document Doc;
        private List<Room> Rooms;
        private List<FloorType> FloorTypes;
        private double floorOffset;

        public ToolForm(Document doc)
        {
            InitializeComponent();
            Doc = doc;
            Rooms = GetAllRooms(doc);
            FloorTypes = GetFloorTypes(doc);
            Load += ToolForm_Load;
            listBox1.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            listBox2.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
        }

        private void ToolForm_Load(object sender, EventArgs e)
        {
            InitializeRoomList();
            InitializeFloorTypeList();
            InitializeCeilingTypeList();
            InitializeLevelList();
        }

        private void InitializeRoomList()
        {
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            foreach (Room room in Rooms)
            {
                string roomName = room.Name;
                listBox1.Items.Add(roomName);
                listBox2.Items.Add(roomName);
            }
        }

        private void InitializeFloorTypeList()
        {
            comboBox3.Items.Clear();

            foreach (FloorType floorType in FloorTypes)
            {
                comboBox3.Items.Add(floorType.Name);
            }
        }

        private void InitializeCeilingTypeList()
        {
            comboBox4.Items.Clear();

            FilteredElementCollector collector = new FilteredElementCollector(Doc);
            collector.OfClass(typeof(CeilingType));

            foreach (CeilingType ceilingType in collector)
            {
                if (ceilingType.IsValidObject)
                {
                    comboBox4.Items.Add(ceilingType.Name);
                }
            }
        }

        private CeilingType GetCeilingTypeByName(string ceilingTypeName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(Doc);
            collector.OfClass(typeof(CeilingType));

            foreach (CeilingType ceilingType in collector)
            {
                if (ceilingType.IsValidObject && ceilingType.Name == ceilingTypeName)
                {
                    return ceilingType;
                }
            }

            return null;
        }

        private List<Room> GetAllRooms(Document doc)
        {
            List<Room> rooms = new List<Room>();

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> elements = collector.OfClass(typeof(SpatialElement)).ToElements();

            IEnumerable<Room> roomElements = elements.Where(elem => elem is Room).Cast<Room>();

            rooms.AddRange(roomElements);

            return rooms;
        }

        private List<FloorType> GetFloorTypes(Document doc)
        {
            List<FloorType> floorTypes = new List<FloorType>();

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(FloorType));

            foreach (FloorType floorType in collector)
            {
                if (floorType.IsValidObject)
                {
                    floorTypes.Add(floorType);
                }
            }

            return floorTypes;
        }

        private void InitializeLevelList()
        {
            comboBox1.Items.Clear();

            FilteredElementCollector collector = new FilteredElementCollector(Doc);
            ICollection<Element> elements = collector.OfClass(typeof(Level)).ToElements();

            foreach (Element element in elements)
            {
                Level level = element as Level;
                if (level != null && level.IsValidObject)
                {
                    Parameter parameter = level.LookupParameter("LEVEL_NUMBER");
                    if (parameter != null && parameter.HasValue)
                    {
                        string levelNumber = parameter.AsString();
                        comboBox1.Items.Add(level);
                    }
                }
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count > 0 && comboBox3.SelectedItem != null)
            {
                string selectedFloorType = comboBox3.SelectedItem.ToString();
                FloorType floorType = FloorTypes.FirstOrDefault(ft => ft.Name == selectedFloorType);

                if (floorType != null)
                {
                    Transaction trans = new Transaction(Doc, "Crear Pisos");

                    if (trans.Start() == TransactionStatus.Started)
                    {
                        Level level = Doc.ActiveView.GenLevel;

                        foreach (object selectedItem in listBox1.SelectedItems)
                        {
                            string selectedRoom = selectedItem.ToString();
                            Room room = Rooms.FirstOrDefault(r => r.Name == selectedRoom);

                            if (room != null)
                            {
                                IList<IList<BoundarySegment>> boundarySegments = room.GetBoundarySegments(new SpatialElementBoundaryOptions());
                                CurveArray curveArray = new CurveArray();

                                foreach (IList<BoundarySegment> segmentList in boundarySegments)
                                {
                                    foreach (BoundarySegment segment in segmentList)
                                    {
                                        Curve curve = segment.GetCurve();
                                        XYZ offsetVector = new XYZ(floorOffset, 0, 0); // Desplazamiento horizontal
                                        Curve offsetCurve = curve.CreateTransformed(Transform.CreateTranslation(offsetVector)); // Aplicar el desplazamiento (offset)
                                        curveArray.Append(offsetCurve);
                                    }
                                }

                                Floor floor = Doc.Create.NewFloor(curveArray, floorType, level, false);
                            }
                        }

                        trans.Commit();
                        MessageBox.Show("Se han creado los pisos correctamente.");
                    }
                }
            }
            else
            {
                MessageBox.Show("Debes seleccionar al menos un Room y un tipo de piso.");
            }
        }
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            floorOffset = (double)numericUpDown1.Value;
        }
    }
}