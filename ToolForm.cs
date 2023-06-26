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
        private List<Level> Levels;
        private double floorOffset;

        public ToolForm(Document doc)
        {
            InitializeComponent();
            Doc = doc;
            Rooms = GetAllRooms(doc);
            FloorTypes = GetFloorTypes(doc);
            Levels = GetLevels(doc);
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

        private List<Level> GetLevels(Document doc)
        {
            List<Level> levels = new List<Level>();

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> elements = collector.OfClass(typeof(Level)).ToElements();

            foreach (Element element in elements)
            {
                Level level = element as Level;
                if (level != null && level.IsValidObject)
                {
                    levels.Add(level);
                }
            }

            return levels;
        }

        private void InitializeLevelList()
        {
            comboBox1.Items.Clear();

            foreach (Level level in Levels)
            {
                comboBox1.Items.Add(level.Name);
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count > 0 && comboBox1.SelectedItem != null && comboBox3.SelectedItem != null)
            {
                string selectedFloorType = comboBox3.SelectedItem.ToString();
                FloorType floorType = FloorTypes.FirstOrDefault(ft => ft.Name == selectedFloorType);

                if (floorType != null)
                {
                    Transaction trans = new Transaction(Doc, "Create Floors");

                    if (trans.Start() == TransactionStatus.Started)
                    {
                        Level selectedLevel = Levels.FirstOrDefault(level => level.Name == comboBox1.SelectedItem.ToString());
                        double offset = (double)numericUpDown1.Value;
                        int createdFloorCount = 0;

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
                                        XYZ offsetVector = new XYZ(offset, 0, 0); // Horizontal offset
                                        Curve offsetCurve = curve.CreateTransformed(Transform.CreateTranslation(offsetVector));
                                        curveArray.Append(offsetCurve);
                                    }
                                }

                                Floor floor = Doc.Create.NewFloor(curveArray, floorType, selectedLevel, false);
                                if (floor != null)
                                    createdFloorCount++;
                            }
                        }

                        trans.Commit();
                        MessageBox.Show("Created " + createdFloorCount.ToString() + " floors successfully.");
                    }
                }
            }
            else
            {
                MessageBox.Show("You must select at least one Room, one Level, and one Floor Type.");
            }
        }
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            floorOffset = (double)numericUpDown1.Value;
        }
    }
}