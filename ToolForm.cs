using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
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
        private Document doc;
        private List<Room> Rooms;
        private List<FloorType> FloorTypes;
        private List<Level> Levels;
        private List<CeilingType> ceilingTypes;
        private double floorOffset;
        private decimal offsetIncrement = 0.1M;
        private decimal currentOffset = 0.0M;

        public ToolForm(Document doc)
        {
            InitializeComponent();
            this.doc = doc;
            Rooms = GetAllRooms(doc);
            FloorTypes = GetFloorTypes(doc);
            Levels = GetLevels(doc);
            Load += ToolForm_Load;
            listBox1.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended; 
        }

        private void ToolForm_Load(object sender, EventArgs e)
        {
            InitializeRoomList();
            InitializeFloorTypeList();
            InitializeCeilingTypeList();
            InitializeLevelList();
            InitializeNumericUpDown();
        }

        private void InitializeRoomList()
        {
            listBox1.Items.Clear();
            foreach (Room room in Rooms)
            {
                string roomName = room.Name;
                listBox1.Items.Add(roomName);
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
            ceilingTypes = new List<CeilingType>();
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(CeilingType));
            foreach (CeilingType ceilingType in collector)
            {
                if (ceilingType.IsValidObject)
                {
                    comboBox4.Items.Add(ceilingType.Name);
                    ceilingTypes.Add(ceilingType);
                }
            }
        }

        private CeilingType GetCeilingTypeByName(string ceilingTypeName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
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
            Autodesk.Revit.DB.View view = doc.ActiveView;
            FilteredElementCollector collector = new FilteredElementCollector(doc, view.Id);
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
            comboBox2.Items.Clear();

            foreach (Level level in Levels)
            {
                comboBox1.Items.Add(level.Name);
                comboBox2.Items.Add(level.Name);
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            currentOffset = numericUpDown1.Value;

            numericUpDown1.Text = currentOffset.ToString("+#.0;-#.0;0.0");

            UpdateNumericUpDownIncrement();
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            currentOffset = numericUpDown2.Value;

           
            numericUpDown2.Text = currentOffset.ToString("+#.0;-#.0;0.0");

            UpdateNumericUpDownIncrement();
        }
        private void UpdateNumericUpDownIncrement()
        {
            numericUpDown1.Increment = offsetIncrement;
            numericUpDown2.Increment = offsetIncrement;
        }

        private void InitializeNumericUpDown()
        {
            numericUpDown1.DecimalPlaces = 1;
            numericUpDown1.Increment = 0.1m;
            numericUpDown1.Minimum = -100m;
            numericUpDown1.Maximum = 100m; 
            numericUpDown1.ThousandsSeparator = false;
            numericUpDown1.Text = currentOffset.ToString("+#.0;-#.0;0.0");

            numericUpDown2.DecimalPlaces = 1;
            numericUpDown2.Increment = 0.1m;
            numericUpDown2.Minimum = -100m; 
            numericUpDown2.Maximum = 100m; 
            numericUpDown2.ThousandsSeparator = false;
            numericUpDown2.Text = currentOffset.ToString("+#.0;-#.0;0.0");
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count > 0 && comboBox1.SelectedItem != null && comboBox3.SelectedItem != null)
            {
                string selectedFloorType = comboBox3.SelectedItem.ToString();
                FloorType floorType = FloorTypes.FirstOrDefault(ft => ft.Name == selectedFloorType);

                if (floorType != null)
                {
                    Transaction trans = new Transaction(doc, "Create Floors");

                    if (trans.Start() == TransactionStatus.Started)
                    {
                        Level selectedLevel = Levels.FirstOrDefault(level => level.Name == comboBox1.SelectedItem.ToString());
                        double floorOffset = (double)numericUpDown1.Value; // Obtener el valor del NumericUpDown1 directamente
                        int createdFloorCount = 0;

                        foreach (object selectedItem in listBox1.SelectedItems)
                        {
                            string selectedRoom = selectedItem.ToString();
                            Room room = Rooms.FirstOrDefault(r => r.Name == selectedRoom);

                            if (room != null)
                            {
                                IList<IList<BoundarySegment>> boundarySegments = room.GetBoundarySegments(new SpatialElementBoundaryOptions());
                                IList<CurveLoop> curveLoops = new List<CurveLoop>();

                                foreach (IList<BoundarySegment> segmentList in boundarySegments)
                                {
                                    CurveLoop curveLoop = new CurveLoop();
                                    foreach (BoundarySegment segment in segmentList)
                                    {
                                        Curve curve = segment.GetCurve();
                                        curveLoop.Append(curve);
                                    }
                                    curveLoops.Add(curveLoop);
                                }

                                Floor floor = Floor.Create(doc, curveLoops, floorType.Id, selectedLevel.Id, false, null, 0.0); // Añadimos el valor predeterminado 0.0 para el parámetro 'slope'
                                if (floor != null)
                                {
                                    // Obtener la elevación actual del piso
                                    double currentElevation = floor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).AsDouble();

                                    // Calcular la nueva elevación aplicando el desplazamiento
                                    double newElevation = selectedLevel.Elevation + floorOffset;

                                    // Mover el piso a la nueva elevación
                                    ElementTransformUtils.MoveElement(doc, floor.Id, new XYZ(0, 0, newElevation - currentElevation));

                                    createdFloorCount++;
                                }
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

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count > 0 && comboBox2.SelectedItem != null && comboBox4.SelectedItem != null)
            {
                string selectedCeilingType = comboBox4.SelectedItem.ToString();
                CeilingType ceilingType = GetCeilingTypeByName(selectedCeilingType);

                if (ceilingType != null)
                {
                    Transaction trans = new Transaction(doc, "Create Ceilings");

                    if (trans.Start() == TransactionStatus.Started)
                    {
                        Level selectedLevel = Levels.FirstOrDefault(level => level.Name == comboBox2.SelectedItem.ToString());
                        double offsetFromLevel = selectedLevel.Elevation;
                        double ceilingOffset = (double)numericUpDown2.Value; // Obtener el valor del NumericUpDown2 directamente

                        int createdCeilingCount = 0;

                        foreach (object selectedItem in listBox1.SelectedItems)
                        {
                            string selectedRoom = selectedItem.ToString();
                            Room room = Rooms.FirstOrDefault(r => r.Name == selectedRoom);

                            if (room != null)
                            {
                                IList<IList<BoundarySegment>> boundarySegments = room.GetBoundarySegments(new SpatialElementBoundaryOptions());
                                CurveLoop profile = new CurveLoop();

                                foreach (IList<BoundarySegment> segmentList in boundarySegments)
                                {
                                    foreach (BoundarySegment segment in segmentList)
                                    {
                                        Curve curve = segment.GetCurve();
                                        XYZ offsetVector = new XYZ(0, 0, ceilingOffset);
                                        Curve offsetCurve = curve.CreateTransformed(Transform.CreateTranslation(offsetVector));
                                        profile.Append(offsetCurve);
                                    }
                                }

                                Ceiling ceiling = Ceiling.Create(doc, new List<CurveLoop> { profile }.Cast<CurveLoop>().ToList(), ceilingType.Id, selectedLevel.Id, null, 0.0);
                                if (ceiling != null)
                                {
                                    // Obtener el parámetro "Height Offset From Level" del techo
                                    Parameter offsetParam = ceiling.get_Parameter(BuiltInParameter.CEILING_HEIGHTABOVELEVEL_PARAM);

                                    // Verificar si el parámetro es válido y ajustar su valor
                                    if (offsetParam != null && offsetParam.StorageType == StorageType.Double)
                                    {
                                        offsetParam.Set(ceilingOffset);
                                    }

                                    createdCeilingCount++;
                                }
                            }
                        }

                        trans.Commit();
                        MessageBox.Show("Created " + createdCeilingCount.ToString() + " ceilings successfully.");
                    }
                }
            }
            else
            {
                MessageBox.Show("You must select at least one Room, one Level, and one Ceiling Type.");
            }
        }
    }

 }
