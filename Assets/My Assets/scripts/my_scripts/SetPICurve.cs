using System;

namespace Assets.My_Assets.scripts.my_scripts
{
    class SetPICurve : FuzzySet
    {
        double a, b, c, d;
        public override double f(double x)
        {
            double S = -1.0;
            double Z = -1.0;
            if (x > b) S = 1.0;
            if (x < a) S = 0.0;
            if (a <= x && x <= b) S = 0.5 * (1 + Math.Cos(((x - b) / (b - a)) * Math.PI));
            if (x > d) Z = 0.0;
            if (x < c) Z = 1.0;
            if (c <= x && x <= d) Z = 0.5 * (1 + Math.Cos(((x - c) / (d - c)) * Math.PI));
            return Math.Min(S, Z);
        }
        public SetPICurve(String label,double a,double b, double c,double d)
        {
            this.label = label;
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }

    }
}
