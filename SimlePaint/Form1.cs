using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace SimlePaint
{

    public partial class Form1 : Form
    {
        Bitmap bmp;
        Graphics g;
        bool isPainting = false;
        Pen pen;
        Pen erase;
        Point pt1, pt2;
        PaintType curType=PaintType.None;
        ColorDialog colorDialog = new ColorDialog();

        public enum PaintType
        {
            None,
            Pencil,
            Fill,
            Eraser,
            Circle,
            Rect,
            Line

        }

        public Form1()
        {
            
            InitializeComponent();
            pen = new Pen(Color.Black, 1);
            erase=new Pen(Color.White, 1);
            cur_color.BackColor= Color.Black;
            bmp=new Bitmap(pic.Size.Width,pic.Size.Height);
            g = Graphics.FromImage(bmp);//This method returns a new Graphics for the specified Image
            g.Clear(Color.White);
            pic.Image = bmp;
            Text = "Paint";
          
           
        }

     
       

        private void pic_MouseDown(object sender, MouseEventArgs e)
        {
            isPainting = true;
            pt1 = e.Location;
        }

        private void pic_MouseMove(object sender, MouseEventArgs e)
        {
            pic.Refresh();
            if (isPainting)
            {
                if (curType == PaintType.Eraser)
                {
                    pic.Cursor = Cursors.NoMove2D;
                    pt2 = e.Location;
                    g.DrawLine(erase, pt1, pt2);
                    pt1 = pt2;
                }
                else
                {
                    pic.Cursor = Cursors.Arrow;
                    if (curType == PaintType.Pencil)
                    {
                        pt2 = e.Location;
                        g.DrawLine(pen, pt1, pt2);
                        pt1 = pt2;

                    }
                }
              
               

            }
           
        }

        private void pic_MouseUp(object sender, MouseEventArgs e)
        {
            isPainting = false;
            pt2 = e.Location;
            if (curType == PaintType.Circle)
            {
            
                g.DrawEllipse(pen, new Rectangle(pt1, new Size(pt2.X - pt1.X, pt2.Y - pt1.Y)));
     
            }
            if(curType==PaintType.Rect)
            {
                int x = Math.Min(pt1.X, pt2.X);
                int y = Math.Min(pt1.Y, pt2.Y);
                int width = Math.Abs(pt2.X - pt1.X);
                int height = Math.Abs(pt2.Y - pt1.Y);
                g.DrawRectangle(pen, new Rectangle(x, y, width, height));
   
            }
            if(curType==PaintType.Line)
            {
                g.DrawLine(pen, pt1, pt2);
            }
            pic.Refresh();
           
        }

        private void btn_pencil_Click(object sender, EventArgs e)
        {
            curType = PaintType.Pencil;
        }

        private void btn_eraser_Click(object sender, EventArgs e)
        {
            curType = PaintType.Eraser;
            
          
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            pen.Width = trackBar1.Value;
            erase.Width = trackBar1.Value;
   
        }

        private void btn_eraser_Leave(object sender, EventArgs e)
        {
            pic.Cursor = Cursors.Arrow;


        }

        private void btn_circle_Click(object sender, EventArgs e)
        {
            curType = PaintType.Circle;
        }

        private void btn_rectangle_Click(object sender, EventArgs e)
        {
            curType = PaintType.Rect;

        }

        private void btn_color_Click(object sender, EventArgs e)
        {
            colorDialog.ShowDialog();
            pen.Color = colorDialog.Color;
            cur_color.BackColor = pen.Color;
        }
        private Point getPoint(PictureBox pb, Point p)
        {
            float x = 1f * pb.Image.Width / pb.Width;
            float y = 1f * pb.Image.Height / pb.Height;

            return new Point((int)(x * p.X), (int)(y * p.Y));

        }
        private void color_picker_MouseClick(object sender, MouseEventArgs e)
        {

            Point colorPickerClicked = getPoint(color_picker ,e.Location);
            Color newColor = ((Bitmap)color_picker.Image).GetPixel(colorPickerClicked.X, colorPickerClicked.Y);
            if (newColor != Color.FromArgb(30, 30, 30))
            {
                pen.Color = newColor;
                cur_color.BackColor = pen.Color;
            }
        }


        public void Fill(Bitmap bm, int x, int y, Color new_clr)
        {
            Color old_color = bm.GetPixel(x, y);
            Stack<Point> pixel = new Stack<Point>(); 
            pixel.Push(new Point(x, y));
            bm.SetPixel(x, y, new_clr);
            if (old_color == new_clr) return;
            while (pixel.Count > 0)
            {   
                Point pt = (Point)pixel.Pop();
                if (pt.X > 0 && pt.Y > 0 && pt.X < bm.Width - 1 && pt.Y < bm.Height - 1)
                {
                    validate(bm, pixel, pt.X - 1, pt.Y, old_color, new_clr);
                    validate(bm, pixel, pt.X, pt.Y - 1, old_color, new_clr); 
                    validate(bm, pixel, pt.X + 1, pt.Y, old_color, new_clr); 
                    validate(bm, pixel, pt.X, pt.Y + 1, old_color, new_clr);
                }
            }
        }

        private void validate(Bitmap bm, Stack<Point> sp, int x, int y, Color old_color, Color new_color)
        {
            Color cx = bm.GetPixel(x, y);
            if (cx == old_color)
            {
                sp.Push(new Point(x, y));
                bm.SetPixel(x, y, new_color);
            }
        }

        private void pic_MouseClick(object sender, MouseEventArgs e)
        {
            if(PaintType.Fill==curType)
            {
                Point point= getPoint(pic, e.Location); 
                Fill(bmp, point.X, point.Y, pen.Color);
            }
        }  

        private void btn_fill_Click(object sender, EventArgs e)
        {
            curType= PaintType.Fill;
        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            var saveDig = new SaveFileDialog();
            saveDig.Filter = "JPEG files (*.jpeg)|*.jpeg|JPG files (*.jpg)|*.jpg|All files (*.*)|*.*";
            string ext = Path.GetExtension(saveDig.FileName);
            if (saveDig.ShowDialog() == DialogResult.OK)
            {
                string ext1 = Path.GetExtension(saveDig.FileName);
                if (ext1.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) || ext1.Equals(".jpg", StringComparison.OrdinalIgnoreCase))
                {
                    bmp.Save(saveDig.FileName, ImageFormat.Jpeg);
                }
                else
                {
                    MessageBox.Show("Invalid file format. Please select a JPEG or JPG file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("A simple paint application Created by Mohammad Amin Ghasemi\nContact me: mohammadgh9080@gmail.com");
        }

        private void btn_line_Click(object sender, EventArgs e)
        {
            curType = PaintType.Line;
        }
    }
}
