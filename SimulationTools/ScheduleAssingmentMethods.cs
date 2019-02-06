using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    // Contains all methods for performing an Assignment Heuristic.
    // Low level AssignJobToMachine type methods not included.
    partial class Schedule
    {
        /// <summary>
        /// In prec order, assign jobs to the machine with the smallest load.
        /// </summary>
        public void MakeGreedyLoadAssignment()
        {
            AssignmentDescription = "GreedyLoadBalancing";
            AssignJobsBy(GreedyLoadBalancing);
        }

        /// <summary>
        /// In prec order, assign jobs to a random machine
        /// </summary>
        public void MakeRandomAssignment()
        {
            AssignmentDescription = "Random";
            AssignJobsBy(RandomMachineAssignment);
        }

        /// <summary>
        /// Given a problem with precedence arcs as a DAG. Creates an assignment by iterating jobs over machines. Very naive.
        /// </summary>
        public void MakeRollingMachineAssignment()
        {
            AssignmentDescription = "Rolling Machine Assignment";
            if (PrecedenceDAG.N <= 1)
            {
                throw new Exception("No DAG given. Cannot build schedule without problem instance");
            }
            int NJobsAssigned = 0;
            int[] nParentsProcessed = new int[PrecedenceDAG.N + 1];
            Queue<Job> AllPredDone = new Queue<Job>(); // The jobs that will no longer change Rj are those for which all Parents have been considered.           
            foreach (Job j in PrecedenceDAG.Jobs)  //All jobs without predecessors can know their final Rj (it is equal to their own rj).
            {
                if (j.Predecessors.Count == 0)
                {
                    AllPredDone.Enqueue(j);
                }
            }

            int CandidateMachineID = 0;

            while (AllPredDone.Count > 0)
            {
                bool DebugAssignmentSuccess = false;
                CandidateMachineID++;
                if (CandidateMachineID >= Machines.Count + 1) { CandidateMachineID = 1; }

                Job CurrentJob = AllPredDone.Dequeue();

                for (int i = 0; i < Machines.Count; i++)
                {
                    if (!IsFeasibleAssignment(CurrentJob, GetMachineByID(CandidateMachineID)))
                    {
                        // try the next machine:
                        CandidateMachineID++;
                        if (CandidateMachineID >= Machines.Count) { CandidateMachineID = 0; }
                    }
                    else
                    {
                        // assign to that machine
                        //IMPORTANT: Update the Queue before adding any arcs, or things may be added to the queue twice:
                        foreach (Job Succ in CurrentJob.Successors)
                        {
                            nParentsProcessed[Succ.ID]++;
                            if (nParentsProcessed[Succ.ID] == Succ.Predecessors.Count)
                            {
                                AllPredDone.Enqueue(Succ);
                            }
                        }
                        //IMPORTANT: Only do this after the queue has been updated. 
                        // if it is the first job on the machine, no precedence arc is needed:
                        if (GetMachineByID(CandidateMachineID).AssignedJobs.Count > 0)
                        {
                            PrecedenceDAG.AddArc(GetMachineByID(CandidateMachineID).LastJob(), CurrentJob);
                        }
                        //IMPORTANT: Only do this after the queue has been updated and after machine arcs have been updated                         
                        AssignJobToMachine(CurrentJob, GetMachineByID(CandidateMachineID));

                        DebugAssignmentSuccess = true;
                        break;
                    }
                }
                if (!DebugAssignmentSuccess) { throw new Exception("Schedulde.Build was unable to create a feasible schedule: the algorithm is bugged."); }
                NJobsAssigned++;
            }
            if (NJobsAssigned < PrecedenceDAG.N)
            {
                throw new Exception("Not all jobs assigned");
            }
        }

        /// <summary>
        /// Create the assignment from the Pinedo Example (mostly for debugging).
        /// </summary>
        public void PinedoSchedule()
        {
            AssignJobToMachineById(0, 1); //Dummy job

            AssignJobToMachineById(1, 1);
            AssignJobToMachineById(2, 1);
            AssignJobToMachineById(6, 1);
            AssignJobToMachineById(8, 1);

            AssignJobToMachineById(3, 2);
            AssignJobToMachineById(4, 2);
            AssignJobToMachineById(5, 2);
            AssignJobToMachineById(7, 2);
            AssignJobToMachineById(9, 2);

        }

        /// <summary>
        /// In order of precedence graph (and thus feasibly), assign jobs to machines.
        /// </summary>
        /// <param name="AssignmentLogic"></param>
        private void AssignJobsBy(Action<Job> AssignmentLogic)
        {
            int[] nParentsProcessed = new int[PrecedenceDAG.N];

            Queue<Job> AllPredDone = new Queue<Job>(); // Jobs whose predecessors have been visited           
            foreach (Job j in PrecedenceDAG.Jobs) //Jobs without prec constraints and without machine parents are ready to visit.
            {
                if (j.Predecessors.Count == 0)
                {
                    AllPredDone.Enqueue(j);
                }
            }

            while (AllPredDone.Count > 0)
            {
                Job CurrentJob = AllPredDone.Dequeue();
                // process this job
                AssignmentLogic(CurrentJob);
                // tell successors this job is processed
                foreach (Job Succ in CurrentJob.Successors)
                {
                    nParentsProcessed[Succ.ID]++;
                    if (nParentsProcessed[Succ.ID] == Succ.Predecessors.Count)
                    {
                        AllPredDone.Enqueue(Succ);
                    }
                }

            }

        }

        
                
        private void GreedyLoadBalancing(Job CurrentJob)
        {
            int MinLoadId = -1;
            double MinLoad = double.MaxValue;
            Machine candidateMachine;
            for (int i = 0; i < Machines.Count; i++)
            {
                candidateMachine = GetMachineByID(i + 1); // + 1 because machine ids are 1 based
                if (candidateMachine.Load < MinLoad) { MinLoad = candidateMachine.Load; MinLoadId = candidateMachine.MachineID; }
            }
            Machine MinLoadMachine = GetMachineByID(MinLoadId);

            if (!IsFeasibleAssignment(CurrentJob, MinLoadMachine)) { throw new Exception("Unfeasible assignment"); }
            else
            {
                AssignJobToMachine(CurrentJob, MinLoadMachine);
            }
        }

        private void RandomMachineAssignment(Job CurrentJob)
        {
            int candidateMachineId = DistributionFunctions.UniformInt(Machines.Count) + 1; // + 1 because machine ids are 1 based
            Machine candidateMachine = GetMachineByID(candidateMachineId);

            if (!IsFeasibleAssignment(CurrentJob, candidateMachine)) { throw new Exception("Unfeasible assignment"); }
            else
            {
                AssignJobToMachine(CurrentJob, candidateMachine);
            }

        }

       


      

  
    }
}
