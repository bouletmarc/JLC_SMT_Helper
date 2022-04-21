using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace JLC_SMT_Helper
{

    public partial class Form2 : Form
    {
        public double IncreaserVal = 0.0;

        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                IncreaserVal = double.Parse(textBox1.Text);
                this.Close();
            }
            catch 
            {
                MessageBox.Show("Unable to parse text '" + textBox1.Text + "' to a number value!");
                //DialogResult = DialogResult.None;
                button1.DialogResult = DialogResult.None;
            }
        }
    }
}
