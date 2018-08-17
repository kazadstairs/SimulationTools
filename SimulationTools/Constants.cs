using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    static class Constants
    {
        public static int NumberOfEvents = 1000;
        public static double DEFAULT_RM = 0.0;
        public static string[] RMNames = { "SoFS" // Sum of free slacks
        };
        public static string[] QMNames = { "Cmax",
                                           "LinearStartDelay",
                                           "Start Punctuality",
                                           "Finish Punctuality"
        };
    }
}
