using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Delaunay_triangulation
{
    public class DelaunayTriangulation
    {
        List<Point2D> points;//lista de todos los puntos
        List<Point2D> leftOverPoints=new List<Point2D>();//lista de puntos que faltan por triangular
        List<Point2D> pointsUsed = new List<Point2D>();//lista de puntos triangulados
        internal List<Triangule> res = new List<Triangule>();//lista con los triangulos a devolver por el metodo Triangulate
        public DelaunayTriangulation(List<Point2D>points)//este es el constructor
        {
            this.points = points;
            for (int i = 0; i < points.Count; i++)
            {
                leftOverPoints.Add((Point2D)points[i].Clone());
            }
        }

        public List<Triangule> Triangulate()
        {
            res.Clear();
            Triangule root = new Triangule();//este es el primer triangulo
            Pair pair;
            List<Point2D> leftOverPointsTemp = new List<Point2D>(leftOverPoints);
            List<Point2D> leftOverPoints2 = new List<Point2D>(leftOverPoints);
            foreach (Point2D[] p in GetRandomPoints())//la idea es: de todas los trios de puntos posibles, me quedo con el primero de esos trios tal que cumpla la condicion de Delaunay
            {
                if (!HasPointsInside(p[0], p[1], p[2]))
                {
                    root = new Triangule(p[0], p[1], p[2]);
                    res.Add(root);
                    leftOverPointsTemp.Remove(p[0]);
                    leftOverPointsTemp.Remove(p[1]);
                    leftOverPointsTemp.Remove(p[2]);

                    pointsUsed.Add(p[0]);//adiciona a la lista de puntos usados, los puntos que hay en p
                    pointsUsed.Add(p[1]);
                    pointsUsed.Add(p[2]);                    
                    
                    pair = Triangulate(leftOverPointsTemp,res);
                    if (pair.ok)
                        return pair.res;
                    else
                    {
                        res.RemoveAt(0);//quita de la posicion pasada de parametro
                        pointsUsed.RemoveAt(pointsUsed.Count - 1);
                        pointsUsed.RemoveAt(pointsUsed.Count - 1);
                        pointsUsed.RemoveAt(pointsUsed.Count - 1);

                        leftOverPointsTemp = leftOverPoints2;
                    }
                }
            }
            return null;
        }

        private Pair Triangulate(List<Point2D>leftOverPoints,List<Triangule>res)
        {
            List<Triangule> temp=null;
            Point2D newPoint = new Point2D();
            while (leftOverPoints.Count > 0)
            {
                for (int i = 0; i < leftOverPoints.Count; i++)
                {
                    temp = GetNextTriangules(leftOverPoints[i]);
                    if (temp.Count > 0)
                    {
                        res.AddRange(temp);
                        newPoint=leftOverPoints[i];
                        leftOverPoints.RemoveAt(i);
                        pointsUsed.Add(newPoint);
                        Pair p=Triangulate(leftOverPoints, res);
                        if (p.ok)
                            return p;
                        else
                        {
                            res.RemoveRange(res.Count - temp.Count, temp.Count);
                            leftOverPoints.Insert(i, newPoint);
                            pointsUsed.Remove(newPoint);
                        }
                    }
                }
                return new Pair(res,false);
            }
            return new Pair(res,true);
        }

        private List<Triangule> GetNextTriangules(Point2D newPoint)
        {
            List<Triangule> result = new List<Triangule>();
            foreach (Point2D[] p in GetPairRandomPoints())
            {
                if (!p[0].Equals(newPoint) && !p[1].Equals(newPoint))
                {
                    if (!HasPointsInside(p[0],p[1],newPoint))
                    {
                        result.Add(new Triangule(p[0],p[1],newPoint)); 
                    }
                }                
            }
            return result;
        }

        private bool HasPointsInside(Point2D point2D, Point2D point2D_2, Point2D point2D_3)
        {
            List<Triangule> list = new List<Triangule>();
            Triangule t = new Triangule(point2D, point2D_2, point2D_3);
            list.Add(t);
            Dictionary<Triangule, Circle> dict = GetCircumcicles(list);
            foreach (Point2D p in points)
            {
                if (!p.Equals(point2D) && !p.Equals(point2D_2) && !p.Equals(point2D_3))
                {
                    if (Math.Pow(p.x - dict[t].center.x, 2) + Math.Pow(p.y - dict[t].center.y, 2) <= Math.Pow(dict[t].radius, 2))
                        return true;
                }
            }
            return false; 
        }      
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                
        IEnumerable<Point2D[]> GetRandomPoints()
        {	
        	// esta funcion va probando todos los triangulos posibles
            for (int i = 0; i < points.Count; i++)
                for (int j = i + 1; j < points.Count; j++)
                    for (int k = j + 1; k < points.Count; k++)
                        yield return new Point2D[3] { points[i],points[j],points[k]}; //la instruccion "yield" seguida de la instruccion "return" lo que hace es "congelar o parar" la ejecucion de este metodo. Se devuelve lo que diga en el return y se para la ejecucion del metodo hasta que vuelva a ser llamado. Cuando es vuelto a llamar, continua la ejecucion del metodo por donde se habia quedado
        }

        IEnumerable<Point2D[]> GetPairRandomPoints()
        {
            // esta funcion va probando todos las parejas de puntos posibles
            for (int i = 0; i < pointsUsed.Count; i++)
                for (int j = i + 1; j < pointsUsed.Count; j++)
                    yield return new Point2D[2] { pointsUsed[i], pointsUsed[j] };
        }

        public Dictionary<Triangule,Circle> GetCircumcicles(List<Triangule> triangules)
        {
            Dictionary<Triangule,Circle>res=new Dictionary<Triangule,Circle>();
            Rect r1, r2,r3,r4;
            Point2D intersection;
            float radius;
            if(triangules != null)
                foreach (Triangule t in triangules)
                {
                    r1=GetRectEcuation(t.a, t.b);
                    r2 = GetMediatrixEcuation(t.a, t.b,r1);
                    r3 = GetRectEcuation(t.b,t.c);
                    r4 = GetMediatrixEcuation(t.b,t.c,r3);
                    intersection = GetIntersectPoint(r2, r4);
                    radius=(float)GetDistance(t.a, intersection);
                    res[t] = new Circle(radius, (Point2D)intersection.Clone());
                }
            return res;
        }

        public float GetDistance(Point2D p1, Point2D p2)//con la norma euclideana
        {
            return (float)Math.Sqrt(Math.Pow(p1.x - p2.x, 2) + Math.Pow(p1.y - p2.y, 2));
        }

        public Point2D GetIntersectPoint(Rect r1, Rect r2)
        {
            float x = (r2.N - r1.N) / (float)(r1.PendientEval() - r2.PendientEval());
            float y = r1.Eval(x);
            return new Point2D(x,y);
        }

        public Rect GetMediatrixEcuation(Point2D p1, Point2D p2,Rect r)
        {
            Point2D halfPoint = new Point2D((p1.x+p2.x)/2f,(p1.y+p2.y)/2f);
            Rational m = new Rational(r.M.denominator,r.M.numerator * -1);
            return new Rect(m, -1 * m.Eval() * halfPoint.x + halfPoint.y);
        }
        
        public Rect GetRectEcuation(Point2D p1, Point2D p2)
        {
            return new Rect(new Rational((int)(p1.y - p2.y), (int)(p1.x - p2.x)), -1 * p1.x * ((float)(p1.y - p2.y) / (float)(p1.x - p2.x)) + p1.y);
        }

        public void SaveTriangulesInFEFormat(List<Triangule> triangles)
        {
            if (triangles!=null && triangles.Count > 0)
            {
                string[] lineas = new string[triangles.Count * 4];
                for (int i = 0; i < triangles.Count; i++)
                {
                    lineas[(i * 4)] = "Triangle" + Convert.ToString(i + 1);
                    lineas[(i * 4) + 1] = Convert.ToString(triangles[i].a.x) + ' ' + Convert.ToString(triangles[i].a.y);
                    lineas[(i * 4) + 2] = Convert.ToString(triangles[i].b.x) + ' ' + Convert.ToString(triangles[i].b.y);
                    lineas[(i * 4) + 3] = Convert.ToString(triangles[i].c.x) + ' ' + Convert.ToString(triangles[i].c.y);
                }
                File.WriteAllLines("outPut.txt", lineas);
            }
        } 
    }

    public class Rect
    {
        public Rational M;//pendiente de la recta
        public float N;// valor de la y cuando esta recta se intersecta con el eje y
        public Rect() { }
        public Rect(Rational M, float N)
        {
            this.M = M; this.N = N;
        }

        public float PendientEval()
        {	
        	return M.Eval();
        }

        public float Eval(float x)
        {
            return x * PendientEval() + N;
        }
        
    }

    public class Rational
    {
        public int numerator, denominator;
        public Rational(int numerator, int denominator)
        {
            this.numerator = numerator; this.denominator = denominator;
        }

        public float Eval()
        {
            if (denominator == 0) return 0;
            return (float)numerator / (float)denominator;
        }
    }

    public class Circle : ICloneable//de la clase circulo me interesa conocer el punto centro y su radio
    {
        public float radius = 0;
        public Point2D center=new Point2D();

        public Circle() { }//constructor vacio
        public Circle(float radius, Point2D center)//el otro constructor
        {
            this.radius = radius;
            this.center = center;
        }

        #region ICloneable Members

        public object Clone()
        {
            return new Circle(radius, (Point2D)center.Clone());
        }

        #endregion
    }

    public class Point2D:ICloneable
    {
        public float x, y;

        public Point2D() { }

        public Point2D(float x, float y)
        {
            this.x = x; this.y = y;
        }

        public override bool Equals(object obj)
        {
            Point2D other = (Point2D)obj;
            return other.x == x && other.y == y;
        }

        #region ICloneable Members

        public object Clone()
        {
            return new Point2D(x, y);
        }

        #endregion
    }

    public class Triangule:ICloneable
    {
        public Point2D a, b, c;
		       
        bool FindAssign(Point2D a1,Point2D a2,Point2D a3,float low)
        {	
            if (a1.x == low)
            {
                if (a2.x == low)
                {
                    if (a1.y > a2.y)
                    {
                        this.a = a1;
                        this.b = a2;
                        this.c = a3;
                        return true;
                    }
                    else { this.a = a2; this.b = a1; this.c = a3; return true; }
                }
                else if (a3.x == low)
                {
                    if (a1.y > a3.y)
                    {
                        this.a = a1;
                        this.b = a3;
                        this.c = a2;
                        return true;
                    }
                    else { this.a = a3; this.b = a1; this.c = a2; return true; }
                }
                else if (a2.x > a3.x)
                { this.a = a3; this.b = a1; this.c = a2; return true; }
                else { this.a = a2; this.b = a1; this.c = a3; }
            }
            return false;
        }
        
        public Triangule(Point2D a, Point2D b, Point2D c)
        {
            if (a.Equals(b) || a.Equals(c) || b.Equals(c))
                throw new Exception("no pueden haber 2 puntos de un triangulo iguales");
            float low = Math.Min(a.x, b.x);
            low = Math.Min(low, c.x);
            if (!FindAssign(a, b, c,low) && !FindAssign(b, a, c,low))
                FindAssign(c,a,b,low);
        }

        public Triangule() { }

        public override bool Equals(object obj)
        {
            Triangule other = obj as Triangule;
            return other.a.Equals(a) && other.b.Equals(b) && other.c.Equals(c);
        }

        #region ICloneable Members

        public object Clone()
        {
            return new Triangule((Point2D)a.Clone(), (Point2D)b.Clone(), (Point2D)c.Clone());
        }

        #endregion
    }

    class Pair
    {
        public List<Triangule> res;
        public bool ok = false;
        public Pair(List<Triangule>res,bool ok)
        { this.res = res; this.ok = ok; }
    }
}
