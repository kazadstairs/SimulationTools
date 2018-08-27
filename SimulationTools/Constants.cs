using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    static class Constants
    {
        public static string OUTPATH;
        public static int NumberOfEvents = 100;
        public static int NRuns = 30; // Number of runs in the simulation;
        public static double PermittedIncreaseForPunctuality = 0.05; // Within how much delay a job is still considered on time. E.g. if set to 0.1, a job that starts before 1.1*sj is considered on time.
        public static double DEFAULT_RM = 0.0;
        public static string[] RMNames = { "FS", // Sum of free slacks
                                           "BFS", // Binary free slack
                                           "UFS", // Upperbound free slack
                                           "wFS" //Weighted sum of free slacks
        };
        public static string[] QMNames = { "Cmax",
                                           "LinearStartDelay",
                                           "Start Punctuality",
                                           "Finish Punctuality"
        };
    }
}
