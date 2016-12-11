﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;
 /////////////
namespace LPhotoEditor
{
    public partial class Form1 : Form
    {
        //основная картинка
        internal Bitmap img;
        //картинка второго буфера для двойной буферизации
        private Bitmap _back;
        //выделенная область
        private Rectangle selectedRegion;
        //GDI буфра
        Graphics _buffer;

        //нажата ли мышь
        private bool MousePressed;
        //отпущена ли мышь
        private bool MouseReleased = true;
        //точка нажатия мыши
        private Point clickPoint;
        //Графический буфер для двойной буферизации
        private BufferedGraphics _backBuffer;
        private BufferedGraphicsContext _backBufferContext;

        public Form1()
        {
            selectedRegion = new Rectangle();
            InitializeComponent();

            
        }



        private void button1_Click(object sender, EventArgs e)
        {
            //Создадим окно открытия файла
            var win = new OpenFileDialog { Filter = @"Изображения (*.png,*.jpeg,*.bmp,*.dib)|*.png;*.jpeg;*.bmp;*.dib" };
            if (win.ShowDialog() != DialogResult.OK)
            {
                win.Dispose();
                return;
            }
            var a = win.FileName;
            //Попробуем открыть его на экране. Если не получится...
            try
            {
                //загрузим изображение и обновим окно в соответствии с ним
                img = (Bitmap)Image.FromFile(a);
                InitDrawRegion(img.Width, img.Height);
                pictureBox1.Width = img.Width;
                pictureBox1.Height = img.Height;
                pictureBox1.Image = img;
                CropBtn.Location = new Point(img.Width + 10, CropBtn.Location.Y + 30);
                button2.Location = new Point(img.Width + 10, CropBtn.Location.Y + 30);
                button3.Location = new Point(img.Width + 10, button2.Location.Y + 30);
                button4.Location = new Point(img.Width + 10, button3.Location.Y + 30);
                button6.Location = new Point(img.Width + 10, button4.Location.Y + 30);

                ReInitDrawRegion();
                _backBuffer.Render(_buffer);
                pictureBox1.Refresh();
            }
            catch (Exception)
            {
                //...выведем сообщение об ошибке.
                MessageBox.Show(@"Ошибка открытия файла");
                win.Dispose();

            }
            //Освободим ресурс.
            win.Dispose();

        }


        //корректирует размеры окна
        void InitDrawRegion(int w, int h)
        {
            Size = new Size(w+140,h+150);
        }

        //обработчик нажатия на кнопку обрезать
        private void CropBtn_Click(object sender, EventArgs e)
        {
            //если нет изображения или ничего не выделено - ничего не делаем
            if(pictureBox1.Image == null || selectedRegion == Rectangle.Empty)
                return;

            //обновим изображение и все буферы
            img = (Bitmap)cropAtRect(img, selectedRegion);
            pictureBox1.Width = img.Width;
            pictureBox1.Height = img.Height;
            ReInitDrawRegion();
            InitDrawRegion(img.Width, img.Height);

            _backBuffer.Render(_buffer);
            pictureBox1.Refresh();
            UpdateImage(img);

        }

        //обрабочик нажатия мыши
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            //если нет изображения - ничего не делаем
            if (img == null)
                return;
            MousePressed = true;
            MouseReleased = false;
            //установим точку начала в точку нажатия мыши
            selectedRegion.X = e.X;
            selectedRegion.Y = e.Y;
            clickPoint = new Point(e.X,e.Y);
            
        }
        
        //обработчик отпускания мыши
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            //если нет изобаржения ничего не делаем
            if (img == null)
                return;
            //обновимм область выделения
            selectedRegion.Width = e.X;
            selectedRegion.Height = e.Y;
            MousePressed = false;
            MouseReleased = true;
            //отрисуем во второй буфер прямоугольник
            _backBuffer.Graphics.DrawImage(img, 0, 0);
            selectedRegion = Rect(this, clickPoint.X, clickPoint.Y, e.X, e.Y, _backBuffer.Graphics);
            pictureBox1.Refresh();

        }

        //обработчик перемещения мыши
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            //если нет изображения то ничего не делаем
            if(img == null)
                return;
            //если кнопка мыши не была нажата тоже ничего не делаем
            if (!MousePressed && MouseReleased) return;
            //отрисуем в буфер прямоугольник
            _backBuffer.Graphics.DrawImage(img, 0,0);
            Rect(this, clickPoint.X, clickPoint.Y, e.X, e.Y, _backBuffer.Graphics);
            pictureBox1.Refresh();
        }

        //рисует квадрат по любым четырем точкам
        static Rectangle Rect(Form1 form1, int x, int y, int sx, int sy, Graphics g)
        {
            Rectangle a = new Rectangle();
            if (x >= sx && y >= sy)
            {
                a = new Rectangle(sx, sy, x - sx, y - sy);

            }
            else if (x < sx && y >= sy)
            {
                a = new Rectangle(x, sy, sx - x, y - sy);
            }
            else if (x >= sx && y < sy)
            {
                a = new Rectangle(sx, y, x - sx, sy - y);
            }
            else if (x < sx && y < sy)
            {
                a= new Rectangle(x, y, sx - x, sy - y);

            }
            if (a.Width == 0 && a.Height == 0)
            {
                return Rectangle.Empty;
            }
             g.DrawRectangle(Pens.Black, a);
            return a;
        }

        //инициализация поверхности рисования и графических буферов
        void ReInitDrawRegion()
        {
            _back = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            _buffer = Graphics.FromImage(_back);
            _backBufferContext = BufferedGraphicsManager.Current;
            _backBufferContext.MaximumBuffer = new Size(pictureBox1.Width + 1, pictureBox1.Height + 1);
            _backBuffer = _backBufferContext.Allocate(_buffer, new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height));


            _backBuffer.Graphics.DrawImage(img, 0, 0);
            
        }

        //обработчик отрисовки
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if(_backBuffer != null)
            _backBuffer.Render(e.Graphics);

        }
        //функция обрезки
        public static Image cropAtRect(Image b, Rectangle r)
        {
            Bitmap nb = new Bitmap(r.Width, r.Height);
            Graphics g = Graphics.FromImage(nb);
            g.DrawImage(b, -r.X, -r.Y);
            return nb;
        }

        //обработчик поворота изображения
        private void button2_Click(object sender, EventArgs e)
        { 

        }
        //функция масштабирования изображения в хорошем качестве
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        //обновление изображения, размеров поля рисования и положения кнопок
        private void UpdateImage(Bitmap imgs)
        {
            img = imgs;
            ReInitDrawRegion();
            InitDrawRegion(imgs.Width, imgs.Height);
            pictureBox1.Width = img.Width;
            pictureBox1.Height = img.Height;
            _backBuffer.Graphics.DrawRectangle(Pens.Black, selectedRegion);
            CropBtn.Location = new Point(img.Width + 10, CropBtn.Location.Y );
            button2.Location = new Point(img.Width + 10, CropBtn.Location.Y + 30);
            button3.Location = new Point(img.Width + 10, button2.Location.Y + 30);
            button4.Location = new Point(img.Width + 10, button3.Location.Y + 30);
            button6.Location = new Point(img.Width + 10, button4.Location.Y + 30);
            pictureBox1.Refresh();
        }

        //функция, вызываемая из формы изменения размера
        public void resize(int w, int h)
        {
           var a = ResizeImage(img, w, h);
           UpdateImage(a);
        }

        //функция вызова формы изменения размера
        private void button3_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
                return;
            var f = new ResizeForm(this);
            f.Show();
        }


        //кнопка фокус
        private void button4_Click(object sender, EventArgs e)
        {
      
        }

        //кнопка сохранения
        private void button5_Click(object sender, EventArgs e)
        {
        }

        //Кнопка черно-белое
        private void button6_Click(object sender, EventArgs e)
        {
            
        }
    }
}
