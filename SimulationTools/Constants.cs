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
        public static string[] RMNames = { "FS", // Sum of free slacks
                                           "BFS", // Binary free slack
                                           "UFS" // Upperbound free slack
        };
        public static string[] QMNames = { "Cmax",
                                           "LinearStartDelay",
                                           "Start Punctuality",
                                           "Finish Punctuality"
        };
    }
}
