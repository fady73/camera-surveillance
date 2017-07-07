using System;
using System.Drawing;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Net;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Threading;
using MySql.Data.MySqlClient;
namespace WindowsFormsApplication6
{
    public partial class addfaces : Form
    {
       
        private static string h1;
        Thread th;
        public delegate Bitmap mynewframe(object b);
        Image<Bgr, Byte> currentFrame;
        bool startthread;
        HaarCascade Haar;
        HaarCascade Haar2;
        HaarCascade Haar3;
        int framenum = 1;
        List<Image> trainingset; 
        bool startsaving = false;
        Image<Gray, byte> result;
        public addfaces()
        {
            InitializeComponent();

        }
        private void f_FormClosed(object sender, EventArgs e)
        {

            if (startthread == true)
                th.Abort();

            Application.Exit();
        }

        private void addfaces_Load(object sender, EventArgs e)
        {
           
            button4.Visible = false;
            label1.Visible = false;
            textBox2.Visible = false;
            this.FormClosed += new FormClosedEventHandler(f_FormClosed);
            button1.Visible = false;
            button2.Visible = false;
            try
            {
                Haar = new HaarCascade("haarcascade_frontalface_alt_tree.xml");
                Haar2 = new HaarCascade("haarcascade_frontalface_default.xml");

                Haar3 = new HaarCascade("haarcascade_profilefaced.xml");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }


        }
       
        void repeat()
        {     
            while (true)
            {
                framenum++;
                WebRequest requestPic = WebRequest.Create(h1);
                WebResponse responsePic = requestPic.GetResponse();
                if (framenum % 3 == 0)
                {
                        mynewframe dm = new mynewframe(loading);
                        IAsyncResult m = dm.BeginInvoke(Image.FromStream(responsePic.GetResponseStream()), null, null);
                        pictureBox1.Image = dm.EndInvoke(m);

                }
                else
                {
                    pictureBox1.Image = Image.FromStream(responsePic.GetResponseStream());
                }


            }
        }
        private Bitmap loading(object e)
        {
            Image myimg = (Image)e;
            currentFrame = new Image<Bgr, Byte>((Bitmap)myimg);
            if (currentFrame != null)
            {
                Image<Gray, byte> Grayframe = currentFrame.Convert<Gray, byte>();//.Flip(Emgu.CV.CvEnum.FLIP.VERTICAL);
                var faces = Grayframe.DetectHaarCascade(Haar, 1.2, 8, HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(pictureBox1.Height / 10, pictureBox1.Width / 10))[0];
                var faces2 = Grayframe.DetectHaarCascade(Haar2, 1.2, 8, HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(pictureBox1.Height / 10, pictureBox1.Width / 10))[0];
                var faces3 = Grayframe.DetectHaarCascade(Haar3, 1.2, 8, HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(pictureBox1.Height / 10, pictureBox1.Width / 10))[0];
                Bitmap bmpinput = Grayframe.ToBitmap();
                bool x = false, y = false;  
                foreach (var face in faces)
                {
                    currentFrame.Draw(face.rect, new Bgr(Color.Red), 2);
                    if (startsaving)
                    {
                        
                      result = currentFrame.Copy(face.rect).Convert<Gray, byte>().Resize(50, 50, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                      pictureBox2.Image = result.Bitmap;
                        try
                        {
                            byte[] bArr = imgToByteConverter(result.Bitmap);
                            insert_img(bArr,textBox2.Text);
                            
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                    }
                    x = true;
                }
                if (!x)
                {

                    foreach (var face in faces2)
                    {
                      
                        currentFrame.Draw(face.rect, new Bgr(Color.Red), 2);
                        y = true;
                        if (startsaving)
                        {
                           
                           result = currentFrame.Copy(face.rect).Convert<Gray, byte>().Resize(50, 50, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                            pictureBox2.Image = result.Bitmap;
                            try
                            {
                                byte[] bArr = imgToByteConverter(result.Bitmap);
                                insert_img(bArr,textBox2.Text);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.ToString());
                            }
                        }
                    }

                    if (!y)
                    {
                        foreach (var face in faces3)
                        {
                           
                            currentFrame.Draw(face.rect, new Bgr(Color.Red), 2);
                            if (startsaving)
                            {
                
                               result = currentFrame.Copy(face.rect).Convert<Gray, byte>().Resize(50, 50, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                               pictureBox2.Image = result.Bitmap;
                                try
                                {
                                    byte[] bArr = imgToByteConverter(result.Bitmap);
                                    insert_img(bArr,textBox2.Text);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.ToString());
                                }
                            }
                           
                        }
                    }
                }
            }
            return currentFrame.Bitmap;

        }
        public static byte[] imgToByteConverter(Image inImg)
        {
            ImageConverter imgCon = new ImageConverter();
            return (byte[])imgCon.ConvertTo(inImg, typeof(byte[]));
        }
        private void button3_Click(object sender, EventArgs e)
        {
            h1 = "http://" + textBox1.Text + ":8080/shot.jpg";

            th = new Thread(() => repeat());
            th.Start();
            startthread = true;
            button1.Visible = true;
            button2.Visible = true;
            textBox1.Visible = false;
            button3.Visible = false;
            button4.Visible = true;
            label1.Visible = true;
            textBox2.Visible = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (startthread == true)
                th.Abort();
            Form1 f = new Form1();
            f.Show();
            this.Dispose();

            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox2.Text =="")
            {

                MessageBox.Show("please enter name of person");

            }
            else
            {
                startsaving = true;
                trainingset = new List<Image>();
           
            }
            

        }

        public void insert_img(byte[] x,String name)
        {

            MySqlConnection con = new MySqlConnection("server=localhost;database=test;uid=root;pwd=root");
            con.Open();
            byte [] img =x;
            
            MySqlCommand cmd;
          
           
            cmd = new MySqlCommand("insert into face values(@p1,@p2)", con);
            cmd.Parameters.Add("@p1", MySqlDbType.VarChar, 255);
            cmd.Parameters.Add("@p2", MySqlDbType.Blob);
            cmd.Parameters["@p1"].Value = name;
            cmd.Parameters["@p2"].Value = img;
            cmd.ExecuteNonQuery();
            con.Close();
        }
        private void button4_Click(object sender, EventArgs e)
        {
            
            startsaving = false;
           
            textBox2.Text = null;
            MessageBox.Show("thank u u can save another persons");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            
            
        }
    }
}
