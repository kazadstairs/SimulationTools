﻿using System;
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

        public static double SowUFS(Schedule S, double fraction)
        {
            return RM(NSucc, FreeSlackOf, Upperbound, S, fraction);
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

        public static double SowUTS(Schedule S, double fraction)
        {
            return RM(NSucc, TotalSlackOf, Upperbound, S, fraction);
        }

        public static double SoSDR(Schedule S)
        {
            return RM(Unweighted, SDROf, Unmodified, S, 0);
        }

        public static Distribution NormalBasedEstimatedCmax(Schedule Sched, double VariationAssumption) // As actual Distr. of jobs not known, assume they are normal with StdDev = StdDevAssumption * MeanProcTime
        {            
            //Console.WriteLine("Comparing Normalbased Estimated Completion times");
            var C = new Distribution[Sched.PrecedenceDAG.N]; //all starttimes as distributions (Mean,Var) pairs. S_j^k = S[j,k]
            Sched.ForeachJobInPrecOrderDo(J => EstimateCompletionTimeDistribution(J, Sched, C,VariationAssumption));
            // completion time of all jobs known, now for each machine, take the maximum of the completion times of the last jobs:
            Distribution Cmax = new ZeroDistribution();
            foreach (Machine M in Sched.Machines)
            {
                if (M.LastJob() != null)
                {
                    Cmax = DistributionFunctions.Maximum(Cmax, C[M.LastJob().ID], true);
                }
                //else nothing: empty machines do not contribute to potential delay.
            }

            /*
            double Cmax = 0;
            double Cj = 0;
            foreach (Job J in Sched.PrecedenceDAG.Jobs)
            {
                Cj = GetStartTimeDistribution(J, Sched, S).Mean + J.MeanProcessingTime;
                Console.WriteLine("C{0} = {1}",J.ID,Cj);
                if (Cj > Cmax)
                {
                    Cmax = Cj;
                }
            }
            Console.WriteLine("E(Cmax) = {0}", Cmax);
            */
            if (Cmax.Mean < 0.01)
            {
                Console.WriteLine("WARNING, very small MAKESPAN: {0}",Cmax.Mean);
            }
            if (Cmax.Mean < Sched.DeterministicCmax - 0.1)
            {
                Console.Write("WARNING: CMax.Mean too small fired in NormalBasedEstimatedCmax.. recalculating and setting deterministic ESS...");
                Sched.CalcESS();
                Sched.SetESS();
                Sched.EstimateCmax();
                if (Cmax.Mean < Sched.DeterministicCmax - 0.1)
                {
                    Console.WriteLine("ERROR: CMax.Mean too small. Cmax.Mean = {0}, DetCmax = {1}", Cmax.Mean, Sched.DeterministicCmax);
                    Sched.ForeachJobInPrecOrderDo(J => Console.WriteLine("S{0,-2} = {1,-3}, P{0,-2} = {2,-3}, C{0,-2}={3,-3}, C[{0,-2}] = {4,-3}.",
                                                                            J.ID,
                                                                            Sched.GetStartTimeOfJob(J),
                                                                            J.MeanProcessingTime,
                                                                            Sched.GetStartTimeOfJob(J) + J.MeanProcessingTime,
                                                                            C[J.ID].Mean));
                    throw new Exception("Cmax.Mean too small");
                }
                else
                {
                    Console.WriteLine("... RESOLVED. Hope this does not cause other bugs.");
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
                if (S.DeterministicCmax - j.MeanProcessingTime - S.GetStartTimeOfJob(j) < 0)
                {
                    throw new Exception(string.Format("{0}",S.GetStartTimeOfJob(j)));
                }
                return S.DeterministicCmax - j.MeanProcessingTime - S.GetStartTimeOfJob(j);
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
        private static void _Deprecated_EstimateStartTimeDistribution(Job J,Schedule Sched, Distribution [,] S, double StandardDeviationAssumption)
        {
            // as a starting point, Starttime = completion of machine predecessor.
            // todo: ADD RELEASE DATES TO STARTIME POINT?
            if (Sched.GetMachinePredecessor(J) == null)
            {
                S[J.ID, 0] = new ZeroDistribution();
                Console.WriteLine("Setting S[{0},{1}] = N(0,0)", J.ID, 0);
            }
            else // CASE 1
            {
                Job Pred = Sched.GetMachinePredecessor(J);
                Distribution PredCompletion = DistributionFunctions.NormalAddition( S[Pred.ID, Pred.Predecessors.Count], new ConstantAsDistribution(Pred.MeanProcessingTime));
                S[J.ID, 0] = PredCompletion;
                Console.WriteLine("Setting S[{0},{1}]", J.ID, 0);
            }
            // IN A SPECIFIC ORDER: First all predecessors on the same machine.
            int _k = 0;
            foreach (Job Predecessor in J.Predecessors)
            {
                if (Sched.GetMachineByJobID(Predecessor.ID) == Sched.GetMachineByJobID(J.ID))
                {
                    _k++;

                    // both jobs on the same machine
                    if (Predecessor.ID == Sched.GetMachinePredecessor(J).ID) // CASE 1, should never fire
                    {
                        S[J.ID, _k] = S[J.ID, _k - 1];
                        //S[J.ID, _k] = DistributionFunctions.Maximum(S[J.ID, _k - 1],
                        //    DistributionFunctions.NormalAddition(new ConstantAsDistribution(Predecessor.MeanProcessingTime), GetStartTimeDistribution(Predecessor, Sched, S)),
                        //    false);
                        //pred is machine pred
                        // perhaps this is not necessary: I don't have q_ij, other than processing times.
                    }
                    else
                    {
                        //same machine, but not direct machine predecessor.: CASE 2
                        //X = S_i, Y = S^k-1_j so that delta = Y-X = sum_i to mp(j) P
                        S[J.ID, _k] = S[J.ID, _k - 1];

                        /*Distribution delta = new Distribution();
                        Job CurrentJob = Predecessor;
                        int DEBUG = 0;
                        while (CurrentJob != J)
                        {
                            delta.Mean += CurrentJob.MeanProcessingTime;
                            delta.Variation += (StandardDeviationAssumption * CurrentJob.MeanProcessingTime) * (StandardDeviationAssumption * CurrentJob.MeanProcessingTime);

                            CurrentJob = Sched.GetMachineSuccessor(CurrentJob);
                            if (DEBUG > 100)
                            {
                                throw new Exception("infinite loop");
                            }
                        }
                        S[J.ID, _k] = DistributionFunctions.MaximumGivenDelta(
                                                    DistributionFunctions.NormalAddition(GetStartTimeDistribution(Predecessor, Sched, S), new ConstantAsDistribution(Predecessor.MeanProcessingTime)), //S_i + p_i
                                                    S[J.ID, _k-1], //S_j^k-1
                                                    delta);

                        Console.WriteLine("Setting S[{0},{1}]", J.ID, _k);
                        */
                    }
                }
                else
                {
                    // job on a different machine, handle in next loop.
                }
            }
            // IN A SPECIFIC ORDER: next all predecessors on a different machine
            foreach (Job Predecessor in J.Predecessors)
            {
                if (Sched.GetMachineByJobID(Predecessor.ID) == Sched.GetMachineByJobID(J.ID))
                {
                    // both jobs on the same machine: done in last loop, skip.
                }
                else
                {
                    _k++;

                    S[J.ID,_k] = DistributionFunctions.Maximum(
                                                    DistributionFunctions.NormalAddition(GetStartTimeDistribution(Predecessor, Sched, S), new ConstantAsDistribution(Predecessor.MeanProcessingTime)), //S_i + p_i
                                                    S[J.ID, _k - 1], //S_j^k-1
                                                    true);

                    Console.WriteLine("Setting S[{0},{1}] due to different machine predecessor.", J.ID, _k);
                }
            }
            //finally, compare with the release date
            S[J.ID, _k] = DistributionFunctions.Maximum(new ConstantAsDistribution(J.EarliestReleaseDate), S[J.ID, _k],true);
            Console.WriteLine("S{0}=S[{0},{1}]={2}",J.ID,_k,S[J.ID,_k].Mean);
        }

        private static void EstimateCompletionTimeDistribution(Job J, Schedule Sched, Distribution[] C, double StandardDeviationAssumption)
        {
            if (Sched.GetMachinePredecessor(J) == null)
            {
                C[J.ID] = new ZeroDistribution(); // processing time at end of function
                //Console.WriteLine("Setting C[{0},{1}] = N(0,0)", J.ID, 0);
            }
            else // CASE 1
            {
                Job Pred = Sched.GetMachinePredecessor(J);
                C[J.ID] = C[Pred.ID];
            }
            // determine the last predecessor on each machine.
            int[] LastPredecessorID = new int[Sched.Problem.NMachines + 1];
            for (int i = 0; i < Sched.Problem.NMachines + 1; i++)
            {
                LastPredecessorID[i] = -1;
            }
            int AssignedMachineID = -1;
            foreach (Job Pred in J.Predecessors)
            {
                AssignedMachineID = Sched.GetMachineByJobID(Pred.ID).MachineID;
                if (LastPredecessorID[AssignedMachineID] < 0) // if no predecessor yet found on this machine, this job is the last known predecessor on this machine
                {
                    LastPredecessorID[AssignedMachineID] = Pred.ID;
                }
                else if (C[Pred.ID].Mean > C[Sched.Problem.DAG.GetJobById(LastPredecessorID[AssignedMachineID]).ID].Mean)
                {
                    //this job is the last one on the macine (so far)
                    LastPredecessorID[AssignedMachineID] = Pred.ID;
                }
            }//now LastPredecessor contains the IDs of all last jobs on the machine.
            for (int m = 1; m < Sched.Problem.NMachines + 1; m++)
            {
                if (m == Sched.GetMachineByJobID(J.ID).MachineID) //Case 1 or case 2
                {
                    //last predecessor on same machine, either considered above, or not the Machine predecessor, so we can ignore it.
                }
                else // different machine, update the starttime of J
                {
                    if (LastPredecessorID[m] < 0)
                    {
                        //no predecessor on this machine: This machine does not influence the startime of J
                    }
                    else
                    {
                        C[J.ID] = DistributionFunctions.Maximum(C[LastPredecessorID[m]], C[J.ID], true);
                    }
                }
            }
            //compare to release date:
            C[J.ID] = DistributionFunctions.Maximum(C[J.ID], new ConstantAsDistribution(J.EarliestReleaseDate),true);
            //now C[J.ID] is the approximated maximum starttime of J, add the processing time info of J to estimate the completion time:
            C[J.ID] = DistributionFunctions.NormalAddition(C[J.ID], new Distribution(J.MeanProcessingTime, StandardDeviationAssumption * J.MeanProcessingTime));
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
                if (j.ID == 0)
                {//skip
                }
                else
                {
                    total += Weight(j, S) * Modifier(SlackMeasure, j, S, fraction);
                }
                
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

        public static double NSucc(Job j, Schedule S)
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
