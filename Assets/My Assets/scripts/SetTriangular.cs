using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.My_Assets.scripts
{
    class SetTriangular : FuzzySet
    {
        private double a, b, c;
        public override double f(double x)
        {
            if (x < a || x > c) return 0.0;
            if (a <= x && x <= b) return (x - a) / (b - a);
            if (b <= x && x <= c) return (c - x) / (c - b);
            return -1;
        }
        public SetTriangular(String label,double a,double b, double c)
        {
            this.label = label;
            this.a = a;
            this.b = b;
            this.c = c;
        }
    }
}
