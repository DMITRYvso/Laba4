using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Laba4
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void OpenForm1(bool readOnly)
        {
            try
            {
                Form1 form1;

                if (readOnly)
                {
                    form1 = new Form1(true);
                }
                else
                {
                    form1 = new Form1();
                }

                form1.Show();
                this.Hide();

                form1.FormClosed += (s, args) =>
                {
                    this.Show();
                    this.Activate();
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии формы: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            OpenForm1(false);
        }
    }
}