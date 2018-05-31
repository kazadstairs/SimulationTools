using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    class Distribution
    {
        private Random rand;

        public Distribution()
        {
            rand = new Random();
        }

        public double SampleStandardNormal()
        {
            double u1 = 1.0 - rand.NextDouble();
            double u2 = 1.0 - rand.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            
        }
        /// <summary>
        /// Uses the Box-Mueller transform to generate a N(0,1) random number, then addapts it to make it N(mu,S2)
        /// </summary>
        /// <returns></returns>
        public double SampleNormal(double mean, double StdDev)
        {            
            return mean + StdDev * SampleStandardNormal();
        }
    }
}
