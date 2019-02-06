using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    static class FitnessFunctions
    {
        /// <summary>
        /// Returns a negative value so that small Cmax are wanted by HC LS.
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public static double MeanBasedCmax(Schedule S)
        {
            return - S.EstimateCmax();
        }

        public static double NormalApproxCmax(Schedule S)
        {
            return -RobustnessMeasures.NormalBasedEstimatedCmax(S, 0.3).Mean;
        }

      
        
    }
}
