using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QQLibrary
{
    public partial class FormVCode : Form
    {
        private FormVCode()
        {
            InitializeComponent();
        }
        public FormVCode(Bitmap bmp)
        {
            InitializeComponent();
            pictureBox1.BackgroundImageLayout = ImageLayout.Center;
            //pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.Image = bmp;
        }

        private void FormVCode_Load(object sender, EventArgs e)
        {
        }
        public string GetCode()
        {
            return textBox1.Text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
