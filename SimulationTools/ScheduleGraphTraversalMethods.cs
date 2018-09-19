using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    partial class Schedule
    {
        private List<Job> BuildSuccessorList(Job j)
        {
            throw new Exception("What uses this?");
            Console.WriteLine("WARNING: BuildSuccessorList is slow. Are you sure you need it?");
            List<Job> Succs = new List<Job>(j.Successors.Count + 1);
            Job MSucc = GetMachineSuccessor(j);
            Succs.Add(MSucc);
            foreach (Job v in j.Successors)
            {
                if (v != MSucc) { Succs.Add(v); }
            }
            return Succs;
        }

        /// <summary>
        /// Performs the Action (Job ==> Void) on each Job in Precedence order (topological order). Considers Machine Predecessors.
        /// </summary>
        /// <param name="PerFormAction"></param>
        public void ForeachJobInPrecOrderDo(Action<Job> PerFormAction)
        {
            int[] nParentsProcessed = new int[PrecedenceDAG.N + 1]; // Position i contains the number of parents of Job with ID i that have been fully updated.
            Stack<Job> AllPredDone = new Stack<Job>(); // The jobs that will no longer change Rj are those for which all Parents have been considered.  
            bool[] IsPushed = new bool[PrecedenceDAG.N + 1];
            bool[] IsVisited = new bool[PrecedenceDAG.N + 1];
            for (int i = 0; i < PrecedenceDAG.N + 1; i++)
            {
                IsPushed[i] = false;
                IsVisited[i] = false;
            }

            foreach (Job j in PrecedenceDAG.Jobs)  //All jobs without predecessors can know their final Rj (it is equal to their own rj).
            {
                if (j.Predecessors.Count == 0
                    && GetMachinePredecessor(j) == null)
                {
                    AllPredDone.Push(j);
                    IsPushed[j.ID] = true;
                    // ESS[j.ID] = j.EarliestReleaseDate; Do this as part of the action!
                }
            }
            Job CurrentJob = null;
            //algo
            int DebugJobsPopped = 0;
            while (AllPredDone.Count > 0)
            {
                CurrentJob = AllPredDone.Pop();
                if (IsVisited[CurrentJob.ID])
                {
                    // Oops, found it twice.
                }
                DebugJobsPopped++;

                PerFormAction(CurrentJob);

                IsVisited[CurrentJob.ID] = true;
                Console.WriteLine("Visited J{0} with successors:",CurrentJob.ID);
                foreach (Job J in CurrentJob.Successors)
                {
                    Console.Write("{0}  ",J.ID);
                }


                Job MachineSucc = GetMachineSuccessor(CurrentJob);
                if (MachineSucc != null)
                {
                    Console.Write("(Machine arc:) {0}", MachineSucc.ID);
                }
                Console.Write(Environment.NewLine);


                foreach (Job Child in CurrentJob.Successors)
                {
                    nParentsProcessed[Child.ID]++;
                    Job MachinePred = GetMachinePredecessor(Child);
                    if (nParentsProcessed[Child.ID] == Child.Predecessors.Count && (MachinePred == null || IsVisited[MachinePred.ID]))
                    {
                        if (!IsPushed[Child.ID])
                        {
                            AllPredDone.Push(Child);
                            IsPushed[Child.ID] = true;
                        }
                    }
                }
                if (MachineSucc != null && nParentsProcessed[MachineSucc.ID] == MachineSucc.Predecessors.Count)
                {
                    if (!IsPushed[MachineSucc.ID])
                    {
                        AllPredDone.Push(MachineSucc);
                        IsPushed[MachineSucc.ID] = true;
                    }
                }
                // missing machine arcs? NO! Machine arcs are dealt with.
            }
            for (int i = 0; i < PrecedenceDAG.N; i++)
            {
                if (!IsVisited[i])
                {
                    Console.WriteLine("ERROR! Printing Schedule for debugging then aborting.");
                    Console.WriteLine("Processed {0}/{1} jobs in Prec Order", DebugJobsPopped, PrecedenceDAG.N);
                    Console.WriteLine("Checking for cycles...");
                    bool Cyclesfound = false;
                    foreach (Machine M in this.Machines)
                    {
                        for (int higherindex = 1; higherindex < M.AssignedJobs.Count; higherindex++)
                        {
                            for (int lowerindex = 0; lowerindex < higherindex; lowerindex++)
                            {
                                if (PrecedenceDAG.PrecPathExists(M.AssignedJobs[higherindex], M.AssignedJobs[lowerindex]))
                                {
                                    Console.WriteLine("Cycle found on M{2}: Job {0} --Marcs--> Job {1} --Precs--> Job {0}", M.AssignedJobs[lowerindex].ID, M.AssignedJobs[higherindex].ID, M.MachineID);
                                    Cyclesfound = true;
                                }
                            }
                        }

                    }
                    if (!Cyclesfound) { Console.WriteLine("No cycles found"); }

                    this.Print();

                    throw new Exception("Not all jobs visited!");
                }
            }
        }

        /// <summary>
        /// Perfors the Action (Job ==> Void) on each Job in reverse precedence order (inverted topological order). Considers Machine Arcs
        /// </summary>
        /// <param name="PerFormAction"></param>
        public void ForeachJobInReversePrecOrderDo(Action<Job> PerFormAction)
        {
            int[] nChildrenProcessed = new int[PrecedenceDAG.N + 1]; // Position i contains the number of children of Job with ID i that have been fully updated.
            Stack<Job> AllSuccDone = new Stack<Job>(); // The jobs that will no longer change LSS are those for which all Parents have been considered.           
            foreach (Job j in PrecedenceDAG.Jobs)  //All jobs without successors can know their final sjLSS (it is equal to Cmax - pj).
            {
                if (j.Successors.Count == 0 && GetMachineSuccessor(j) == null)
                {
                    AllSuccDone.Push(j);
                }
            }
            Job CurrentJob = null;
            bool[] IsVisited = new bool[PrecedenceDAG.N + 1];
            bool[] IsPushed = new bool[PrecedenceDAG.N + 1];
            //algo
            while (AllSuccDone.Count > 0)
            {
                CurrentJob = AllSuccDone.Pop();

                PerFormAction(CurrentJob);

                IsVisited[CurrentJob.ID] = true;
                Job MachinePred = GetMachinePredecessor(CurrentJob);
                if (MachinePred != null && nChildrenProcessed[MachinePred.ID] == MachinePred.Successors.Count)
                {
                    if (!IsPushed[MachinePred.ID]) AllSuccDone.Push(MachinePred);
                }
                foreach (Job Parent in CurrentJob.Predecessors)
                {
                    nChildrenProcessed[Parent.ID]++;
                    if (nChildrenProcessed[Parent.ID] == Parent.Successors.Count && (GetMachineSuccessor(Parent) == null || IsVisited[GetMachineSuccessor(Parent).ID]))
                    {
                        if (!IsPushed[Parent.ID]) AllSuccDone.Push(Parent);
                    }
                }
            }
        }

        /// <summary>
        /// For every successor (including Machine successor), perform an Action
        /// </summary>
        /// <param name="j"></param>
        /// <param name="ApplyAction"></param>
        public void ForEachSuccDo(Job j, Action<Job> ApplyAction)
        {
            Job MSucc = GetMachineSuccessor(j);
            ApplyAction(MSucc);
            foreach (Job v in j.Successors)
            {
                if (v != MSucc) { ApplyAction(v); }
            }
        }

        public Job GetMachineSuccessor(Job J)
        {
            if (AssignedMachineID[J.ID] < 0) { throw new Exception("Job not yet assigned to a machine"); }
           //     MachineArcPointers[i.ID] == null) { throw new Exception("Job not yet assigned to a machine"); }
         //   if (MachineArcPointers[i.ID].MachineId == -1)
         //   {
                //Job was assigned and has been unassigned (intentional behaviour).
         //       return null;
         //   }
            else
            {
                if (GetIndexOnMachine(J) < AssignedMachine(J).AssignedJobs.Count - 1)// GetMachineByID(MachineArcPointers[J.ID].MachineId).AssignedJobs.Count - 1) // successor exists
                {
                    return AssignedMachine(J).AssignedJobs[GetIndexOnMachine(J) + 1];
                }
                else // Last job on the machine
                {
                    return null;
                }
            }

        }

        public int GetIndexOnMachine(Job J)
        {
            return GetMachineByID(AssignedMachineID[J.ID]).GetJobIndex(J);
        }

        public Job GetMachinePredecessor(Job J)
        {
            if (AssignedMachineID[J.ID] == 0) { throw new Exception("Job not yet assigned to a machine"); }
            if (AssignedMachineID[J.ID] == -1)
            {
                //Job was assigned and has been unassigned (intentional behaviour).
                return null;
            }
            else
            {
                if (GetIndexOnMachine(J) > 0) // predecessor exists
                {
                    return AssignedMachine(J).AssignedJobs[GetIndexOnMachine(J) - 1];
                }
                else // First job on the machine
                {
                    return null;
                }
            }

        }

        /// <summary>
        /// Perform a breadth first search from OriginJob. Apply the Applylogic. Applylogic should return true in the case of early stopping and false if the search fails.
        /// </summary>
        /// <param name="OriginJob">The Job from which BFS is performed</param>
        /// <param name="ApplyLogic">Job ==> Bool, returns true in the case of early stopping. False in the case of exhaustive unsuccessful search</param>
        /// <returns></returns>
        private bool PrecAndMachineBFSfrom(Job OriginJob, Func<Job, bool> ApplyLogic)
        {
            bool[] BFSVisited = new bool[this.PrecedenceDAG.N];
            Queue<Job> BFSQueue = new Queue<Job>();
            BFSQueue.Enqueue(OriginJob);
            Job CurrentJob;
            Job MSucc;
            while (BFSQueue.Count > 0)
            {
                CurrentJob = BFSQueue.Dequeue();
                foreach (Job Succ in CurrentJob.Successors)
                {
                    if (ApplyLogic(Succ) == true) { return true; }
                    else if (!BFSVisited[Succ.ID])
                    {
                        BFSVisited[Succ.ID] = true;
                        BFSQueue.Enqueue(Succ);
                    }

                }
                // same logic as the foreach above. Note that a successor will only be visited once, independent of how many in arcs it has.
                // ONLY check for successors if this job is assigned
                if (AssignedMachineID[CurrentJob.ID] <= 0)
                {
                    //Job not yet assigned, no successor arcs will exits.
                }
                else
                {
                    MSucc = GetMachineSuccessor(CurrentJob);
                    if (MSucc != null)
                    {
                        if (ApplyLogic(MSucc) == true) { return true; }
                        else if (!BFSVisited[MSucc.ID])
                        {
                            BFSVisited[MSucc.ID] = true;
                            BFSQueue.Enqueue(MSucc);
                        }

                    }
                    else
                    {
                        Console.WriteLine("Warning: Not checked!");
                    }

                }


            }
            return false;
        }


        /// <summary>
        /// Checks for path in COMBINED Prec,Machine graph
        /// </summary>
        /// <param name="OriginJob"></param>
        /// <param name="TargetJob"></param>
        /// <returns></returns>
        public bool PrecAndMachinePathExists(Job OriginJob, Job TargetJob)
        {
            return PrecAndMachineBFSfrom(OriginJob, (Job CurrentJob) => CurrentJob == TargetJob);
        }

        /// <summary>
        /// Excluding the Machine Arc ForbiddenArc, checks for a path from OriginJob to TargetJob.
        /// </summary>
        /// <param name="OriginJob"></param>
        /// <param name="TargetJob"></param>
        /// <param name="ForbiddenArc"></param>
        /// <returns></returns>
        public bool PrecMachPathExistsWithout(Job OriginJob, Job TargetJob, Tuple<Job, Job> ForbiddenArc)
        {
            bool[] BFSVisited = new bool[this.PrecedenceDAG.N];
            Queue<Job> BFSQueue = new Queue<Job>();
            BFSQueue.Enqueue(OriginJob);
            Job CurrentJob;
            Job MSucc;
            while (BFSQueue.Count > 0)
            {
                CurrentJob = BFSQueue.Dequeue();
                if (CurrentJob == TargetJob) { return true; }
                foreach (Job Succ in CurrentJob.Successors)
                {
                    if (!BFSVisited[Succ.ID])
                    {
                        BFSVisited[Succ.ID] = true;
                        BFSQueue.Enqueue(Succ);
                    }

                }
                // same logic as the foreach above. Note that a successor will only be visited once, independent of how many in arcs it has.
                // ONLY check for successors if this job is assigned
                if (AssignedMachineID[CurrentJob.ID] <= 0)
                {
                    //Job not yet assigned, no successor arcs will exits.
                }
                else
                {
                    MSucc = GetMachineSuccessor(CurrentJob);
                    if (MSucc != null && !BFSVisited[MSucc.ID])
                    {
                        if (CurrentJob == ForbiddenArc.Item1 && MSucc == ForbiddenArc.Item2)
                        {
                            // this is the forbidden arc, ignore it.
                        }
                        else
                        {
                            BFSVisited[MSucc.ID] = true;
                            BFSQueue.Enqueue(MSucc);
                        }
                    }
                }
            }
            return false;
        }

        private void UpdateReleaseDateFor(Job j)
        {
            ESS[j.ID] = j.EarliestReleaseDate;
            foreach (Job Parent in j.Predecessors)
            {
                if (ESS[j.ID] < ESS[Parent.ID] + Parent.MeanProcessingTime)
                {
                    ESS[j.ID] = ESS[Parent.ID] + Parent.MeanProcessingTime;
                }
            }
            if (GetMachinePredecessor(j) != null && ESS[j.ID] < ESS[GetMachinePredecessor(j).ID] + GetMachinePredecessor(j).MeanProcessingTime)
            {
                ESS[j.ID] = ESS[GetMachinePredecessor(j).ID] + GetMachinePredecessor(j).MeanProcessingTime;
            }
        }

        private void Update_LSS_StartDateFor(Job j)
        {
            LSS[j.ID] = this.EstimatedCmax - j.MeanProcessingTime;
            foreach (Job Child in j.Successors)
            {
                if (LSS[j.ID] > LSS[Child.ID] - j.MeanProcessingTime) // if j is set to start at an impossible time, set it earlier.
                {
                    LSS[j.ID] = LSS[Child.ID] - j.MeanProcessingTime;
                }
            }
            if (GetMachineSuccessor(j) != null && LSS[j.ID] > LSS[GetMachineSuccessor(j).ID] - j.MeanProcessingTime)
            {
                LSS[j.ID] = LSS[GetMachineSuccessor(j).ID] - j.MeanProcessingTime;
            }
        }

    }
}
