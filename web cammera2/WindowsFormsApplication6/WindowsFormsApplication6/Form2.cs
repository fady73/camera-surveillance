using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

namespace WindowsFormsApplication6
{
    public partial class Form2 : Form
    {
        Image<Bgr, Byte> Original;
        HaarCascade Haar;
        private static string link = "";
       
        public Form2(String l)
        {
            link = l;
            InitializeComponent();
            Haar = new HaarCascade("haarcascade_frontalface_default.xml");

         //   stream1 = new MJPEGStream(link);
         //stream1.NewFrame += stream_Newframe;

           // stream1.Start();
        }
       
        /*void stream_Newframe(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap bmp = (Bitmap)eventArgs.Frame.Clone();
            Original = new Image<Bgr, byte>(bmp);

            if (Original != null)
            {
                Image<Gray, byte> Grayframe = Original.Convert<Gray, byte>();
                var faces = Grayframe.DetectHaarCascade(Haar, 1.2, 4, HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(pictureBox1.Height / 5, pictureBox1.Width / 5))[0];

                foreach (var face in faces)
                {
                    Original.Draw(face.rect, new Bgr(Color.Red), 2);
                }
            }

            pictureBox1.Image = Original.Bitmap;

        }
        */
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }
    }
}
