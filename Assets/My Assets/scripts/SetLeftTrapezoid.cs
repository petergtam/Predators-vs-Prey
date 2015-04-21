using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.My_Assets.scripts
{
    class SetLeftTrapezoid: FuzzySet
    {
        double a, b;
        public override double f(double x)
        {
            if (x > b) return 0.0;
            if (x < a) return 1.0;
            if (a <= x && x <= b) return (b - x) / (b - a);
            return -1;
        }
        public SetLeftTrapezoid(String label,double a,double b)
        {
            this.label = label;
            this.a = a;
            this.b = b;
        }
    }
}
