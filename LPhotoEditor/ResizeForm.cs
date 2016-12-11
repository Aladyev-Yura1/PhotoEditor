using System;
using System.Windows.Forms;
using System.Threading;

namespace LPhotoEditor
{
    public partial class ResizeForm : Form
    {
        //ссылка на главную форму, из нее вызовем ресайз
        private Form1 mainform;
        private float ratio;
        private int w, h;
        public ResizeForm(Form1 f)
        {
            mainform = f;
            InitializeComponent();
            //получим ширину и высоту изображения сразу в текстбоксы
            textBox1.Text = f.img.Width.ToString();
            textBox2.Text = f.img.Height.ToString();
            //посчитаем соотношения сторон для мастабирования с пропорциями
            ratio = (float)f.img.Width/f.img.Height;
        }
        //обработчик изменения текста в тексбоксе ширина
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            //работает только если само текстовое поле в фокусе. Это нужно чтобы изобежать циклического вызова между двумя полями
            if (textBox1.Focused)
            {
                try
                {
                    w = Int32.Parse(textBox1.Text);

                }
                catch (Exception)
                {
                    return;
                }
                if (checkBox1.Checked)
                {
                    //если стоит галка, обновим второе поле
                    textBox2.Text = Math.Round(w/ratio).ToString();
                }
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            //работает только если само текстовое поле в фокусе. Это нужно чтобы изобежать циклического вызова между двумя полями

            if (textBox2.Focused)
            {
                try
                {
                    h = Int32.Parse(textBox2.Text);
                }
                catch
                {
                    return;
                }
                if (checkBox1.Checked)
                {
                    //если стоит галка, обновим второе поле

                    textBox1.Text = Math.Round(ratio*h).ToString();
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            //при постановке галки обновим значения второго поля
            try
            {
                w = Int32.Parse(textBox1.Text);

            }
            catch (Exception)
            {
                return;
            }
                
                if (checkBox1.Checked)
                {
                    textBox2.Text = Math.Round(w / ratio).ToString();
                }
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //кнопка окей
            try
            {
                w = Int32.Parse(textBox1.Text);
                h = Int32.Parse(textBox2.Text);

            }
            catch (Exception)
            {
                //если введены неверные данные
                MessageBox.Show(@"Введите только целые числа");
                return;
            }
            w = Int32.Parse(textBox1.Text);
            mainform.resize(w, h);
            Thread.Sleep(2);
            mainform.resize(w, h);
        }

       
    }
}
