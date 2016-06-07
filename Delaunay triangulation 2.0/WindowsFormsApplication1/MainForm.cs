using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Delaunay_triangulation;

namespace WindowsFormsApplication1
{
    public partial class MainForm : Form
    {
        DelaunayTriangulation delaunay; 
        List<Triangule> triangulated = new List<Triangule>();
        Dictionary<Triangule, Circle> triangulatedWithCircles = new Dictionary<Triangule, Circle>();
        Random r = new Random();
        Triangule selectedTriangule = null;
        List<Point2D> points = new List<Point2D>();
        Point2D tempPoint = new Point2D();
        bool released = true;
        bool moved = false;
                
        public MainForm()
        {
            InitializeComponent();
        }

        public void DrawLines(List<Triangule>triangules,Graphics g)
        {
            foreach (Triangule t in triangules)
            {
                g.DrawLine(Pens.Blue, t.a.x, t.a.y, t.b.x, t.b.y);
                g.DrawLine(Pens.Blue, t.b.x, t.b.y, t.c.x, t.c.y);
                g.DrawLine(Pens.Blue, t.c.x, t.c.y, t.a.x, t.a.y);
                if (selectedTriangule == t)
                {
                    Circle c=delaunay.GetCircumcicles(new List<Triangule>() { selectedTriangule})[t];
                    float x = c.center.x - c.radius;
                    float y = c.center.y - c.radius;
                    g.DrawEllipse(Pens.GreenYellow, x, y, 2 * c.radius, 2 * c.radius);
                }
                if (selectedTriangule != null) continue;
                if (checkBox1.Checked)
                {
                    if (triangulatedWithCircles.ContainsKey(t))
                    {
                        Circle c = triangulatedWithCircles[t];
                        float x = c.center.x - c.radius;
                        float y = c.center.y - c.radius;
                        g.DrawEllipse(Pens.GreenYellow, x, y, 2 * c.radius, 2 * c.radius);
                    }
                }
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(pictureBox1.BackColor);
            if (triangulated!=null && triangulated.Count > 0)
                DrawLines(triangulated, e.Graphics);
            if(points.Count > 0)
                foreach(Point2D p in points)
                {
                    e.Graphics.DrawEllipse(Pens.Black,p.x,p.y,2,2);
                }
        }

        private void button1_Click(object sender, EventArgs e)//generar puntos aleatorios
        {
            Random randomizer = new Random();
            int count = randomizer.Next(25) + 3;  
            points.Clear();
            for (int i = 0; i < count; i++)
            {
                points.Add(new Point2D(r.Next(0,pictureBox1.Width),r.Next(0,pictureBox1.Height)));
            }
            selectedTriangule = null;
            Refresh();
        }

        private void button2_Click(object sender, EventArgs e)//triangular
        {
            if (points.Count > 2)
            {
                selectedTriangule = null;
                delaunay = new DelaunayTriangulation(points);
                triangulated = delaunay.Triangulate();
                if (checkBox1.Checked)
                    triangulatedWithCircles = delaunay.GetCircumcicles(triangulated);
                Refresh();
            }
        }
        
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode==Keys.Z)
            {
                if (points.Count > 0)
                {
                    points.RemoveAt(points.Count - 1);
                    delaunay = new DelaunayTriangulation(points);
                    triangulated = delaunay.Triangulate();
                    if (checkBox1.Checked)
                        triangulatedWithCircles = delaunay.GetCircumcicles(triangulated);
                    Refresh();
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            points.Clear();
            selectedTriangule = null;
            if(triangulated != null)
                triangulated.Clear();
            Refresh();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (released)
                    released = false;
                points.Add(new Point2D(e.X, e.Y));
                Refresh();
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                released = true;
                if (!moved && checkBox2.Checked)
                { button2_Click(null, null); }
                else { Refresh(); moved = false; }
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (selectedTriangule == null)
                {
                    Rect rab, rbc, rca;
                    foreach (Triangule t in triangulated)
                    {
                        rab = delaunay.GetRectEcuation(t.a, t.b);
                        rbc = delaunay.GetRectEcuation(t.b, t.c);
                        rca = delaunay.GetRectEcuation(t.c, t.a);
                        if ((rab.Eval(e.X) <= e.Y && rbc.Eval(e.X) >= e.Y && rca.Eval(e.X) <= e.Y) || ((rab.Eval(e.X) >= e.Y && rbc.Eval(e.X) <= e.Y && rca.Eval(e.X) >= e.Y)))
                        {
                            selectedTriangule = t;
                            break;
                        }
                    }
                }
                else { selectedTriangule = null; }
                Refresh();
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!released && e.Button == MouseButtons.Left)
            {
                moved = true;
                points.RemoveAt(points.Count - 1);
                points.Add(new Point2D(e.X, e.Y));
                if (checkBox2.Checked)
                    button2_Click(null, null);
                else Refresh();
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (triangulated != null && triangulated.Count > 0)
                delaunay.SaveTriangulesInFEFormat(triangulated);
            else MessageBox.Show("No existen triángulos a salvar en formato FE");
        }
    }
}
