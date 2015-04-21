using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.My_Assets.scripts
{
    public abstract class FuzzySet
    {
       
        protected String label;
        /*
        private double _centroid;
        private double _bisector;
        public double centroid()
        {
            return this._centroid;
        }
        public double bisector()
        {
            return this._bisector();
        }
        */
        //Abstract functions, each set should implement 
        public abstract double f(double val);
        //public abstract double setCentroid();
        //public abstract double setBisector();
        public FuzzySet()
        {
        }
        public FuzzySet(String label)
        {
            this.label = label;
        }
        public String name()
        {
            return this.label;
        }
        
    }
}

