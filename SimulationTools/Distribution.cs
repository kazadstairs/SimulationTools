using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    class Distribution
    {
        public double Mean;
        public double Variation;

        public Distribution()
        { }

        public Distribution(double mean, double variation)
        {
            Mean = mean;
            Variation = variation;
        }
    }



    class ZeroDistribution : Distribution
    {
        public ZeroDistribution() { Mean = 0.0; Variation = 0.0; }
    }

    class ConstantAsDistribution : Distribution { public ConstantAsDistribution(double Constant) { Mean = Constant; Variation = 0.0; } }

}
