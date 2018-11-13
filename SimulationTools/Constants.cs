﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    static class Constants
    {
        public static string ALLRESULTSPATH;
       // public static int NumberOfEvents = 100;
        public static bool INCLUDEJOBINFO = false;  // true: include start and completion times for each job for each simulation run in output.
                                                    // false: do not include start and completion times for each job in simulation run.

        public static double PermittedIncreaseForPunctuality = 0.05; // Within how much delay a job is still considered on time. E.g. if set to 0.1, a job that starts before 1.1*sj is considered on time.
        public static double DEFAULT_RM = 0.0;

        // public static int DISTRIBUTIONTYPE = 1;

       


        public static string[] RMNames = { "FS", // Sum of free slacks
                                           "BFS", // Sum of Binary free slack
                                           "UFS", // Sum of Upperbound free slack
                                           "wFS", //Sum of Weighted sum of free slacks
                                           "TS", // Sum of Total Slack
                                           "BTS", // Sum of Binary Total Slack
                                           "UTS", // Sum of Upperbound Total Slack
                                           "wTS", // NSucc weigthed total Slack
                                           "SDR", // Sum of Slack Duration Ratio (TS/pj)
                                           "DetCmax", //Deterministic Cmax
                                           "NormalApproxCmax" // Approximation of expected makespan based on assumption that jobs are normally distributed
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
