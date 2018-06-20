using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    static class RobustnessMeasures
    {
        /// <summary>
        /// The free slack of job j in schedule S is the amount of time j can slip without delaying the start of the very next activity.
        /// </summary>
        /// <param name="j">Job j</param>
        /// <param name="S">Schedule s</param>
        /// <returns></returns>
        /// 
        public static double SlowFreeSlack(Job j, Schedule S)
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
                return freeSlack;
            }
            else
            {
                return S.EstimatedCmax - j.MeanProcessingTime - S.GetStartTimeOfJob(j);
            }
        }

        /// <summary>
        /// Calculate and return the sum of all free slacks. The free slack of job j in schedule S is the amount of time j can slip without delaying the start of the very next activity.
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public static double SumOfFreeSlacks(Schedule S)
        {
            double sum = 0.0;
            foreach (Job j in S.PrecedenceDAG.Jobs)
            {
                sum += SlowFreeSlack(j, S);
            }
            return sum;
        }

        public static int RMCount = 1;


        public static double DebugNumberOfJobsInIndexOrder(Schedule Sched)
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

        }

    }
}
