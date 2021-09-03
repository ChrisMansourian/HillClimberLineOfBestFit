using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HillClimberLineBestFit
{
    public partial class Form1 : Form
    {
        List<Point> points = new List<Point>();
        Graphics gfx;
        Bitmap canvas;
        double slope = 0;
        double yIntercept = 0;
        public Form1()
        {
            InitializeComponent();
        }

        public void DrawAxis()
        {
            gfx.DrawLine(Pens.Black, canvas.Width / 2, 0, canvas.Width / 2, canvas.Height);
            gfx.DrawLine(Pens.Black, 0, canvas.Height / 2, canvas.Width, canvas.Height / 2);
        }

        public void DrawPoints()
        {
            foreach (var point in points)
            {
                gfx.FillEllipse(Brushes.Red, point.X - 1 + canvas.Width / 2, -point.Y - 1 + canvas.Height / 2, 2, 2);
            }
        }

        public void DrawLine()
        {
            //gfx.DrawLine(new Pen(Color.Green, 1), 0, (int)(-(canvas.Width / 2 * slope) - yIntercept), canvas.Width / 2, (int)(canvas.Height / 2 - yIntercept));
            gfx.DrawLine(new Pen(Color.Green, 1), 0, -(int)(slope * (-(canvas.Width) / 2))-(int)yIntercept+canvas.Height/2, canvas.Width, -(int)(slope * ((canvas.Width)/2))+canvas.Height/2-(int)yIntercept); 
        }

        public void Update()
        {
            gfx.Clear(Color.White);
            DisplayPoints();
            DrawAxis();
            DrawPoints();
            DrawLine();
            pictureBox1.Image = canvas;
        }

        public void DisplayPoints()
        {
            PointsLabel.Text = "Points: ";
            foreach (var point in points)
            {
                PointsLabel.Text += $"\nX: {point.X} Y {point.Y}";
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            points.Clear();
            canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            gfx = Graphics.FromImage(canvas);
            Update();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (int.TryParse(XTextBox.Text, out int x) && int.TryParse(YTextBox.Text, out int y))
            {
                points.Add(new Point(x, y));
                XTextBox.Clear();
                YTextBox.Clear();
                Update();
            }
        }

        private void Clear_Click(object sender, EventArgs e)
        {
            points.Clear();
            DisplayPoints();
        }

        private void CalculateBtn_Click(object sender, EventArgs e)
        {
            double ErrorThreshold = 0;
            foreach (var point in points)
            {
                ErrorThreshold += point.Y;
            }
            ErrorThreshold = Math.Abs(ErrorThreshold / 1);
            double currentError = double.MaxValue;
            double[] currentMutation = new double[2];
            int timesSinceErrorChange = 0;
            while (currentError > ErrorThreshold)
            {
                timesSinceErrorChange = 0;
                currentMutation = new double[2];
                currentError = CalculateError(currentMutation[0], currentMutation[1]);
                while (timesSinceErrorChange < 25)
                {
                    currentMutation = Mutate(currentMutation, currentError);
                    double newError = CalculateError(currentMutation[0], currentMutation[1]);
                    if (newError < currentError)
                    {
                        timesSinceErrorChange = 0;
                        currentError = newError;
                    }
                    else
                    {
                        timesSinceErrorChange++;
                    }
                }
            }

            slope = currentMutation[0];
            yIntercept = currentMutation[1];
            Equation.Text = $"Equation: y = {slope}x + {yIntercept}";

            Update();

        }

        Random gen = new Random();
        public double[] Mutate(double[] current, double currentError)
        {

            int index = gen.Next(0, current.Length);

            double amount = .1;
            if (gen.Next(0, 2) == 1)
            {
                amount = -1;
            }
            current[index] += amount;
            current[index] = Math.Round(current[index], 2);
            double newError = CalculateError(current[0], current[1]);
            if (newError < currentError)
            {
                return current;
            }
            current[index] -= amount;
            current[index] = Math.Round(current[index], 2);
            return current;
        }

        public double CalculateError(double slope, double yIntercept)
        {
            double totalError = 0;
            foreach (var point in points)
            {
                totalError += Math.Abs(point.Y - (slope * point.X + yIntercept));
            }
            return totalError / points.Count;
        }
    }
}
