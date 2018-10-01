using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    static class RobustnessMeasures
    {
        
        public static double SoFS(Schedule S)
        {
            return RM(Unweighted, FreeSlackOf, Unmodified,S,0);
        }

        public static double SoBFS(Schedule S,double fraction)
        {
            return RM(Unweighted, FreeSlackOf, Binary, S, fraction);
        }

        public static double SoUFS(Schedule S,double fraction)
        {
            return RM(Unweighted, FreeSlackOf, Upperbound, S, fraction);
        }

        public static double SowFS(Schedule S)
        {
            return RM(NSucc, FreeSlackOf, Unmodified, S, 0);
        }

        public static double SoTS(Schedule S)
        {
            return RM(Unweighted, TotalSlackOf, Unmodified, S, 0);
        }

        public static double SoBTS(Schedule S,double fraction)
        {
            return RM(Unweighted, TotalSlackOf, Binary, S, fraction);
        }

        public static double SoUTS(Schedule S, double fraction)
        {
            return RM(Unweighted, TotalSlackOf, Upperbound, S, fraction);
        }

        public static double SowTS(Schedule S)
        {
            return RM(NSucc, TotalSlackOf, Unmodified, S, 0);
        }

        public static double SoSDR(Schedule S)
        {
            return RM(Unweighted, SDROf, Unmodified, S, 0);
        }

        public static double NormalBasedEstimatedCmax(Schedule Sched, double StdDevAssumption) // As actual Distr. of jobs not known, assume they are normal with StdDev = StdDevAssumption * MeanProcTime
        {
            var S = new Distribution[Sched.PrecedenceDAG.N, Sched.PrecedenceDAG.N]; //all starttimes as distributions (Mean,Var) pairs. S_j^k = S[j,k]
            Sched.ForeachJobInPrecOrderDo(J => EstimateStartTimeDistribution(J, Sched, S,StdDevAssumption));
            double Cmax = 0;
            double Cj = 0;
            foreach (Job J in Sched.PrecedenceDAG.Jobs)
            {
                Cj = GetStartTimeDistribution(J, Sched, S).Mean + J.MeanProcessingTime;
                if (Cj > Cmax)
                {
                    Cmax = Cj;
                }
            }
            return Cmax;
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
            S.CalcESS();
            S.CalcLSS();
            return S.GetLatestStart(j) - S.GetEarliestStart(j);
        }
        /// <summary>
        /// Assume we do not know the distribution of the jobs. As an estimate, use Normal distribution and StdDev = 0.1 meanproctime.
        /// </summary>
        /// <param name="J"></param>
        /// <param name="Sched"></param>
        /// <param name="S"></param>
        /// <param name="StandardDeviationAssumption">How large the standard deviation is assumed to be, as a factor of the mean processing time.</param>
        /// <returns></returns>
        private static void EstimateStartTimeDistribution(Job J,Schedule Sched, Distribution [,] S, double StandardDeviationAssumption)
        {
            // as a starting point, Starttime = completion of machine predecessor.
            // todo: ADD RELEASE DATES TO STARTIME POINT?
            if (Sched.GetMachinePredecessor(J) == null)
            {
                S[J.ID, 0] = new ZeroDistribution();
                Console.WriteLine("Setting S[{0},{1}]", J.ID, 0);
            }
            else
            {
                Job Pred = Sched.GetMachinePredecessor(J);
                Distribution PredCompletion = DistributionFunctions.NormalAddition( S[Pred.ID, Pred.Predecessors.Count], new ConstantAsDistribution(Pred.MeanProcessingTime));
                S[J.ID, 0] = PredCompletion;
                Console.WriteLine("Setting S[{0},{1}]", J.ID, 0);
            }
            // IN A SPECIFIC ORDER: First all predecessors on the same machine.
            int _k = 1;
            foreach (Job Predecessor in J.Predecessors)
            {
                if (Sched.GetMachineByJobID(Predecessor.ID) == Sched.GetMachineByJobID(J.ID))
                {
                    // both jobs on the same machine
                    if (Predecessor.ID == Sched.GetMachinePredecessor(J).ID)
                    {
                        Distribution delta = new Distribution();
                        //pred is machine pred
                        throw new Exception("TODO, implement this next");
                    }
                    else
                    {
                        //same machine, but not direct machine predecessor.
                        //X = S_i, Y = S^k-1_j so that delta = Y-X = sum_i to mp(j) P
                        Distribution delta = new Distribution();
                        Job CurrentJob = Predecessor;
                        while (CurrentJob != J)
                        {
                            delta.Mean += CurrentJob.MeanProcessingTime;
                            delta.Variation += (StandardDeviationAssumption * CurrentJob.MeanProcessingTime) * (StandardDeviationAssumption * CurrentJob.MeanProcessingTime);
                        }
                        S[J.ID, _k] = DistributionFunctions.MaximumGivenDelta(
                                                    DistributionFunctions.NormalAddition(GetStartTimeDistribution(Predecessor, Sched, S), new ConstantAsDistribution(Predecessor.MeanProcessingTime)), //S_i + p_i
                                                    S[J.ID, _k-1], //S_j^k-1
                                                    delta);

                        Console.WriteLine("Setting S[{0},{1}]", J.ID, Predecessor.ID);
                    }
                }
                else
                {
                    // job on a different machine, handle in next loop.
                }
                _k++;
            }
            // IN A SPECIFIC ORDER: next all predecessors on a different machine
            _k = 1;
            foreach (Job Predecessor in J.Predecessors)
            {
                if (Sched.GetMachineByJobID(Predecessor.ID) == Sched.GetMachineByJobID(J.ID))
                {
                    // both jobs on the same machine: done in last loop, skip.
                }
                else
                {
                    S[J.ID,_k] = DistributionFunctions.Maximum(
                                                    DistributionFunctions.NormalAddition(GetStartTimeDistribution(Predecessor, Sched, S), new ConstantAsDistribution(Predecessor.MeanProcessingTime)), //S_i + p_i
                                                    S[J.ID, _k - 1], //S_j^k-1
                                                    true);

                    Console.WriteLine("Setting S[{0},{1}]", J.ID, Predecessor.ID);
                }
                _k++;
            }
        }

        private static Distribution GetStartTimeDistribution(Job J, Schedule Sched, Distribution[,] S)
        {
            return S[J.ID, J.Predecessors.Count];
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

        private static double MeanRM(Func<Job, Schedule, double> Weight, Func<Job, Schedule, double> SlackMeasure, Func<Func<Job, Schedule, double>, Job, Schedule, double, double> Modifier, Schedule S, double fraction)
        {
            double total = 0.0;
            foreach (Job j in S.PrecedenceDAG.Jobs)
            {
                total += Weight(j, S) * Modifier(SlackMeasure, j, S, fraction);
    }
            return total/S.PrecedenceDAG.N;
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
