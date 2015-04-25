using System;

namespace Assets.My_Assets.scripts.my_scripts
{
    class SetTrapezoid : FuzzySet{
        double a, b, c, d;
        public override double  f(double x)
        {
            if (x < a || x > d) return 0.0;
            if (b <= x && x <= c) return 1.0;
            if (a <= x && x <= b) return (x - a) / (b - a);
            if (c <= x && x <= d) return (d - x) / (d - c);
            return -1;

        }
        public SetTrapezoid(String label,double a,double b,double c,double d)
        {
            this.label = label;
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }
        public SetTrapezoid()
        {
        }
    }
}
