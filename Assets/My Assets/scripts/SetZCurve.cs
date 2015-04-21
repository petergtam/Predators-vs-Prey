using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.My_Assets.scripts
{
    class SetZCurve : FuzzySet
    {
        double a, b;
        public override double f(double x)
        {
            if (x > b) return 0.0;
            if (x < a) return 1.0;
            if (a <= x && x <= b) return 0.5 * (1 + Math.Cos(((x - a) / (b - a)) * Math.PI));
            return -1;
        }
        public SetZCurve(String label, double a, double b)
        {
            this.label = label;
            this.a = a;
            this.b = b;
        }

    }
}
