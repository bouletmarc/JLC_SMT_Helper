using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace JLC_SMT_Helper
{
    public partial class Form1 : Form
    {
        List<string> EndLinesBOM = new List<string>();
        List<string> EndLinesCPL = new List<string>();

        bool Opened_BOM = false;
        bool Opened_CPL = false;

        bool DoingPartsList = false;
        bool AlreadyConverted = false;
        string Filepath = "";
        string Filename = "";


        bool Loading = false;

        public Form1()
        {
            InitializeComponent();

            label2.Text = "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void SpawnGridBOM()
        {
            if (EndLinesBOM.Count > 0)
            {
                Loading = true;
                dataGridView1.Rows.Clear();

                for (int i = 1; i < EndLinesBOM.Count; i++)
                {
                    string[] SplittedCmds = EndLinesBOM[i].Split(';');
                    dataGridView1.Rows.Add(SplittedCmds);
                }
                Loading = false;
            }
        }

        private void SpawnGridCPL()
        {
            if (EndLinesCPL.Count > 0)
            {
                dataGridView2.Rows.Clear();

                for (int i = 1; i < EndLinesCPL.Count; i++)
                {
                    string[] SplittedCmds = EndLinesCPL[i].Split(';');
                    dataGridView2.Rows.Add(SplittedCmds);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.GetCellCount(DataGridViewElementStates.Selected) > 0)
            {
                EndLinesBOM.RemoveAt(dataGridView1.SelectedCells[0].RowIndex + 1);
            }

            SpawnGridBOM();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            EndLinesBOM.Add(";;;;;");

            int AddingAt = -1;
            if (dataGridView1.GetCellCount(DataGridViewElementStates.Selected) > 0)
            {
                AddingAt = dataGridView1.SelectedCells[0].RowIndex + 1;
            }
            
            if (AddingAt != -1)
            {
                MoveLine(true, AddingAt + 1);
                /*List<string> EndLinesBuf = new List<string>();
                EndLinesBuf = EndLines;

                for (int i = AddingAt; i < EndLines.Count; i++)
                {
                    if (i == AddingAt) EndLines[i] = EndLinesBuf[EndLinesBuf.Count - 1];
                    else EndLines[i] = EndLinesBuf[i - 1];
                }*/
            }

            SpawnGridBOM();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //SaveFile();
        }

        private void MoveLine(bool BOM_File, int ToLine)
        {
            if (BOM_File)
            {
                string ThisLine = EndLinesBOM[EndLinesBOM.Count - 1];
                EndLinesBOM.RemoveAt(EndLinesBOM.Count - 1);
                EndLinesBOM.Insert(ToLine, ThisLine);
            }
            else
            {
                string ThisLine = EndLinesCPL[EndLinesCPL.Count - 1];
                EndLinesCPL.RemoveAt(EndLinesCPL.Count - 1);
                EndLinesCPL.Insert(ToLine, ThisLine);
            }
        }

        private void SaveFile(bool BOM_File)
        {
            List<string> EndLinesBuffer = new List<string>();

            if (BOM_File) EndLinesBuffer = EndLinesBOM;
            else EndLinesBuffer = EndLinesCPL;

            if (EndLinesBuffer.Count > 0)
            {
                string SaveString = "";

                for (int i = 0; i < EndLinesBuffer.Count; i++)
                {
                    string[] SplittedCmds = EndLinesBuffer[i].Split(';');

                    for (int i2 = 0; i2 < SplittedCmds.Length; i2++)
                    {
                        string RemadeWithComma = SplittedCmds[i2];
                        if (RemadeWithComma.Contains(",")) RemadeWithComma = "\"" + RemadeWithComma + "\"";

                        SaveString += RemadeWithComma;
                        if (i2 < SplittedCmds.Length - 1) SaveString += ",";
                    }

                    if (i < EndLinesBuffer.Count - 1) SaveString += Environment.NewLine;
                }

                if (BOM_File)
                {
                    string SName = Filename;
                    SName = SName.Replace("_locations", "");
                    SName = SName.Replace("_location", "");
                    SName = SName.Replace("_Locations", "");
                    SName = SName.Replace("_Location", "");
                    SName = SName.Replace("_JLC_BOM", "");
                    saveFileDialog1.FileName = SName + "_JLC_BOM.csv";
                }
                else
                {
                    string SName = Filename;
                    SName = SName.Replace("_parts", "");
                    SName = SName.Replace("_part", "");
                    SName = SName.Replace("_Parts", "");
                    SName = SName.Replace("_part", "");
                    SName = SName.Replace("_JLC_CPL", "");
                    saveFileDialog1.FileName = SName + "_JLC_CPL.csv";
                }

                DialogResult result = saveFileDialog1.ShowDialog();
                if (result == DialogResult.OK)
                {
                    File.Create(saveFileDialog1.FileName).Dispose();
                    File.WriteAllText(saveFileDialog1.FileName, SaveString);
                }
            }
        }

        void LoadFile(bool BOM_File)
        {
            if (BOM_File)
            {
                openFileDialog1.Filter = "Eagle BOM File CSV|*.csv";
                openFileDialog1.DefaultExt = "csv";
            }
            else
            {
                openFileDialog1.Filter = "Eagle Mount SMD File MNT or CSV|*.csv;*.mnt";
                openFileDialog1.DefaultExt = "csv";
            }
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                Filepath = Path.GetDirectoryName(openFileDialog1.FileName);
                Filename = Path.GetFileNameWithoutExtension(openFileDialog1.FileName);
                string[] AllLines = File.ReadAllLines(openFileDialog1.FileName);

                this.Text = "JLC PCB - SMT Assembly Files Helper - " + Filename + ".csv";

                if (AllLines.Length > 0)
                {
                    Loading = true;
                    DoingPartsList = false;
                    AlreadyConverted = false;
                    bool WarningDone = false;
                    //EndLines.Clear();
                    int QuantityAt = 0;
                    int DesignatorAt = 0;
                    int FootprintAt = 0;
                    int CommentAt = 0;
                    int ValueAt = 0;

                    bool CanGo = true;

                    for (int i = 0; i < AllLines.Length; i++)
                    {
                        //EndLines.Add(AllLines[i]);

                        //";"
                        if (i == 0)
                        {
                            if (AllLines[i].Contains(";") || AllLines[i].Contains("Footprint")) DoingPartsList = true;
                            if (AllLines[i].Contains("Designator")) AlreadyConverted = true;
                        }

                        if (!WarningDone)
                        {
                            if ((!DoingPartsList && BOM_File) || (DoingPartsList && !BOM_File))
                            {
                                CanGo = false;
                                DialogResult result2 = MessageBox.Show("The file opened aren't of the correct type\nDo you still want to open the file using the other type method?", "Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                                if (result2 == DialogResult.Yes) CanGo = true;

                                WarningDone = true;
                            }
                        }

                        if (CanGo)
                        {
                            if (i == 0)
                            {
                                if (DoingPartsList) EndLinesBOM.Clear();
                                else EndLinesCPL.Clear();
                            }

                            if (!AlreadyConverted)
                            {
                                if (DoingPartsList)
                                {
                                    //"Qty";"Value";"Device";"Package";"Parts";"Description";"MF";"MPN";"OC_FARNELL";"OC_NEWARK";"PROD_ID";"SF_ID";"SPICEPREFIX";"VALUE";
                                    //Qty,Designator,Footprint,Comment

                                    //EndLines[i] = EndLines[i].Replace("\"", "");
                                    AllLines[i] = AllLines[i].Replace("\"", "");
                                    string ThisRemadeLine = "";
                                    string[] SplittedCmds = AllLines[i].Split(';');

                                    if (i == 0)
                                    {
                                        for (int i2 = 0; i2 < SplittedCmds.Length; i2++)
                                        {
                                            if (SplittedCmds[i2].Contains("Qty")) QuantityAt = i2;
                                            if (SplittedCmds[i2].Contains("Parts")) DesignatorAt = i2;
                                            if (SplittedCmds[i2].Contains("Package")) FootprintAt = i2;
                                            if (SplittedCmds[i2].Contains("Description")) CommentAt = i2;
                                            if (SplittedCmds[i2].Contains("Value")) ValueAt = i2;
                                        }
                                        ThisRemadeLine = "Qty;Designator;Footprint;Comment;LCSC";
                                    }
                                    else
                                    {
                                        SplittedCmds[FootprintAt] = SplittedCmds[FootprintAt].Replace("C0402", "Capacitor 0402");
                                        SplittedCmds[FootprintAt] = SplittedCmds[FootprintAt].Replace("C0603", "Capacitor 0603");
                                        SplittedCmds[FootprintAt] = SplittedCmds[FootprintAt].Replace("C0805", "Capacitor 0805");
                                        SplittedCmds[FootprintAt] = SplittedCmds[FootprintAt].Replace("C1206", "Capacitor 1206");
                                        SplittedCmds[FootprintAt] = SplittedCmds[FootprintAt].Replace("C1205", "Capacitor 1205");
                                        SplittedCmds[FootprintAt] = SplittedCmds[FootprintAt].Replace("R0402", "Resistor 0402");
                                        SplittedCmds[FootprintAt] = SplittedCmds[FootprintAt].Replace("R0603", "Resistor 0603");
                                        SplittedCmds[FootprintAt] = SplittedCmds[FootprintAt].Replace("R0805", "Resistor 0805");
                                        SplittedCmds[FootprintAt] = SplittedCmds[FootprintAt].Replace("R1206", "Resistor 1206");
                                        SplittedCmds[FootprintAt] = SplittedCmds[FootprintAt].Replace("R1205", "Resistor 1205");
                                        SplittedCmds[FootprintAt] = SplittedCmds[FootprintAt].Replace("CHIPLED", "LED");
                                        SplittedCmds[FootprintAt] = SplittedCmds[FootprintAt].Replace("CHIP-LED", "LED ");
                                        SplittedCmds[FootprintAt] = SplittedCmds[FootprintAt].Replace("$", " ");
                                        SplittedCmds[FootprintAt] = SplittedCmds[FootprintAt].Replace("-", " ");
                                        SplittedCmds[FootprintAt] = SplittedCmds[FootprintAt].Replace("_", " ");
                                        SplittedCmds[FootprintAt] = SplittedCmds[FootprintAt].Replace("/", " ");
                                        SplittedCmds[FootprintAt] = SplittedCmds[FootprintAt].Replace("\\", " ");
                                        SplittedCmds[FootprintAt] = SplittedCmds[FootprintAt].Replace("@1", "");

                                        
                                        SplittedCmds[ValueAt] = SplittedCmds[ValueAt].Replace("$", " ");
                                        SplittedCmds[ValueAt] = SplittedCmds[ValueAt].Replace("-", " ");
                                        SplittedCmds[ValueAt] = SplittedCmds[ValueAt].Replace("_", " ");
                                        SplittedCmds[ValueAt] = SplittedCmds[ValueAt].Replace("/", " ");
                                        SplittedCmds[ValueAt] = SplittedCmds[ValueAt].Replace("\\", " ");
                                        SplittedCmds[ValueAt] = SplittedCmds[ValueAt].Replace("0.1uF", "100nF");
                                        SplittedCmds[ValueAt] = SplittedCmds[ValueAt].Replace("@1", "");

                                        string CommentLine = SplittedCmds[FootprintAt] + " " + SplittedCmds[ValueAt];

                                        if (CommentLine.Contains("MEGA") && !CommentLine.Contains("ATMEGA")) CommentLine = CommentLine.Replace("MEGA", "ATMEGA");
                                        CommentLine = CommentLine.Replace("CAP ", "Capacitor ");
                                        CommentLine = CommentLine.Replace("TO 252 +12v", "78M05");
                                        CommentLine = CommentLine.Replace("DPACK 3 ", "");
                                        CommentLine = CommentLine.Replace("DO214AC ", "");

                                        ThisRemadeLine = SplittedCmds[QuantityAt] + ";" + SplittedCmds[DesignatorAt] + ";" + CommentLine + ";" + CommentLine + ";";
                                        //ThisRemadeLine = SplittedCmds[QuantityAt] + ";" + SplittedCmds[DesignatorAt] + ";" + SplittedCmds[FootprintAt] + " " + SplittedCmds[ValueAt] + ";" + SplittedCmds[CommentAt] + ";";
                                    }

                                    EndLinesBOM.Add(ThisRemadeLine);
                                }
                                else
                                {
                                    string ThisRemadeLine = "";
                                    string[] SplittedCmds = AllLines[i].Split(' ');
                                    int AddingCount = 0;

                                    for (int i2 = 0; i2 < SplittedCmds.Length; i2++)
                                    {
                                        if (SplittedCmds[i2] != "")
                                        {
                                            ThisRemadeLine += SplittedCmds[i2] + ";";

                                            if (AddingCount == 2)
                                            {
                                                ThisRemadeLine += "TOP;";
                                                AddingCount++;
                                            }
                                            AddingCount++;
                                        }
                                    }

                                    if (i == 0) EndLinesCPL.Add("Designator;Mid X;Mid Y;Layer;Rotation");
                                    EndLinesCPL.Add(ThisRemadeLine);
                                }
                            }
                            else
                            {
                                string ThisRemadeLine = "";

                                if (DoingPartsList)
                                {
                                    //5,"D2, D3, D4, D5, D8",LED-0603,
                                    string CuttingLine = AllLines[i];
                                    while (CuttingLine.Contains(","))
                                    {
                                        //Console.WriteLine("here:" + CuttingLine);
                                        bool HasComma = false;
                                        if (CuttingLine[0] == '\"') HasComma = true;

                                        if (!HasComma) ThisRemadeLine += CuttingLine.Substring(0, CuttingLine.IndexOf(","));
                                        else ThisRemadeLine += CuttingLine.Substring(1, CuttingLine.IndexOf("\",") - 1);
                                        ThisRemadeLine += ";";

                                        //Console.WriteLine("here:" + ThisRemadeLine);


                                        if (!HasComma) CuttingLine = CuttingLine.Substring(CuttingLine.IndexOf(",") + 1);
                                        else CuttingLine = CuttingLine.Substring(CuttingLine.IndexOf("\",") + 2);
                                    }
                                    //Console.WriteLine("here:" + ThisRemadeLine);

                                    EndLinesBOM.Add(ThisRemadeLine);
                                }
                                else
                                {
                                    ThisRemadeLine = AllLines[i].Replace(",", ";");
                                    EndLinesCPL.Add(ThisRemadeLine);
                                }

                            }
                        }
                    }

                    if (DoingPartsList)
                    {
                        SpawnGridBOM(); //Must Spawn the grid before checking for unavailable parts
                        Check4unavailablepart();
                        SpawnGridBOM();
                        toolStripMenuItem2.Enabled = true;
                        lSCSPartsListFromBOMcsvToolStripMenuItem.Enabled = true;
                        Opened_BOM = true;
                        label5.Visible = false;
                    }
                    else
                    {
                        SpawnGridCPL();
                        toolStripMenuItem3.Enabled = true;
                        Opened_CPL = true;
                        label6.Visible = false;
                    }

                    Loading = false;
                }
            }
        }

        private void Check4LSCSPartNumber()
        {
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                CheckPartNumber(dataGridView1.Rows[i].Cells[3].Value.ToString());
            }
        }

        private void CheckPartNumber(string ThisPart)
        {
            //if (ThisPart == "")
            //EndLinesBOM.RemoveAt(dataGridView1.SelectedCells[0].RowIndex + 1);

        }

        private void Check4unavailablepart()
        {
            int i = 0;
            while (i < dataGridView1.Rows.Count)
            {
                if (IsUnavailablePart(dataGridView1.Rows[i].Cells[2].Value.ToString()))
                {
                    EndLinesBOM.RemoveAt(i + 1);
                    SpawnGridBOM();
                } 
                else
                {
                    i++;
                }
            }
        }

        private bool IsUnavailablePart(string ThisPart)
        {
            if (ThisPart == "SMD1,27 2,54 1 MISO") return true;
            if (ThisPart == "SMD1,27 2,54 3 SCK") return true;
            if (ThisPart == "SMD1,27 2,54 5 RST") return true;
            if (ThisPart == "SMD1,27 2,54 2") return true;
            if (ThisPart == "SMD1,27 2,54 4") return true;
            if (ThisPart == "SMD1,27 2,54 6") return true;

            if (ThisPart == "MKDSN1,5 2 5,08 MKDSN1,5 2 5,08") return true;
            if (ThisPart == "MKDSN1,5 2 5,08 ") return true;
            if (ThisPart == "MKDSN1,5 2 5,08") return true;

            if (ThisPart == "USB MINIB USB B") return true;
            if (ThisPart == "USB B PTH USB B PTH") return true;

            if (ThisPart == "SMT JUMPER 3 1 NC TRACE SILK JMP") return true;
            if (ThisPart == "SMT JUMPER 2 NO NO SILK JMP") return true;
            if (ThisPart == "SMT JUMPER 2 NC PASTE NO SILK ") return true;
            if (ThisPart == "651005136521 651005136521") return true;

            if (ThisPart == "HC49 HC49") return true;
            if (ThisPart == "HC49 CRYSTAL") return true;

            if (ThisPart == "B25P 5K") return true;
            if (ThisPart == "C025 050X050 10uF 35V") return true;
            if (ThisPart == "B3F 10XX ") return true;
            if (ThisPart == "1727036 1727036") return true;
            if (ThisPart == "EG1213 EG1213") return true;
            if (ThisPart == "EVUF POT") return true;
            if (ThisPart == "POWER JACK PTH POWER JACK") return true;
            if (ThisPart == "BATTCOM 20MM PTH ") return true;
            if (ThisPart == "DIL28 ") return true;
            if (ThisPart == "228 1277 00 0602J ") return true;

            if (ThisPart == "1X01 ") return true;
            if (ThisPart == "1X02 ") return true;
            if (ThisPart == "1X03 ") return true;
            if (ThisPart == "1X04 ") return true;
            if (ThisPart == "1X05 ") return true;
            if (ThisPart == "1X06 ") return true;
            if (ThisPart == "1X16 ") return true;
            if (ThisPart == "1X01 90 ") return true;
            if (ThisPart == "1X02 90 ") return true;
            if (ThisPart == "1X03 90 ") return true;
            if (ThisPart == "1X04 90 ") return true;
            if (ThisPart == "1X05 90 ") return true;
            if (ThisPart == "1X06 90 ") return true;
            if (ThisPart == "1X03 SMALL DATA") return true;
            if (ThisPart == "1X01 ROUND ") return true;
            if (ThisPart == "1X01 90 ROUND ") return true;
            if (ThisPart == "1X02 ROUND ") return true;
            if (ThisPart == "1X02 90 ROUND ") return true;
            if (ThisPart == "1X03 ROUND ") return true;
            if (ThisPart == "1X03 90 ROUND ") return true;

            if (ThisPart.Contains("NOT REQUIRED")) return true;
            if (ThisPart.Contains("not required")) return true;

            //if (ThisPart == "") return true;

            return false;
        }

        private void bOMcsvToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadFile(true);
        }

        private void cPLcsvToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadFile(false);
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            SaveFile(true);
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            SaveFile(false);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            int AddingAt = -1;
            if (dataGridView1.GetCellCount(DataGridViewElementStates.Selected) > 0)
            {
                AddingAt = dataGridView1.SelectedCells[0].RowIndex + 1;
            }

            if (AddingAt != -1)
            {
                string CurrentItem = EndLinesBOM[AddingAt];
                string[] SplittedCmds = CurrentItem.Split(';');

                if (SplittedCmds.Length >= 1)
                {
                    if (SplittedCmds[1].Contains(","))
                    {
                        string[] SplittedDesignator = SplittedCmds[1].Split(',');

                        EndLinesBOM[AddingAt] = "1;" + SplittedDesignator[0] + ";";
                        for (int i = 2; i < SplittedCmds.Length; i++) EndLinesBOM[AddingAt] += SplittedCmds[i] + ";";

                        for (int i = 1; i < SplittedDesignator.Length; i++)
                        {
                            EndLinesBOM.Add("1;" + SplittedDesignator[i].Substring(1) + ";");
                            for (int i2 = 2; i2 < SplittedCmds.Length; i2++) EndLinesBOM[EndLinesBOM.Count - 1] += SplittedCmds[i2] + ";";

                            MoveLine(true, AddingAt + i);
                        }

                        SpawnGridBOM();
                    }
                }
            }
        }

        private void dataGridView1_Click(object sender, EventArgs e)
        {
            if (!Opened_BOM)
            {
                LoadFile(true);
            }
            else
            {
                int AddingAt = -1;
                if (dataGridView1.GetCellCount(DataGridViewElementStates.Selected) > 0)
                {
                    AddingAt = dataGridView1.SelectedCells[0].RowIndex;
                }

                if (AddingAt != -1)
                {
                    if (dataGridView1.Rows[AddingAt].Cells.Count >= 2)
                    {
                        label2.Text = dataGridView1.Rows[AddingAt].Cells[1].Value.ToString();
                    }
                }
            }
        }

        private void dataGridView2_Click(object sender, EventArgs e)
        {
            if (!Opened_CPL)
            {
                LoadFile(false);
            }
            else
            {
                int AddingAt = -1;
                if (dataGridView2.GetCellCount(DataGridViewElementStates.Selected) > 0)
                {
                    AddingAt = dataGridView2.SelectedCells[0].RowIndex;
                }

                if (AddingAt != -1)
                {
                    if (dataGridView2.Rows[AddingAt].Cells.Count >= 5)
                    {
                        Loading = true;
                        if (dataGridView2.Rows[AddingAt].Cells[3].Value.ToString() == "TOP") comboBox1.SelectedIndex = 0;
                        if (dataGridView2.Rows[AddingAt].Cells[3].Value.ToString() == "BOTTOM") comboBox1.SelectedIndex = 1;

                        if (dataGridView2.Rows[AddingAt].Cells[4].Value.ToString() == "0") comboBox2.SelectedIndex = 0;
                        if (dataGridView2.Rows[AddingAt].Cells[4].Value.ToString() == "90") comboBox2.SelectedIndex = 1;
                        if (dataGridView2.Rows[AddingAt].Cells[4].Value.ToString() == "180") comboBox2.SelectedIndex = 2;
                        if (dataGridView2.Rows[AddingAt].Cells[4].Value.ToString() == "270") comboBox2.SelectedIndex = 3;
                        Loading = false;
                    }
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!Loading)
            {
                int AddingAt = -1;
                if (dataGridView2.GetCellCount(DataGridViewElementStates.Selected) > 0)
                {
                    AddingAt = dataGridView2.SelectedCells[0].RowIndex;
                }

                if (AddingAt != -1)
                {
                    AddingAt++;
                    string[] SplittedCmds = EndLinesCPL[AddingAt].Split(';');
                    EndLinesCPL[AddingAt] = SplittedCmds[0] + ";" + SplittedCmds[1] + ";" + SplittedCmds[2] + ";" + comboBox1.Text + ";" + SplittedCmds[4] + ";";

                    SpawnGridCPL();
                }
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!Loading)
            {
                for (int i = 0; i < dataGridView2.Rows.Count; i++)
                {
                    if (ItemSelectedInThisRow(dataGridView2, i))
                    {
                        int AddingAt = i + 1;
                        string[] SplittedCmds = EndLinesCPL[AddingAt].Split(';');
                        EndLinesCPL[AddingAt] = SplittedCmds[0] + ";" + SplittedCmds[1] + ";" + SplittedCmds[2] + ";" + SplittedCmds[3] + ";" + comboBox2.Text + ";";

                        //SpawnGridCPL();
                    }
                }
                SpawnGridCPL();

                //##########################
                /*int AddingAt = -1;
                if (dataGridView2.GetCellCount(DataGridViewElementStates.Selected) > 0)
                {
                    AddingAt = dataGridView2.SelectedCells[0].RowIndex;
                }

                if (AddingAt != -1)
                {
                    AddingAt++;
                    string[] SplittedCmds = EndLinesCPL[AddingAt].Split(';');
                    EndLinesCPL[AddingAt] = SplittedCmds[0] + ";" + SplittedCmds[1] + ";" + SplittedCmds[2] + ";" + SplittedCmds[3] + ";" + comboBox2.Text + ";";

                    SpawnGridCPL();
                }*/
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            int AddingAt = -1;
            if (dataGridView1.GetCellCount(DataGridViewElementStates.Selected) > 0)
            {
                AddingAt = dataGridView1.SelectedCells[0].RowIndex + 1;
            }

            if (AddingAt != -1)
            {
                string[] SplittedCmds = EndLinesBOM[AddingAt].Split(';');

                string Search1 = SplittedCmds[2];
                string Search2 = SplittedCmds[3];

                bool FirstFound = true;
                string AddedDesignator = "";
                int NewQuantity = 1;

                for (int i = 1; i < EndLinesBOM.Count; i++)
                {
                    string[] SplittedCmdsInner = EndLinesBOM[i].Split(';');
                    if (SplittedCmdsInner.Length >= 4)
                    {
                        if (SplittedCmdsInner[2] == Search1 && SplittedCmdsInner[3] == Search2)
                        {
                            if (FirstFound)
                            {
                                FirstFound = false;
                            }
                            else
                            {
                                AddedDesignator += ", " + SplittedCmdsInner[1];
                                NewQuantity++;
                                EndLinesBOM.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                }

                for (int i = 1; i < EndLinesBOM.Count; i++)
                {
                    string[] SplittedCmdsInner = EndLinesBOM[i].Split(';');
                    if (SplittedCmdsInner.Length >= 4)
                    {
                        if (SplittedCmdsInner[2] == Search1 && SplittedCmdsInner[3] == Search2)
                        {
                            EndLinesBOM[i] = NewQuantity + ";" + SplittedCmdsInner[1] + AddedDesignator + ";";
                            for (int i2 = 2; i2 < SplittedCmdsInner.Length; i2++) EndLinesBOM[i] += SplittedCmdsInner[i2] + ";";
                        }
                    }
                }

                SpawnGridBOM();
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {
            LoadFile(true);
        }

        private void label6_Click(object sender, EventArgs e)
        {
            LoadFile(false);
        }

        private void lSCSPartsListFromBOMcsvToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                string[] AllLines = File.ReadAllLines(openFileDialog1.FileName);

                if (AllLines.Length > 0)
                {
                    bool DoingPartsList2 = false;
                    bool AlreadyConverted2 = false;
                    bool CanGo = true;

                    for (int i = 0; i < AllLines.Length; i++)
                    {
                        if (i == 0)
                        {
                            if (AllLines[i].Contains(";") || AllLines[i].Contains("Footprint")) DoingPartsList2 = true;
                            if (AllLines[i].Contains("Designator")) AlreadyConverted2 = true;
                        }

                        if (CanGo)
                        {
                            if (AlreadyConverted2)
                            {
                                string ThisRemadeLine = "";

                                if (DoingPartsList2)
                                {
                                    //5,"D2, D3, D4, D5, D8",LED-0603,
                                    string CuttingLine = AllLines[i];
                                    while (CuttingLine.Contains(","))
                                    {
                                        bool HasComma = false;
                                        if (CuttingLine[0] == '\"') HasComma = true;

                                        if (!HasComma) ThisRemadeLine += CuttingLine.Substring(0, CuttingLine.IndexOf(","));
                                        else ThisRemadeLine += CuttingLine.Substring(1, CuttingLine.IndexOf("\",") - 1);
                                        ThisRemadeLine += ";";


                                        if (!HasComma) CuttingLine = CuttingLine.Substring(CuttingLine.IndexOf(",") + 1);
                                        else CuttingLine = CuttingLine.Substring(CuttingLine.IndexOf("\",") + 2);
                                    }

                                    string[] SplittedCmdsInner = ThisRemadeLine.Split(';');
                                    if (SplittedCmdsInner.Length >= 5)
                                    {
                                        string Search1 = SplittedCmdsInner[2];
                                        string Search2 = SplittedCmdsInner[3];
                                        string LSCS_Part = SplittedCmdsInner[4];
                                        SearchAddLSCS(Search1, Search2, LSCS_Part);
                                    }
                                }
                            }
                        }
                    }

                    SpawnGridBOM();
                }
            }
        }

        private void SearchAddLSCS(string Search1, string Search2, string LSCS_Part)
        {
            for (int i = 1; i < EndLinesBOM.Count; i++)
            {
                string[] SplittedCmdsInner = EndLinesBOM[i].Split(';');
                if (SplittedCmdsInner.Length >= 4)
                {
                    if (SplittedCmdsInner[2] == Search1 && SplittedCmdsInner[3] == Search2)
                    {
                        EndLinesBOM[i] = SplittedCmdsInner[0] + ";" + SplittedCmdsInner[1] + ";" + SplittedCmdsInner[2] + ";" + SplittedCmdsInner[3] + ";" + LSCS_Part + ";";
                    }
                }
            }
        }

        private void dataGridView1_Validated(object sender, EventArgs e)
        {
            //UpdateBOMLines();
        }

        private void UpdateBOMLines()
        {
            EndLinesBOM.Clear();
            EndLinesBOM.Add("Qty;Designator;Footprint;Comment;LCSC");

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                string RemadeLine = "";
                for (int i2 = 0; i2 < dataGridView1.Columns.Count; i2++)
                {
                    if (i2 < dataGridView1.Rows[i].Cells.Count)
                    {
                        try
                        {
                            RemadeLine += dataGridView1.Rows[i].Cells[i2].Value.ToString() + ";";
                        }
                        catch
                        {
                            //RemadeLine += ";";
                        }
                    }
                    else
                    {
                        //RemadeLine += ";";
                    }
                }
                EndLinesBOM.Add(RemadeLine);
            }
        }

        private void dataGridView1_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            //if (!Loading) UpdateBOMLines();
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (!Loading) UpdateBOMLines();
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            for (int i = 0; i < dataGridView2.Rows.Count; i++)
            {
                try
                {
                    if (ItemSelectedInThisRow(dataGridView2, i))
                    {
                        int OriginalAngleValue = int.Parse(dataGridView2.Rows[i].Cells[4].Value.ToString());

                        OriginalAngleValue = OriginalAngleValue + 90;
                        if (OriginalAngleValue > 270) OriginalAngleValue = 0;

                        int AddingAt = i + 1;
                        string[] SplittedCmds = EndLinesCPL[AddingAt].Split(';');
                        EndLinesCPL[AddingAt] = SplittedCmds[0] + ";" + SplittedCmds[1] + ";" + SplittedCmds[2] + ";" + SplittedCmds[3] + ";" + (OriginalAngleValue).ToString() + ";";

                        dataGridView2.Rows[i].Cells[4].Selected = true;
                        //SpawnGridCPL();
                    }
                }
                catch { }
            }
            SpawnGridCPL();
        }

        bool ItemSelectedInThisRow(DataGridView ThidGrid, int RowIndex)
        {
            bool IsSelected = false;
            for (int i = 0; i < ThidGrid.Columns.Count; i++)
            {
                if (ThidGrid.Rows[RowIndex].Cells[i].Selected) IsSelected = true;
            }
            return IsSelected;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dataGridView2.Rows.Count; i++)
            {
                try
                {
                    if (ItemSelectedInThisRow(dataGridView2, i))
                    {
                        int OriginalAngleValue = int.Parse(dataGridView2.Rows[i].Cells[4].Value.ToString());

                        OriginalAngleValue = OriginalAngleValue - 90;
                        if (OriginalAngleValue < 0) OriginalAngleValue = 270;

                        int AddingAt = i + 1;
                        string[] SplittedCmds = EndLinesCPL[AddingAt].Split(';');
                        EndLinesCPL[AddingAt] = SplittedCmds[0] + ";" + SplittedCmds[1] + ";" + SplittedCmds[2] + ";" + SplittedCmds[3] + ";" + (OriginalAngleValue).ToString() + ";";

                        //SpawnGridCPL();
                    }
                }
                catch { }
            }
            SpawnGridCPL();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Form2 Form2_0 = new Form2();
            DialogResult result = Form2_0.ShowDialog();
            if (result == DialogResult.OK)
            {
                for (int i = 0; i < dataGridView2.Rows.Count; i++)
                {
                    if (ItemSelectedInThisRow(dataGridView2, i))
                    {
                        double TestVal = -9999.9;
                        try
                        {
                            TestVal = double.Parse(dataGridView2.Rows[i].Cells[1].Value.ToString());
                        }
                        catch
                        {
                            try
                            {
                                TestVal = double.Parse(dataGridView2.Rows[i].Cells[1].Value.ToString().Replace(".", ","));
                            }
                            catch { }
                        }
                        if (TestVal != -9999.9)
                        {
                            int AddingAt = i + 1;
                            string[] SplittedCmds = EndLinesCPL[AddingAt].Split(';');
                            EndLinesCPL[AddingAt] = SplittedCmds[0] + ";" + (TestVal + Form2_0.IncreaserVal).ToString().Replace(",", ".") + ";" + SplittedCmds[2] + ";" + SplittedCmds[3] + ";" + SplittedCmds[4] + ";";

                            //SpawnGridCPL();
                        }
                    }
                }
                SpawnGridCPL();
            }

            if (Form2_0 != null)
            {
                try
                {
                    Form2_0.Dispose();
                }
                catch { }
                Form2_0 = null;
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Form2 Form2_0 = new Form2();
            DialogResult result = Form2_0.ShowDialog();
            if (result == DialogResult.OK)
            {
                for (int i = 0; i < dataGridView2.Rows.Count; i++)
                {
                    if (ItemSelectedInThisRow(dataGridView2, i))
                    {
                        double TestVal = -9999.9;
                        try
                        {
                            TestVal = double.Parse(dataGridView2.Rows[i].Cells[2].Value.ToString());
                        }
                        catch
                        {
                            try
                            {
                                TestVal = double.Parse(dataGridView2.Rows[i].Cells[2].Value.ToString().Replace(".", ","));
                            }
                            catch { }
                        }
                        if (TestVal != -9999.9)
                        {
                            int AddingAt = i + 1;
                            string[] SplittedCmds = EndLinesCPL[AddingAt].Split(';');
                            EndLinesCPL[AddingAt] = SplittedCmds[0] + ";" + SplittedCmds[1] + ";" + (TestVal + Form2_0.IncreaserVal).ToString().Replace(",", ".") + ";" + SplittedCmds[3] + ";" + SplittedCmds[4] + ";";

                            //SpawnGridCPL();
                        }
                    }
                }
                SpawnGridCPL();
            }

            if (Form2_0 != null)
            {
                try
                {
                    Form2_0.Dispose();
                }
                catch { }
                Form2_0 = null;
            }
        }
    }
}
