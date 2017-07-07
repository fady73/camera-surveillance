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
    struct classification
    {
        public PictureBox container;
        public Image myimg;
        
        public classification(PictureBox a, Image b)
        {
            container = a;
            myimg = b;
        }

    };

    public partial class Form1 : Form
    {
       
        MySqlConnection con;
        MySqlCommand cmd;
        DateTime thisDay = DateTime.Today;

        String date1;
        String time1,h,m,s;
        Image<Gray, byte> result;

        public delegate Bitmap mynewframe(object b);
        
        HaarCascade Haar;
        HaarCascade Haar2;
        HaarCascade Haar3;
        System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"alarm.wav");


        Image<Bgr, Byte> currentFrame;
        int a, b, c, d;

        Thread th1, th2, th3, th4;
        private static string h1,h2,h3,h4;
        bool isvideosrc = false;
        bool s1, s2, s3, s4;
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        List<string> labels = new List<string>();
        int counter=0;
        MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 1.5d, 1.5d);
        private void aDDFacesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addfaces f = new addfaces();
            f.Show();
            this.Hide();
            
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        public Form1()
        {
            InitializeComponent();

        }
       
        private void Form1_Load(object sender, EventArgs e)
        {
            con = new MySqlConnection("server=localhost;database=test;uid=root;pwd=root");
            this.FormClosed += new FormClosedEventHandler(f_FormClosed);
            label1.Visible = false;
            label2.Visible = false;
            label3.Visible = false;
            label4.Visible = false;
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
            load_databases();

        }


        private void load_databases()
        {
            con.Open();
            cmd = new MySqlCommand("select * from face", con);
            MySqlDataReader de = cmd.ExecuteReader();
            while (de.Read())
            {
                byte[] x = (byte[])de["image"];
                MemoryStream xx = new MemoryStream(x);
                Bitmap myimg= (Bitmap)Image.FromStream(xx);
                trainingImages.Add(new Image<Gray, byte>(myimg));
                labels.Add(de["name"].ToString());
                counter++;
            }

            con.Close();
        }

        void repeat1(PictureBox p, string url,int framenum)
        {

            while (true)
            {
                framenum++;
                WebRequest requestPic = WebRequest.Create(url);
                WebResponse responsePic = requestPic.GetResponse();




                if (framenum % 3 == 0)
                {
                    classification s = new classification(p, Image.FromStream(responsePic.GetResponseStream()));

                    lock (this)
                    {


                        Thread.Sleep(3);

                        mynewframe dm = new mynewframe(loadmyvideo);
                        IAsyncResult m = dm.BeginInvoke(s, null, null);
                        p.Image = dm.EndInvoke(m);

                    }

                }
                else
                {
                    p.Image = Image.FromStream(responsePic.GetResponseStream());
                }
             
                
            }
        }
        
        private Bitmap loadmyvideo(Object s)
        {
            classification n = (classification)s;
            
            currentFrame= new Image<Bgr, Byte>((Bitmap)n.myimg);
            if (currentFrame != null)
            {
                Image<Gray, byte> Grayframe = currentFrame.Convert<Gray, byte>();//.Flip(Emgu.CV.CvEnum.FLIP.HORIZONTAL);
                var faces = Grayframe.DetectHaarCascade(Haar, 1.2, 8, HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(pictureBox1.Height / 10, pictureBox1.Width / 10))[0];
                var faces2 = Grayframe.DetectHaarCascade(Haar2, 1.2, 8, HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(pictureBox1.Height / 10, pictureBox1.Width / 10))[0];
                var faces3 = Grayframe.DetectHaarCascade(Haar3, 1.2, 8, HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(pictureBox1.Height / 10, pictureBox1.Width / 10))[0];
                bool x = false,y=false;
              
                    foreach (var face in faces)
                    {


                       currentFrame.Draw(face.rect, new Bgr(Color.Red), 2);
                       result = currentFrame.Copy(face.rect).Convert<Gray, byte>().Resize(50, 50, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
    
                        x = true;
                     }
                if (x)
                {
                    String name = "";
                    foreach (MCvAvgComp f in faces)
                    {
                        if (trainingImages.ToArray().Length != 0)
                        {
                            MCvTermCriteria termCrit = new MCvTermCriteria(counter, .001);
                            EigenObjectRecognizer recognizer = new EigenObjectRecognizer(trainingImages.ToArray(), labels.ToArray(), 2000, ref termCrit);
                            name = recognizer.Recognize(result);
                            currentFrame.Draw(name, ref font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.GreenYellow));
                            if (name.Equals("unknown"))
                            {
                                
                                player.Play();
                                

                            }

                        }
                    }
                }

                
                if (!x)
                { 
                
                    foreach (var face in faces2)
                    {
                        currentFrame.Draw(face.rect, new Bgr(Color.Red), 2);
                        result = currentFrame.Copy(face.rect).Convert<Gray, byte>().Resize(50, 50, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                   
                        y = true;
                    }
                    if (y)
                    {
                        String name1 = "";
                        foreach (MCvAvgComp f in faces2)
                        {
                            if (trainingImages.ToArray().Length != 0)
                            {
                                MCvTermCriteria termCrit = new MCvTermCriteria(counter, .001);
                                EigenObjectRecognizer recognizer = new EigenObjectRecognizer(trainingImages.ToArray(), labels.ToArray(), 2000, ref termCrit);
                                name1 = recognizer.Recognize(result);
                                currentFrame.Draw(name1, ref font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.GreenYellow));
                                if (name1.Equals("unknown"))
                                {
                                    //System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"alarm.wav");
                                   player.Play();
                                   

                                }

                            }
                        }

                    }
                    bool z = false;
                    if (!y)
                    {
                        foreach (var face in faces3)
                        {
                            currentFrame.Draw(face.rect, new Bgr(Color.Red), 2);
                            result = currentFrame.Copy(face.rect).Convert<Gray, byte>().Resize(50, 50, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                            z = true;
                            
                        }
                    }
                    if (z)
                    {
                        String name = "";
                        foreach (MCvAvgComp f in faces3)
                        {
                            if (trainingImages.ToArray().Length != 0)
                            {
                                MCvTermCriteria termCrit = new MCvTermCriteria(counter, .001);
                                EigenObjectRecognizer recognizer = new EigenObjectRecognizer(trainingImages.ToArray(), labels.ToArray(), 2000, ref termCrit);
                                name = recognizer.Recognize(result);
                                currentFrame.Draw(name, ref font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.GreenYellow));
                                if (name.Equals("unknown"))
                                {
                                  //  System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"alarm.wav");
                                    player.Play();
                                 
                                    
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

        private void timer1_Tick(object sender, EventArgs e)
        {
            date1 = Convert.ToString(thisDay.Day + "/" + thisDay.Month + "/" + thisDay.Year);
            h = Convert.ToString(DateTime.Now.Hour);
            m= Convert.ToString(DateTime.Now.Minute);
            s = Convert.ToString(DateTime.Now.Second);
            time1 = h + ":" + m + ":" + s;
            label1.Text = "day:"+date1 +" time:"+time1;
            label2.Text = "day:" + date1 + " time:" + time1;
            label3.Text = "day:" + date1 + " time:" + time1;
            label4.Text = "day:" + date1 + " time:" + time1;

        }
       
        private void button1_Click(object sender, EventArgs e)
        {
           
            h1 = "http://" + textBox1.Text + ":8080/shot.jpg";
            h2 = "http://" + textBox2.Text + ":8080/shot.jpg";
            h3 = "http://" + textBox3.Text + ":8080/shot.jpg";
            h4 = "http://" + textBox4.Text + ":8080/shot.jpg";

           

            if (textBox1.Text != "")
            {
                isvideosrc = true;
                s1 = true;
                a = 1;
                // start load frames whuch h1 is url for ip webcam1
                th1 = new Thread(() => repeat1(pictureBox1, h1,a));
                th1.Start();
                textBox1.Visible = false;
                label1.Visible = true;
            }
           if (textBox2.Text != "")
            {
                isvideosrc = true;
                s2 = true;
                b = 1;
                th2 = new Thread(() => repeat1(pictureBox2, h2,b));
                th2.Start();
                textBox2.Visible = false;
                label2.Visible = true;

            }
            if (textBox3.Text != "")
            {
                isvideosrc = true;
                s3 = true;
                c = 1;
                th3 = new Thread(() => repeat1(pictureBox3, h3,c));
                th3.Start();
                textBox3.Visible = false;
                label3.Visible = true;

            }

            if (textBox4.Text != "")
            {
                isvideosrc = true;
                s4 = true;
                d = 1;
                th4 = new Thread(() => repeat1(pictureBox4, h4,d));
                th4.Start();
                textBox4.Visible = false;
                label4.Visible = true;

            }

            
            if (!isvideosrc)
            {
                MessageBox.Show("please enter ip for camera");
            }
          
        }
        private void f_FormClosed(object sender, EventArgs e)
        {
            MessageBox.Show("thank u for using our app");
            player.Stop();
            if (s1)
            {
                th1.Abort();
                textBox1.Visible = true;
                textBox1.Text = null;
                pictureBox1.Image = null;
                label1.Visible = false;

            }
            if (s2)
            {
                th2.Abort();
                textBox2.Visible = true;
                textBox2.Text = null;
                pictureBox2.Image = null;
                label2.Visible = false;
            }
            if (s3)
            {
                th3.Abort();
                textBox3.Visible = true;
                textBox3.Text = null;
                pictureBox3.Image = null;
                label3.Visible = false;
            }
            if (s4)
            {
                th4.Abort();
                textBox4.Text = null;
                textBox4.Visible = true;
                pictureBox4.Image = null;
                label4.Visible = false;
            }


        }



       

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {  
        }
       private void pictureBox2_Click_1(object sender, EventArgs e)
        {
           
        }
        private void pictureBox3_Click_1(object sender, EventArgs e)
        {
            
        }
        private void pictureBox4_Click_1(object sender, EventArgs e)
        {
        }
    }
}
