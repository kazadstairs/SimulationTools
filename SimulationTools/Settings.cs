using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    public static class Settings
    {
        public static int MLS_RUNS = 10;
        public static int MLS_SCHEDS = 2;
        public static string MLS_AH = "Random";
        public static string MLS_HF = "NormalApproxCmax"; //DetCmax or NormalApproxCmax

        public static double RM_UFSandBFS_fraction = 0.30;

        public static bool ON_LAPTOP = true;
        public static bool PAUSE_ON_COMPLETION = true;
        public static bool DEBUG = false;

        public static string RESULTSFILENAME = "Placeholder.txt";

        public static double PermittedIncreaseForPunctuality = 0.0; // Within how much delay a job is still considered on time. E.g. if set to 0.1, a job that starts before 1.1*sj is considered on time.

        public static bool PERFORM_MLS = true; //CURRENTLY SET TO USE FITNESSFUNCTION NormalApproxCmax
        public static bool PERFORM_SIMULATIONS = false;
        public static int N_SIM_RUNS = 300; // Number of runs in the simulation;
        public static bool INCLUDEJOBINFO = false;
        public static bool RMs_USE_MEANS = true;
      //  public static int[] PI_IDS_TO_USE = { 0, 1, 2, 3, 4, 5, 6, 7, 8 , 9, 10 ,11};
        public static string[] PROBLEM_INSTANCES_TO_USE =
        {
            "30j-15r-4m.ms",
            "30j-15r-8m.ms",
            "30j-30r-4m.ms",
            "30j-30r-8m.ms",
            "30j-75r-4m.ms",
            "30j-75r-8m.ms",
            "100j-50r-6m.ms",
            "100j-50r-12m.ms",
            "100j-100r-6m.ms",
            "100j-100r-12m.ms",
            "100j-250r-6m.ms",
            "100j-250r-12m.ms",
        };

        public static string[] DISTRIBUTION = { //"N(p,1)", //Normal, with sigma 1
                                                 //"N(p,0.01p)", //Normal, sigma is 0.01*p
                                                  // "N(p,0.1p)",
                                                   "N(p,0.3p)", // Guido´s normal.
                                                 //  "Exp(p)" //Exponential with mean p
                                                 //"LN(p,0.01p)", //LogNormal distribution, sigma is 0.01p
                                                 //  "LN(p,0.1p)", //LogNormal distribution, sigma is 0.1p
                                               //  "LN(p,0.3p)" //LogNormal distribution, sigma is 0.25p Variation is HUUGE. Don't use this.
        };
    }
}
