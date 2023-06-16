using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MapinfoEditor_KOMIKS
{
    public partial class Main : Form
    {
        protected const int ZOOM_RATIO = 2;
        protected int startX;
        protected int startY;
        protected int mouseInitDragX;
        protected int mouseInitDragY;
        protected bool dragging;
        protected bool clicked;
        protected bool editMode;

        public Main()
        {
            InitializeComponent();
            startX = 0;
            startY = 0;
            dragging = false;
            editMode = false;
        }

        private void Main_Load(object sender, EventArgs e)
        {
            Map.Init();
            mapPanel.MouseWheel += mapPanel_MouseWheel;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = @"Save MFO file on...";
            saveFileDialog.Filter = @"Joymax's MFO file|*.mfo";
            saveFileDialog.FileName = "mapinfo.mfo";

            switch (saveFileDialog.ShowDialog())
            {
                case DialogResult.OK:
                    if (Map.SaveMFOFile(saveFileDialog.FileName) == 0)
                    {
                        MessageBox.Show(@"The file has been saved successfully on " + saveFileDialog.FileName, @"Success", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        break;
                    }
                    MessageBox.Show(@"The file couldn't be saved due to an error! (file in use?)", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    break;
                case DialogResult.Cancel:
                    MessageBox.Show(@"The file wasn't saved!", @"Warning!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    break;
                case DialogResult.None:
                case DialogResult.Abort:
                case DialogResult.Retry:
                case DialogResult.Ignore:
                case DialogResult.Yes:
                case DialogResult.No:
                default:
                    MessageBox.Show(@"Unknown error while saving the file!", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    break;
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Title = @"Select a mapinfo file";
            openFileDialog.Filter = @"Joymax's MFO file|*.mfo";

            switch (openFileDialog.ShowDialog())
            {
                case DialogResult.OK:
                    if (Map.LoadMFOFile(openFileDialog.FileName) == 0)
                    {
                        Text = @"Mapinfo Editor - KOMIKS - " + openFileDialog.FileName;
                        saveToolStripMenuItem.Enabled = true;
                        loadRefRegionToolStripMenuItem.Enabled = true;
                        mapPanel.Refresh();
                        break;
                    }
                    MessageBox.Show(@"This is not a valid MFO file!", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    break;
                case DialogResult.Cancel:
                    break;
                case DialogResult.None:
                case DialogResult.Abort:
                case DialogResult.Retry:
                case DialogResult.Ignore:
                case DialogResult.Yes:
                case DialogResult.No:
                default:
                    MessageBox.Show(@"Unknown error while opening the file!", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    break;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void mapPanel_Paint(object sender, PaintEventArgs e)
        {
            mapPanel.Height = Height;
            mapPanel.Width = Width;
            var graphics = e.Graphics;
            var solidBrush1 = new SolidBrush(Color.Black);
            var solidBrush2 = new SolidBrush(Color.White);
            var solidBrush3 = new SolidBrush(Color.DarkBlue);
            var font = new Font("Arial", 8f);
            var index1 = 0;

            graphics.FillRectangle(solidBrush1, new Rectangle(0, 0, Width, Height));
            
            for (var index2 = 0; index2 <= byte.MaxValue; ++index2)
            {
                for (var maxValue = (int) byte.MaxValue; maxValue >= 0; --maxValue)
                {
                    var region = Map.regions[index1];
                    region.DrawX = maxValue * (Map.regionDrawSize + 1) + startX;
                    region.DrawY = index2 * (Map.regionDrawSize + 1) + startY;
                    if (region.State == MapRegion.STATE.ENABLED)
                    {
                        if (region.DrawX < Width && region.DrawY < Height)
                        {
                            graphics.FillRectangle(solidBrush2, new Rectangle(region.DrawX, region.DrawY, Map.regionDrawSize, Map.regionDrawSize));

                            if (Map.regionDrawSize > 40)
                                graphics.DrawString("X: " + region.X + "\nY: " + region.Y, font, solidBrush3, new PointF(region.DrawX, region.DrawY));
                        }
                    }
                    else if (region.DrawX < Width && region.DrawY < Height)
                    {
                        graphics.FillRectangle(solidBrush3, new Rectangle(region.DrawX, region.DrawY, Map.regionDrawSize, Map.regionDrawSize));

                        if (Map.regionDrawSize > 40)
                            graphics.DrawString("X: " + region.X + "\nY: " + region.Y, font, solidBrush2, new PointF(region.DrawX, region.DrawY));
                    }
                    ++index1;
                }
            }

            viewModeToolTip.Text = editMode ? "Edit mode" : "View mode";
        }

        private void mapPanel_MouseDown(object sender, MouseEventArgs e)
        {
            mouseInitDragX = Cursor.Position.X - startX;
            mouseInitDragY = Cursor.Position.Y - startY;
            clicked = true;
        }

        private void mapPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (clicked)
                dragging = true;

            if (dragging)
            {
                startX = Cursor.Position.X - mouseInitDragX;
                startY = Cursor.Position.Y - mouseInitDragY;
                mapPanel.Refresh();
            }

            informationToolBar.Text = @"Not a region";
            
            var region = Map.regions.Find(r => r.Inside(mapPanel.PointToClient(Cursor.Position)));

            if (region != null)
                informationToolBar.Text = @"X: " + region.X + @" | Y: " + region.Y;
        }

        private void mapPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (!dragging && editMode)
            {
                var region = Map.regions.Find(r => r.Inside(mapPanel.PointToClient(Cursor.Position)));
                region.ToggleState();
            }

            mapPanel.Refresh();
            clicked = false;
            dragging = false;
        }
        
        private void mapPanel_MouseWheel(object sender, MouseEventArgs e)
        {
            if (int.Parse(e.Delta.ToString()) > 0)
            {
                if (Map.regionDrawSize + 2 >= 100)
                    return;

                Map.regionDrawSize += 2;
                mapPanel.Refresh();
            }
            else if (Map.regionDrawSize - 2 >= 2)
            {
                Map.regionDrawSize -= 2;
                mapPanel.Refresh();
            }
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!editMode)
            {
                editMode = true;
                viewToolStripMenuItem.Checked = false;
            }
            else editToolStripMenuItem.Checked = true;

            Refresh();
        }

        private void viewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (editMode)
            {
                editMode = false;
                editToolStripMenuItem.Checked = false;
            }
            else viewToolStripMenuItem.Checked = true;

            Refresh();
        }

        private void loadRefRegionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Title = @"Select a RefRegion.txt file";
            openFileDialog.Filter = @"RefRegion|RefRegion.txt";

            switch (openFileDialog.ShowDialog())
            {
                case DialogResult.OK:
                    Map.LoadRefRegion(openFileDialog.FileName);
                    break;
                case DialogResult.Cancel:
                    break;
                case DialogResult.None:
                case DialogResult.Abort:
                case DialogResult.Retry:
                case DialogResult.Ignore:
                case DialogResult.Yes:
                case DialogResult.No:
                default:
                    MessageBox.Show(@"Unknown error while opening the file!", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    break;
            }
        }
    }
}
