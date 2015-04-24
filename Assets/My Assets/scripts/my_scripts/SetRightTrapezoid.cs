using System;

namespace Assets.My_Assets.scripts.my_scripts
{
    class SetRightTrapezoid : FuzzySet
    {
        double a, b;
        public override double f(double x)
        {
            if (x > b) return 1.0;
            if (x < a) return 0.0;
            if (a <= x && x <= b) return (x - a) / (b - a);
            return -1;

        }
        public SetRightTrapezoid(String label,double a,double b)
        {
            this.label = label;
            this.a = a;
            this.b = b;
        }

    }
}
