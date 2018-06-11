using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    static class Distribution
    {
        static private Random rand;

        static Distribution()
        {
            rand = new Random();
        }

        static public int UniformIntZeroTo(int upper)
        {
            return rand.Next(upper);
        }

        static public double SampleStandardNormal()
        {
            double u1 = 1.0 - rand.NextDouble();
            double u2 = 1.0 - rand.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            
        }
        /// <summary>
        /// Uses the Box-Mueller transform to generate a N(0,1) random number, then addapts it to make it N(mu,S2)
        /// </summary>
        /// <returns></returns>
        static public double SampleNormal(double mean, double StdDev)
        {            
            return mean + StdDev * SampleStandardNormal();
        }
    }
}
