using System;

namespace Assets.My_Assets.scripts.my_scripts
{
    class SetSCurve : FuzzySet
    {
        double a, b;
        public override double f(double x)
        {
            if (x > b) return 1.0;
            if (x < a) return 0.0;
            if (a <= x && x <= b) return 0.5 * (1 + Math.Cos(((x - b) / (b - a)) * Math.PI));
            return -1;
        }
        public SetSCurve(String label, double a, double b)
        {
            this.label = label;
            this.a = a;
            this.b = b;
        }

    }
}
