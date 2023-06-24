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

        private void GetAvailableFloorTypes()
        {
            FilteredElementCollector collector = new FilteredElementCollector(Doc);
            collector.OfClass(typeof(FloorType));

            List<string> floorTypeNames = new List<string>();

            foreach (FloorType floorType in collector)
            {
                if (floorType.IsValidObject)
                {
                    floorTypeNames.Add(floorType.Name);
                }
            }


            foreach (string name in floorTypeNames)
            {
                Console.WriteLine(name);
            }
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
                                CurveArray curveArray = new CurveArray();

                                IList<IList<BoundarySegment>> boundarySegments = room.GetBoundarySegments(new SpatialElementBoundaryOptions());
                                foreach (IList<BoundarySegment> segmentList in boundarySegments)
                                {
                                    foreach (BoundarySegment segment in segmentList)
                                    {
                                        curveArray.Append(segment.GetCurve());
                                    }
                                }

                                Floor floor = Doc.Create.NewFloor(curveArray, floorType, level, false);
                            }
                        }

                        trans.Commit();
                        trans.Dispose();
                        MessageBox.Show("Se han creado los pisos correctamente.");
                    }
                }
            }
            else
            {
                MessageBox.Show("Debes seleccionar al menos un Room y un tipo de piso.");
            }
        }


        private void domainUpDown1_SelectedItemChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void listView3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ToolForm_Load_1(object sender, EventArgs e)
        {

        }




        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }
    }
}