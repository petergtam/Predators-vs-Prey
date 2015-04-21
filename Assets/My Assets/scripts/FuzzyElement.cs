using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.My_Assets.scripts 
{
	public class FuzzyElement
    {
        private List<FuzzySet> sets;
        public const int FUNCTION_CENTROID = 1; //set with centroid closer to value
        public const int FUNCTION_BISECTOR = 2; //set with bisector closer to value
        public const int FUNCTION_MAX = 3; // set which f(val) is maximized
        public const int FUNCTION_MIN = 4; // set which f(val) is minimized

        public FuzzyElement()
        {
            sets = new List<FuzzySet>();
        }
        public void addTriangular(String label,double a,double b, double c)
        {
            sets.Add(new SetTriangular(label,a,b,c));
        }
        public void addTrapezoid(String label,double a,double b,double c,double d)
        {
            sets.Add(new SetTrapezoid(label,a,b,c,d));
        }
        public void addLeftTrapezoid(String label,double a,double b){
            sets.Add(new SetLeftTrapezoid(label,a,b));   
        }
        public void addRightTrapezoid(String label,double a,double b)
        {
            sets.Add(new SetRightTrapezoid(label,a,b));
        }
        public void addSCurve(String label,double a,double b)
        {
            sets.Add(new SetSCurve(label,a,b));
        }
        public void addZCurve(String label,double a,double b)
        {
            sets.Add(new SetZCurve(label,a,b));
        }
        public void addPICurve(String label,double a,double b,double c,double d)
        {
            sets.Add(new SetPICurve(label,a,b,c,d));
        }
        public FuzzySet getByMax(double val)
        {
            return this.unfuzzyfy(val, FUNCTION_MAX);
        }
        public FuzzySet getByMin(double val)
        {
            return this.unfuzzyfy(val, FUNCTION_MIN);
        }
        private FuzzySet unfuzzyfy(double value, int function)
        {
            FuzzySet result = null;
            double best = 0.0;
            foreach( FuzzySet set in sets )
            {
                double setValue = 0;
                switch (function)
                {
                    case FUNCTION_MAX:
                    case FUNCTION_MIN:
                        setValue = set.f(value);
                        break;
                }
                switch (function)
                {
                    case FUNCTION_MIN:
                        if (setValue < best)
                        {
                            best = setValue;
                            result = set;
                        }
                        break;
                    case FUNCTION_MAX:
                        if (setValue > best)
                        {
                            best = setValue;
                            result = set;
                        }
                        break;
                }
            }
            return result;
        }
    }
}
