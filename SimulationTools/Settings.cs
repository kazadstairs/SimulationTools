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
        public static int MLS_SCHEDS = 1;
        public static string MLS_AH = "Random";
        public static string MLS_HF = "DetCmax";

        public static bool ON_LAPTOP = true;
        public static bool PAUSE_ON_COMPLETION = true;
        public static bool DEBUG = false;



        public static bool PERFORM_MLS = true;
        public static bool PERFORM_SIMULATIONS = false;
        public static int N_SIM_RUNS = 300; // Number of runs in the simulation;
        public static int[] PI_IDS_TO_USE = { 0, 1, 2, 3, 4, 5, 6, 7, 8 };

        public static string[] DISTRIBUTION = { //"N(p,1)", //Normal, with sigma 1
                                                 //"N(p,0.01p)", //Normal, sigma is 0.01*p
                                                   "N(p,0.1p)",
                                                   "N(p,0.3p)", // Guido´s normal.
                                                   "Exp(p)", //Exponential with mean p
                                                 //"LN(p,0.01p)", //LogNormal distribution, sigma is 0.01p
                                                   "LN(p,0.1p)", //LogNormal distribution, sigma is 0.1p
                                                 "LN(p,0.3p)" //LogNormal distribution, sigma is 0.25p Variation is HUUGE. Don't use this.
        };
    }
}
