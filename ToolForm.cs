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
        private string selectedRoom2;

        public ToolForm(Document doc)
        {
            InitializeComponent();
            Doc = doc;
            Rooms = GetAllRooms(doc);
            Load += ToolForm_Load;
        }

        private void ToolForm_Load(object sender, EventArgs e)
        {
            InitializeRoomList();
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
                Room room = Rooms.FirstOrDefault(r => r.Name == selectedRoom2);

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

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

      

       
    }
}


