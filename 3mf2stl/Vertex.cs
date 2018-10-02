using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Vertex
    {
        public double x, y, z;

        string value(double v)
        {
            return String.Format("{0:F16}", v);
        }

        public string X()
        {
            return value(x);
        }

        public string Y()
        {
            return value(y);
        }

        public string Z()
        {
            return value(z);
        }

        public Vertex subtract(Vertex right)
        {
            var temp = new Vertex();

            temp.x = x - right.x;
            temp.y = y - right.y;
            temp.z = z - right.z;

            return temp;
        }

        public Vertex cross(Vertex right)
        {
            var temp = new Vertex();

            temp.x = y * right.z - z * right.y;
            temp.y = z * right.x - x * right.z;
            temp.z = x * right.y - y * right.x;

            return temp;
        }

        double self_dot()
        {
            return x * x + y * y + z * z;
        }

        public double length()
        {
            return Math.Sqrt(self_dot());
        }

        public void normalize()
        {
            var len = length();

            if (len != 0)
            {
                x /= len;
                y /= len;
                z /= len;
            }
        }
    }
}
