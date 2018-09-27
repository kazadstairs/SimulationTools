using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    static class Constants
    {
        public static string OUTPATH;
       // public static int NumberOfEvents = 100;
        public static int NRuns = 150; // Number of runs in the simulation;

        public static double PermittedIncreaseForPunctuality = 0.05; // Within how much delay a job is still considered on time. E.g. if set to 0.1, a job that starts before 1.1*sj is considered on time.
        public static double DEFAULT_RM = 0.0;

        // public static int DISTRIBUTIONTYPE = 1;

        public static string[] DISTRIBUTION = { //"N(p,1)", // Normal, with sigma 1
   //                                                "N(p,0.01p)", //Normal, sigma is 0.01*p
                                                   //"N(p,0.1p)",
   //                                                "N(p,0.25p)",
                                                   "N(p,0.30p)", // Guido´s normal.
                                                   "Exp(p)", //Exponential with mean p
   //                                                "LN(p,0.01p)", //LogNormal distribution, sigma is 0.01p
                                                   //"LN(p,0.1p)", //LogNormal distribution, sigma is 0.1p
   //                                                "LN(p,0.25p)" //LogNormal distribution, sigma is 0.25p Variation is HUUGE. Don't use this.
        };


        public static string[] RMNames = { "FS", // Sum of free slacks
                                           "BFS", // Sum of Binary free slack
                                           "UFS", // Sum of Upperbound free slack
                                           "wFS", //Sum of Weighted sum of free slacks
                                           "TS", // Sum of Total Slack
                                           "BTS", // Sum of Binary Total Slack
                                           "UTS", // Sum of Upperbound Total Slack
                                           "wTS", // NSucc weigthed total Slack
                                           "SDR" // Sum of Slack Duration Ratio (TS/pj)
        };
        public static string[] QMNames = { "Cmax",
                                         //  "90%ProjectCertainty", DONE in R
                                         //  "ExtensionTo90%", DONE in R
                                         //  "RelativeExtentionTo90%", DONE in R
                                         //  "Cmax - MeanBasedCmax", DONE in R
                                           "LinearStartDelay",
                                           "Start Punctuality",
                                           "Finish Punctuality"
        };
    }
}
