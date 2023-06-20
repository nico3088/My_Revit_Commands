using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

namespace My_Revit_Commands
{
    public partial class ToolForm : System.Windows.Forms.Form
    {
        Document Doc;
        List<Room> Rooms;
        List<FloorType> FloorTypes;

        public ToolForm(Document doc)
        {
            InitializeComponent();
            Doc = doc;
            Rooms = GetAllRooms(doc);
            FloorTypes = GetFloorTypes(doc);
            Load += ToolForm_Load;
        }

        private void ToolForm_Load(object sender, EventArgs e)
        {
            InitializeRoomList();
            InitializeFloorTypeList();
            InitializeCeilingTypeList();
        }

        private void InitializeRoomList()
        {
            comboBox1.Items.Clear();
            comboBox2.Items.Clear();
            foreach (Room room in Rooms)
            {
                string roomName = room.Name;
                comboBox1.Items.Add(roomName);
                comboBox2.Items.Add(roomName);
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

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null)
            {
                string selectedRoom = comboBox1.SelectedItem.ToString();
                Room room = Rooms.FirstOrDefault(r => r.Name == selectedRoom);

                if (room != null)
                {
                    MessageBox.Show("Room seleccionado: " + room.Name);
                }
            }
        }

        private void ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedItem != null)
            {
                string selectedRoom = comboBox2.SelectedItem.ToString();
                Room room = Rooms.FirstOrDefault(r => r.Name == selectedRoom);

                if (room != null)
                {
                    MessageBox.Show("Room seleccionado: " + room.Name);
                }
            }
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



        private void textBox1_TextChanged(object sender, EventArgs e)
        {
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

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null && comboBox3.SelectedItem != null)
            {
                string selectedRoom = comboBox1.SelectedItem.ToString();
                string selectedFloorType = comboBox3.SelectedItem.ToString();

                Room room = Rooms.FirstOrDefault(r => r.Name == selectedRoom);
                FloorType floorType = FloorTypes.FirstOrDefault(ft => ft.Name == selectedFloorType);

                if (room != null && floorType != null)
                {
                    // Crear el piso en la habitación seleccionada
                    Transaction trans = new Transaction(Doc, "Crear Piso");
                    if (trans.Start() == TransactionStatus.Started)
                    {
                        Level level = Doc.ActiveView.GenLevel;
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
                        trans.Commit();
                        trans.Dispose();
                        MessageBox.Show("Se ha creado el piso correctamente.");
                    }
                }
            }
            else
            {
                MessageBox.Show("Debes seleccionar un Room y un tipo de piso.");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }
    }
}