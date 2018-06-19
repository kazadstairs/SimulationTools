using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    class Schedule
    {
        // Job[] Jobs;
        // Machine[] Machines;
        // List<Tuple<Job, Machine, double>> Assignments;

        public DirectedAcyclicGraph PrecedenceDAG;
        private MachineArcPointer []  MachineArcPointers; // Maps Job Id to machine it is assigned to.
        public List<Machine> Machines; 
        public ProblemInstance Problem;

        // schedule basics:
        public string Description;
        private double[] Starttimes;
        private int[] AssignedMachineID; // keeps track of which machine each job is on: MachineId = MachineIdForJobId[j.id]

        // useful vars:
        public double EstimatedCmax;
        public double[] ESS; 
        public double[] DynamicDueDate;

        public Schedule(ProblemInstance prob)
        {
            Problem = prob;
            PrecedenceDAG = Problem.DAG;
            Machines = new List<Machine>(prob.NMachines); 
            for (int i = 0; i < prob.NMachines; i++)
            {
                Machines.Add(new Machine(i + 1));
            }

            Starttimes = new double[PrecedenceDAG.N];
            for (int i = 0; i < Starttimes.Length; i++)
            {
                Starttimes[i] = -1;
            }

            AssignedMachineID = new int[PrecedenceDAG.N];
            DynamicDueDate = new double[PrecedenceDAG.N];
            ESS = new double[PrecedenceDAG.N];
            MachineArcPointers = new MachineArcPointer[PrecedenceDAG.N];
        }

        private MachineArcPointer GetMachineArcPointer(Job j)
        {
            return MachineArcPointers[j.ID];
        }
        public Job GetMachineSuccessor(Job i)
        {
            if (MachineArcPointers[i.ID] == null) { throw new Exception("Job not yet assigned to a machine"); }
            else
            {
                if (MachineArcPointers[i.ID].ArrayIndex < GetMachineByID(MachineArcPointers[i.ID].MachineId).AssignedJobs.Count - 1) // successor exists
                {
                    return GetMachineByID(MachineArcPointers[i.ID].MachineId).AssignedJobs[MachineArcPointers[i.ID].ArrayIndex + 1];
                }
                else // Last job on the machine
                {
                    return null;
                }
            }

        }

        public Job GetMachinePredecessor(Job i)
        {            
            if (MachineArcPointers[i.ID] == null) { throw new Exception("Job not yet assigned to a machine"); }
            else
            {
                if (MachineArcPointers[i.ID].ArrayIndex > 0) // predecessor exists
                {
                    return GetMachineByID(MachineArcPointers[i.ID].MachineId).AssignedJobs[MachineArcPointers[i.ID].ArrayIndex - 1];
                }
                else // First job on the machine
                {
                    return null;
                }
            }                

        }

        public double GetStartTimeOfJob(Job j)
        {
            return Starttimes[j.ID];
        }

        public void PinedoSchedule()
        {
            AssignJobToMachineById(1, 0);
            AssignJobToMachineById(2, 0);
            AssignJobToMachineById(6, 0);
            AssignJobToMachineById(8, 0);

            AssignJobToMachineById(3, 1);
            AssignJobToMachineById(4, 1);
            AssignJobToMachineById(5, 1);
            AssignJobToMachineById(7, 1);
            AssignJobToMachineById(9, 1);
        }

        public void EstimateCmax()
        {
            //placeholder;
            double Maximum = 0;
            int MaxID = 0;
            for (int i = 1; i < Starttimes.Length; i++)
            {
                if (Starttimes[i] == -1) { throw new Exception("Startimes not calculated yet"); }
                if(Starttimes[i] > Maximum) { Maximum = Starttimes[i]; MaxID = i; }
            }
            EstimatedCmax = Maximum + PrecedenceDAG.GetJobById(MaxID).MeanProcessingTime;
            Console.WriteLine("Debug: Cmax is estimated to be {0}", EstimatedCmax);
        }

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
            int candidateMachineId = Distribution.UniformInt(Machines.Count) + 1; // + 1 because machine ids are 1 based
            Machine candidateMachine = GetMachineByID(candidateMachineId);

            if (!IsFeasibleAssignment(CurrentJob, candidateMachine)) { throw new Exception("Unfeasible assignment"); }
            else
            {
                AssignJobToMachine(CurrentJob, candidateMachine);
            }

        }

        public void MakeGreedyLoadAssignment()
        {
            Description = "GreedyLoadBalancing";
            AssignJobsBy(GreedyLoadBalancing);
        }

        public void MakeRandomAssignment()
        {
            Description = "Random";
            AssignJobsBy(RandomMachineAssignment);
        }

        /// <summary>
        /// Given a problem with precedence arcs as a DAG. Creates an assignment by iterating jobs over machines. Very naive.
        /// </summary>
        public void AssignByRolling()
        {
            Description = "Rolling Machine Assignment";
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
        /// Prints a summary of the current schedule
        /// </summary>
        public void Print()
        {
            Console.WriteLine("Schedule information:");
            Console.WriteLine("Machine info:");
            foreach (Machine m in Machines)
            {
                Console.Write("  M {0}: ", m.MachineID);
                foreach(Job j in m.AssignedJobs)
                {
                    Console.Write("{0}, ", j.ID);
                }
                Console.Write(Environment.NewLine);
            }
            Console.WriteLine("Job info:");
            foreach (Job j in PrecedenceDAG.Jobs)
            {
                Console.WriteLine("  Job {0} is on M {1}. OutArcs:", j.ID, GetMachineByJobID(j.ID).MachineID);
                foreach(Job succ in j.Successors)
                {
                    if (GetMachineByJobID(succ.ID) == GetMachineByJobID(j.ID))
                    {
                        Console.WriteLine("    {0} {1} (machine arc)", j.ID, succ.ID);
                    }
                    else
                    {
                        Console.WriteLine("    {0} {1}", j.ID, succ.ID);
                    }
                }
            }

        }
        private void DebugPrintJobId(Job j)
        {
            Console.WriteLine(j.ID);
        }

        public void MakeHTMLImage(string title)
        {
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(string.Format(@"C:\Users\Gebruiker\Documents\UU\MSc Thesis\Code\SchedulePDFs\InstanceName_{0}.html",title) ))
            {
                file.WriteLine(@"<!DOCTYPE html>");
                file.WriteLine(@"<head>");
                file.WriteLine(@"<style>");

                // css part:
                int scale = 20;
                double top, left, width;                
                foreach (Job j in PrecedenceDAG.Jobs)
                {
                    top = 50 + 50 * GetMachineByJobID(j.ID).MachineID;
                    left =  Starttimes[j.ID] * scale;
                    width = j.MeanProcessingTime * scale;
                    file.WriteLine("div.j{0}",j.ID);
                    file.WriteLine("{position: fixed;");
                    file.WriteLine("top: {1}px; left: {2}px; width: {3}px;", j.ID, top, left, width);
                    file.WriteLine(@"height: 20px; border: 1px solid #73AD21; text-align: center; vertical-align: middle;}");
                }
                file.WriteLine(@"</style></head><body>");
                file.WriteLine(@"<div>");
                file.WriteLine(title);
                file.WriteLine(@"</div>");
                foreach (Job j in PrecedenceDAG.Jobs)
                {
                    file.WriteLine("<div class=\"j{0}\"> J{0}; </div>",j.ID);
                }
                file.WriteLine(@"</body></html>");
            }
        }

        public void MakeTikzImage()
        {
            throw new System.NotImplementedException();
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(@"C:\Users\Gebruiker\Documents\UU\MSc Thesis\Code\SchedulePDFs\InstanceName_ScheduleId.txt"))
            {
                file.WriteLine(@"\begin{tikzpicture}");


                file.WriteLine(@"\end{tikzpicture}");
            }

        }

        public void SetESS()
        {
            foreach (Job j in PrecedenceDAG.Jobs)
            {
                Starttimes[j.ID] = ESS[j.ID];
            }
        }

        public void SetLSS()
        {
            foreach (Job j in PrecedenceDAG.Jobs)
            {
                Starttimes[j.ID] = DynamicDueDate[j.ID] - j.MeanProcessingTime;
            }
        }

        /// <summary>
        /// In O(|Vertices| + |Arcs|), determine the earliest Rj based on Mean Processing time for all jobs
        /// </summary>
        public void SetReleaseDates()
        {
            //init
            throw new Exception("Deprecated code");
            int[] nParentsProcessed = new int[PrecedenceDAG.N + 1]; // Position i contains the number of parents of Job with ID i that have been fully updated.
            Stack<Job> AllPredDone = new Stack<Job>(); // The jobs that will no longer change Rj are those for which all Parents have been considered.           
            foreach (Job j in PrecedenceDAG.Jobs)  //All jobs without predecessors can know their final Rj (it is equal to their own rj).
            {
                if (j.Predecessors.Count == 0)
                {
                    AllPredDone.Push(j);
                    ESS[j.ID] = j.EarliestReleaseDate;
                }
            }
            Job CurrentJob = null;
            double CandidateRj = -1;

            //algo
            while (AllPredDone.Count > 0)
            {
                CurrentJob = AllPredDone.Pop();
                foreach (Job Child in CurrentJob.Successors)
                {
                    CandidateRj = ESS[CurrentJob.ID] + CurrentJob.MeanProcessingTime;
                    if (CandidateRj > ESS[Child.ID]) // if new Rj is larger, update Rj
                    {
                        ESS[Child.ID] = CandidateRj;
                    }
                    nParentsProcessed[Child.ID]++;
                    if (nParentsProcessed[Child.ID] == Child.Predecessors.Count)
                    {
                        AllPredDone.Push(Child);
                    }
                }
            }

            //debug:
            Console.WriteLine("*************************************************");
            Console.WriteLine("Printing earliest release dates:");
            foreach (Job j in PrecedenceDAG.Jobs)
            {
                Console.WriteLine("Job with ID {0} has Rj {1}", j.ID, ESS[j.ID]);
            }

        }

        private void UpdateReleaseDateFor(Job j)
        {
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


        public void ForEachSuccDo(Job j, Action<Job> ApplyAction)
        {
            Job MSucc = GetMachineSuccessor(j);
            ApplyAction(MSucc);
            foreach (Job v in j.Successors)
            {
                if (v != MSucc) { ApplyAction(v); }
            }
        }

        private List<Job> BuildSuccessorIDList(Job j)
        {
            Console.WriteLine("WARNING: BuildSuccessorIDList is slow. Are you sure you need it?");
            List<Job> Succs = new List<Job>(j.Successors.Count + 1);
            Job MSucc = GetMachineSuccessor(j);
            Succs.Add(MSucc);
            foreach (Job v in j.Successors)
            {
                if (v != MSucc) { Succs.Add(v);}
            }
            return Succs;
        }


        /// <summary>
        /// Given a schedule with machine assignments, estimate the earliest start schedule based on mean processing times.
        /// </summary>
        public void CalcESS()
        {
            ForeachJobInPrecOrderDo(UpdateReleaseDateFor);
        }


        /// <summary>
        /// Performs the Action (Job ==> Void) on each Job in Precedence order (topological order). Considers Machine Predecessors.
        /// </summary>
        /// <param name="PerFormAction"></param>
        public void ForeachJobInPrecOrderDo(Action<Job> PerFormAction)
        {
            int[] nParentsProcessed = new int[PrecedenceDAG.N + 1]; // Position i contains the number of parents of Job with ID i that have been fully updated.
            Stack<Job> AllPredDone = new Stack<Job>(); // The jobs that will no longer change Rj are those for which all Parents have been considered.           
            foreach (Job j in PrecedenceDAG.Jobs)  //All jobs without predecessors can know their final Rj (it is equal to their own rj).
            {
                if (j.Predecessors.Count == 0 && GetMachinePredecessor(j) == null)
                {
                    AllPredDone.Push(j);
                    ESS[j.ID] = j.EarliestReleaseDate;
                }
            }
            Job CurrentJob = null;
            bool[] IsVisited = new bool[PrecedenceDAG.N + 1];
            bool[] IsPushed = new bool[PrecedenceDAG.N + 1];
            //algo
            while (AllPredDone.Count > 0)
            {
                CurrentJob = AllPredDone.Pop();

                PerFormAction(CurrentJob);

                IsVisited[CurrentJob.ID] = true;
                Job MachineSucc = GetMachineSuccessor(CurrentJob);
                if (MachineSucc != null && nParentsProcessed[MachineSucc.ID] == MachineSucc.Predecessors.Count)
                {
                    if(!IsPushed[MachineSucc.ID]) AllPredDone.Push(MachineSucc);
                }
                foreach (Job Child in CurrentJob.Successors)
                {
                    nParentsProcessed[Child.ID]++;
                    // if all prec predecessors are processed and either there is no machine parent or that has been processed aswell
                    if (nParentsProcessed[Child.ID] == Child.Predecessors.Count && (GetMachinePredecessor(Child) == null || IsVisited[GetMachinePredecessor(Child).ID]))
                    {
                        if (!IsPushed[Child.ID]) AllPredDone.Push(Child);
                    }
                }
                // also check the machine arc: // Todo, check we are not adding jobs twice!
                

            }

            //debug:
            Console.WriteLine("*************************************************");
            Console.WriteLine("Printing earliest release dates:");
            foreach (Job j in PrecedenceDAG.Jobs)
            {
                Console.WriteLine("Job with ID {0} has Rj {1}", j.ID, ESS[j.ID]);
            }

        }

        /// <summary>
        /// For now, it is the Latest Feasible completion time Based on mean processing times. TODO: Decide what this means exactly. 
        /// </summary>
        /// <param name="Cmax"></param>
        public void SetDeadlines(double Cmax)
        {
            throw new System.NotImplementedException("SetDeadlines is depricated");
            /*
            //init
            int[] nChildrenProcessed = new int[PrecedenceDAG.N + 1]; // Position i contains the number of parents of Job with ID i that have been fully updated.
            Stack<Job> AllSuccDone = new Stack<Job>(); // The jobs that will no longer change due date are those for which all Parents have been considered.           
            foreach (Job j in PrecedenceDAG.Jobs)  //All jobs without successor can know their final duedate (it is equal to Cmax).
            {
                if (j.Successors.Count == 0)
                {
                    AllSuccDone.Push(j);
                    DynamicDueDate[j.ID] = Cmax;
                }
                else
                {
                    DynamicDueDate[j.ID] = double.MaxValue;
                }
            }
            Job CurrentJob = null;
            double CandidateDj = -1;

            //algo
            while (AllSuccDone.Count > 0)
            {
                CurrentJob = AllSuccDone.Pop();
                foreach (Job Parent in CurrentJob.Predecessors)
                {
                    CandidateDj = DynamicDueDate[CurrentJob.ID] - CurrentJob.MeanProcessingTime;
                    if (CandidateDj < DynamicDueDate[Parent.ID]) // if new Dj is smaller, update Dj
                    {
                        DynamicDueDate[Parent.ID] = CandidateDj;
                    }
                    nChildrenProcessed[Parent.ID]++;
                    if (nChildrenProcessed[Parent.ID] == Parent.Successors.Count)
                    {
                        AllSuccDone.Push(Parent);
                    }
                }
            }

            //debug:
            Console.WriteLine("*************************************************");
            Console.WriteLine("Printing latest due dates:");
            foreach (Job j in PrecedenceDAG.Jobs)
            {
                Console.WriteLine("Job with ID {0} has Dj {1}", j.ID, DynamicDueDate[j.ID]);
            }

            Console.WriteLine("*************************************************");
            Console.WriteLine("Printing Latest Start times:");
            foreach (Job j in PrecedenceDAG.Jobs)
            {
                Console.WriteLine("Job with ID {0} has LSS {1}", j.ID, DynamicDueDate[j.ID] - j.SampleProcessingTime());
            }

            Console.WriteLine("*************************************************");
            Console.WriteLine("Printing Slack:");
            foreach (Job j in PrecedenceDAG.Jobs)
            {
                Console.WriteLine("Job with ID {0} has Slack {1}", j.ID, DynamicDueDate[j.ID] - j.SampleProcessingTime() - ESS[j.ID]); //LSS - ESS
            }
            */

        }



        /*
        public void PinedoInstanceSchedule(ProblemInstance Problem)
        {
            
            AssignJobToMachine(Problem.JobsList[0], Problem.Machines[0]); //Dummy job

            AssignJobToMachine(Problem.JobsList[1], Problem.Machines[0]);
            AssignJobToMachine(Problem.JobsList[2], Problem.Machines[0]);
            AssignJobToMachine(Problem.JobsList[6], Problem.Machines[0]);
            AssignJobToMachine(Problem.JobsList[8], Problem.Machines[0]);

            AssignJobToMachine(Problem.JobsList[3], Problem.Machines[1]);
            AssignJobToMachine(Problem.JobsList[4], Problem.Machines[1]);
            AssignJobToMachine(Problem.JobsList[5], Problem.Machines[1]);
            AssignJobToMachine(Problem.JobsList[7], Problem.Machines[1]);
            AssignJobToMachine(Problem.JobsList[9], Problem.Machines[1]);


        }
        */

        // todo, algorithm to make feasible schedules.

        /// <summary>
        /// Checks the feasibility of a machine assignment for both machine and prec arcs.
        /// </summary>
        /// <param name="j"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        bool IsFeasibleAssignment(Job j, Machine m)
        {
            Console.Error.WriteLine("Waring: TODO: Update IsFeasibleAssignment");
            if (AssignedMachineID[j.ID] > 0) { throw new System.Exception("Job already assigned!"); }
            else
            {
                if (m.LastJob() == null)
                {
                    // then the machine is empty and the assignment is always feasible.
                    return true;
                }
                else if (PathExists(j, m.LastJob()))
                {
                    //then adding j to m will create a cycle
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Perform a breadth first search from OriginJob. Apply the Applylogic. Applylogic should return true in the case of early stopping and false if the search fails.
        /// </summary>
        /// <param name="OriginJob">The Job from which BFS is performed</param>
        /// <param name="ApplyLogic">Job ==> Bool, returns true in the case of early stopping. False in the case of exhaustive unsuccessful search</param>
        /// <returns></returns>
        private bool BFSfrom(Job OriginJob, Func<Job,bool> ApplyLogic)
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
                 MSucc = GetMachineSuccessor(CurrentJob);
                 if(ApplyLogic(MSucc) == true) { return true; }
                 else if (!BFSVisited[MSucc.ID])
                 {
                     BFSVisited[MSucc.ID] = true;
                     BFSQueue.Enqueue(MSucc);
                 }

            }
            return false;
        }

        public bool PathExists(Job OriginJob, Job TargetJob)
        {
            return BFSfrom(OriginJob, (Job CurrentJob) => CurrentJob == TargetJob);
        }

        /// <summary>
        /// Assigns job j to machine m. Adds a machine arc (m.lastjob, j) to DAG if m has at least one job. Updates the load of machine m.
        /// </summary>
        /// <param name="j"></param>
        /// <param name="m"></param>
        /*
        void AssignJobToMachine(Job j, Machine m)
        {
            if (AssignedMachineID[j.ID] > 0) { throw new System.Exception("Job already assigned! If you want to reassign use Reassign function"); }
            else
            {
                if (m.AssignedJobs.Count > 0)
                {
                    PrecedenceDAG.AddArc(m.LastJob(), j); // todo: change to add machinearc
                }
                m.AssignedJobs.Add(j);
                m.Load += j.MeanProcessingTime;
                AssignedMachineID[j.ID] = m.MachineID;
            }
        }
       */
        void AssignJobToMachine(Job j, Machine m)
        {
            if (GetMachineArcPointer(j) != null) { throw new System.Exception("Job already assigned! If you want to reassign use Reassign function"); }
            else
            {
                m.AssignedJobs.Add(j);
                MachineArcPointers[j.ID] = new MachineArcPointer(m.MachineID, m.AssignedJobs.Count - 1);
                m.Load += j.MeanProcessingTime;
                AssignedMachineID[j.ID] = m.MachineID;
            }
        }

        public Machine GetMachineByJobID(int jobID)
        {
            return GetMachineByID(AssignedMachineID[jobID]);
        }

        void AssignJobToMachineById(int Jid, int Mid)
        {
            AssignJobToMachine(PrecedenceDAG.GetJobById(Jid), GetMachineByID(Mid));
        }

        private Machine GetMachineByID(int MachineID)
        {
            return Machines[MachineID - 1];
        }
    }
}
