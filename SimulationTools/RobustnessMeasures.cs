﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    static class RobustnessMeasures
    {
        public static double FS(Schedule S)
        {
            return RM(Unweighted, FreeSlackOf, Unmodified,S,0);
        }

        public static double BFS(Schedule S,double fraction)
        {
            return RM(Unweighted, FreeSlackOf, Binary, S, fraction);
        }

        public static double UFS(Schedule S,double fraction)
        {
            return RM(Unweighted, FreeSlackOf, Upperbound, S, fraction);
        }

        public static double wFS(Schedule S)
        {
            return RM(NSucc, FreeSlackOf, Unmodified, S, 0);
        }

        public static double TS(Schedule S)
        {
            return RM(Unweighted, TotalSlackOf, Unmodified, S, 0);
        }

        public static double BTS(Schedule S,double fraction)
        {
            return RM(Unweighted, TotalSlackOf, Binary, S, fraction);
        }

        public static double UTS(Schedule S, double fraction)
        {
            return RM(Unweighted, TotalSlackOf, Upperbound, S, fraction);
        }

        public static double wTS(Schedule S)
        {
            return RM(NSucc, TotalSlackOf, Unmodified, S, 0);
        }

        public static double SDR(Schedule S)
        {
            return RM(Unweighted, SDROf, Unmodified, S, 0);
        }


        // Slack Measures: *********************************
        /// <summary>
        /// The free slack of job j in schedule S is the amount of time j can slip without delaying the start of the very next activity.
        /// </summary>
        /// <param name="j">Job j</param>
        /// <param name="S">Schedule s</param>
        /// <returns></returns>
        /// 
        private static double FreeSlackOf(Job j, Schedule S)
        {
            //TODO: faster implementations must exist.
            if(j.Successors.Count > 0)
            {
                double freeSlack = double.MaxValue;
                double candidate = double.MaxValue;

                foreach (Job succ in j.Successors)
                {
                    candidate = S.GetStartTimeOfJob(succ) - j.MeanProcessingTime - S.GetStartTimeOfJob(j);
                    if (candidate < freeSlack)
                    {
                        freeSlack = candidate;
                    }
                }
                if (freeSlack < 0)
                {
                    throw new Exception("WTF!");
                }
                return freeSlack;
            }
            else
            {
                if (S.EstimatedCmax - j.MeanProcessingTime - S.GetStartTimeOfJob(j) < 0)
                {
                    throw new Exception(string.Format("{0}",S.GetStartTimeOfJob(j)));
                }
                return S.EstimatedCmax - j.MeanProcessingTime - S.GetStartTimeOfJob(j);
            }
        }


        private static double TotalSlackOf(Job j, Schedule S)
        {
            return S.LSS[j.ID] - S.ESS[j.ID];
        }

        private static double SDROf(Job j, Schedule S)
        {
            return TotalSlackOf(j, S) / j.MeanProcessingTime;
        }
        //*******************************************


        /// <summary>
        /// Wrapper to build functions that calculate RMs., 
        /// </summary>
        /// <param name="Weight">Weight is some weighting function </param>
        /// <param name="SlackMeasure">  SlackMeasure is TS or FS</param>
        /// <param name="Modifier">Modifier is None, Binary or Upperbound</param>
        /// <param name="S">The schedule</param>
        /// <returns></returns>
        private static double RM(Func<Job, Schedule, double> Weight, Func<Job, Schedule, double> SlackMeasure, Func<Func<Job,Schedule,double>,Job,Schedule,double,double> Modifier, Schedule S,double fraction)
        {
            double total = 0.0;
            foreach (Job j in S.PrecedenceDAG.Jobs)
            {
                total += Weight(j, S) * Modifier(SlackMeasure,j,S,fraction);
            }
            return total;
        }

        // weights:
        private static double Unweighted(Job j, Schedule S)
        {
            return 1;
        }

        private static double NSucc(Job j, Schedule S)
        {
            double weight = j.Successors.Count;
            if (!j.Successors.Contains(S.GetMachineSuccessor(j)))
            {
                // Machine Successor is seperate.
                weight += 1;
            }
            return weight;
        }
        //

        //Modifiers:

        private static double Unmodified(Func<Job, Schedule, double> RM, Job j, Schedule S, double fraction)
        {
            return RM(j, S);
        }

        private static double Binary(Func<Job, Schedule, double> RM, Job j, Schedule S, double fraction)
        {
            if (RM(j, S) >= fraction * j.MeanProcessingTime)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        private static double Upperbound(Func<Job, Schedule, double> RM, Job j, Schedule S, double fraction)
        {
            if (RM(j, S) >= fraction * j.MeanProcessingTime)
            {
                return fraction * j.MeanProcessingTime;
            }
            else
            {
                return 0;
            }
        }



     
        
        

        


      /*  public static double DebugNumberOfJobsInIndexOrder(Schedule Sched)
        {

            double NGoodPairs = 0;
            foreach (Machine M in Sched.Machines)
            {
                foreach (Job J in M.AssignedJobs)
                {
                    if (Sched.GetMachineSuccessor(J) == null) { NGoodPairs += 1;}
                    else if(J.ID < Sched.GetMachineSuccessor(J).ID) { NGoodPairs += 1; }
                }
            }
            return NGoodPairs;

        }*/

    }
}
