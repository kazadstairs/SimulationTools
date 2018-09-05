using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    [Serializable] class Schedule
    {
        // Job[] Jobs;
        // Machine[] Machines;
        // List<Tuple<Job, Machine, double>> Assignments;

        public DirectedAcyclicGraph PrecedenceDAG;
        public MachineArcPointer[] MachineArcPointers { get; private set; } // Maps Job Id to machine it is assigned to and its position in the list.
        public List<Machine> Machines; 
        public ProblemInstance Problem;
        // schedule basics:
        public string AssignmentDescription;
        public string StartTimeDescription;
        private double[] Starttimes;
        private int[] AssignedMachineID; // keeps track of which machine each job is on: MachineId = MachineIdForJobId[j.id]

        // useful vars:
        public double EstimatedCmax;
        public double[] ESS;
        public double[] LSS;
        //public double[] DynamicDueDate;

        // RMs
        public List<RM> RMs;

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
            LSS = new double[PrecedenceDAG.N];
            ESS = new double[PrecedenceDAG.N];
            MachineArcPointers = new MachineArcPointer[PrecedenceDAG.N];


            RMs = new List<RM>();


        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="Original"></param>
        public Schedule(Schedule Original)
        {
            //Point to same thing as original
            Problem = Original.Problem;
            PrecedenceDAG = Original.PrecedenceDAG;
            RMs = Original.RMs;

            //Create new
            Machines = new List<Machine>(Problem.NMachines);

            for (int i = 0; i < Problem.NMachines; i++)
            {
                Machines.Add(new Machine(i + 1));
            }

            Starttimes = new double[PrecedenceDAG.N];
            for (int i = 0; i < Starttimes.Length; i++)
            {
                Starttimes[i] = -1;
            }

            AssignedMachineID = new int[PrecedenceDAG.N];
            for (int i = 0; i < PrecedenceDAG.N;i++)
            {
                AssignedMachineID[i] = -1;
            }
            LSS = new double[PrecedenceDAG.N];
            ESS = new double[PrecedenceDAG.N];
            MachineArcPointers = new MachineArcPointer[PrecedenceDAG.N];

            //Copy the information:
            Original.ForeachJobInPrecOrderDo(j => AssignJobToMachineById(j.ID, Original.AssignedMachineID[j.ID]));
            CalcESS();
            SetESS();
            for (int i = 0; i < PrecedenceDAG.N; i++)
            {
                if (MachineArcPointers[i].ArrayIndex != Original.MachineArcPointers[i].ArrayIndex) { throw new Exception("Copy mistake"); }
                if (MachineArcPointers[i].MachineId != Original.MachineArcPointers[i].MachineId) { throw new Exception("Copy mistake"); }
            }
        }

        public void CalcRMs()
        {
            if (RMs.Count > 0) { throw new Exception("RMs aready calculated"); }
            else if (EstimatedCmax <= 0) { Console.WriteLine("Warning, estimated Cmax = 0. Calling EstimateCmax()."); EstimateCmax(); } // has to be done before calculating RMs
            foreach (string name in Constants.RMNames)
            {
                RMs.Add(new RM(name));
                RMs[RMs.Count - 1].Calculate(this);
            }
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

        /// <summary>
        /// Calculate ESS than Cmax based on that.
        /// </summary>
        /// <returns></returns>
        public double EstimateCmax()
        {
            //placeholder;
            //Console.WriteLine("Warning: ESS recalculated, Cmax based on new ESS times");
            CalcESS();
            SetESS();
            double Maximum = 0;
            int MaxID = 0;
            for (int i = 1; i < Starttimes.Length; i++)
            {
                if (Starttimes[i] == -1) { throw new Exception("Startimes not calculated yet"); }
                if(Starttimes[i] + PrecedenceDAG.GetJobById(i).MeanProcessingTime > Maximum)
                {
                    Maximum = Starttimes[i] + PrecedenceDAG.GetJobById(i).MeanProcessingTime;
                    MaxID = i;
                }
            }
            EstimatedCmax = Maximum;

         //   Console.WriteLine("Debug: Cmax is estimated to be {0}", EstimatedCmax);
            return EstimatedCmax;
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
            AssignmentDescription = "GreedyLoadBalancing";
            AssignJobsBy(GreedyLoadBalancing);
        }

        public void MakeRandomAssignment()
        {
            AssignmentDescription = "Random";
            AssignJobsBy(RandomMachineAssignment);
        }

        /// <summary>
        /// Given a problem with precedence arcs as a DAG. Creates an assignment by iterating jobs over machines. Very naive.
        /// </summary>
        public void AssignByRolling()
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
        /// Prints a summary of the current schedule
        /// </summary>
        public void Print()
        {
            Console.WriteLine("*** Schedule information: *** ");
          //  Console.WriteLine("* Mean Cmax determined to be: {0}", EstimateCmax());
            Console.WriteLine("* Machine info:");
            
            foreach (Machine m in Machines)
            {
                Console.Write("*");
                Console.Write("  M {0}: ", m.MachineID);
                foreach(Job j in m.AssignedJobs)
                {
                    Console.Write("{0}, ", j.ID);
                }
                Console.Write(" *");
                Console.Write(Environment.NewLine);
            }
            Console.WriteLine("*****************************");
            /*
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
            */

        }
        private void DebugPrintJobId(Job j)
        {
            Console.WriteLine(j.ID);
        }

        public void MakeHTMLImage(string title)
        {
            System.IO.Directory.CreateDirectory(string.Format(@"{0}Results\Schedules\", Program.BASEPATH));
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(string.Format(@"{0}Results\Schedules\{1}.html",Program.BASEPATH,title) ))
            {
                file.WriteLine(@"<!DOCTYPE html>");
                file.WriteLine(@"<head>");
                file.WriteLine(@"<style>");

                // css part:
                int scale = 15;
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
                //CmaxBlok:
                top = 50 ;
                EstimateCmax();
                left = EstimatedCmax * scale;
                width = 2 * scale;
                double height = 50 * Machines.Count + 100;
                file.WriteLine("div.CMAX");
                file.WriteLine("{position: fixed;");
                file.WriteLine("top: {0}px; left: {1}px; width: {2}px;height: {3}px;", top, left, width,height);
                file.WriteLine(@" border: 1px solid #73AD21; text-align: center; vertical-align: middle; colour: red;}");


                file.WriteLine(@"</style></head><body>");
                file.WriteLine(@"<div>");
                file.WriteLine(title);
                file.WriteLine(@"</div>");
                foreach (Job j in PrecedenceDAG.Jobs)
                {
                    file.WriteLine("<div class=\"j{0}\"> J{0}; </div>",j.ID);
                }
                file.WriteLine("<div class=\"CMAX\"> Cmax = {0}; </div>", EstimatedCmax);
                file.WriteLine(@"</body></html>");
            }
        }


        public void SetESS()
        {
            foreach (Job j in PrecedenceDAG.Jobs)
            {
                if (ESS[j.ID] == 0) { //Console.WriteLine("WARNING: ESS is 0 for job {0}. Did you calculate ESS?", j.ID);
                }
                Starttimes[j.ID] = ESS[j.ID];
            }
        }

        public void SetLSS()
        {
            foreach (Job j in PrecedenceDAG.Jobs)
            {
                if (LSS[j.ID] == 0) {// Console.WriteLine("WARNING: LSS is 0 for job {0}. Did you calculate LSS?", j.ID);
                }
                Starttimes[j.ID] = LSS[j.ID];
            }
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


        public void ForEachSuccDo(Job j, Action<Job> ApplyAction)
        {
            Job MSucc = GetMachineSuccessor(j);
            ApplyAction(MSucc);
            foreach (Job v in j.Successors)
            {
                if (v != MSucc) { ApplyAction(v); }
            }
        }

        private List<Job> BuildSuccessorList(Job j)
        {
            Console.WriteLine("WARNING: BuildSuccessorList is slow. Are you sure you need it?");
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

        public void CalcLSS()
        {
            if (Starttimes[1] < 0)
            {
                // start times not calculated yet. ESS needed to estimate Cmax.
                CalcESS();
                SetESS();
            }
            EstimateCmax();
            ForeachJobInReversePrecOrderDo(Update_LSS_StartDateFor);
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
                else
                {/* todo delete debugging statements (all this commented stuff)
                    Console.Write("Job {0} has pred: ", j.ID);
                    foreach (Job pred in j.Predecessors)
                    {
                        Console.Write("  {0}", pred.ID);
                    }
                    if (GetMachinePredecessor(j) != null)
                    {
                        Console.Write("  (Mpred:) {0}", GetMachinePredecessor(j).ID);
                    }
                    Console.Write(Environment.NewLine);
                    */
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

                Job MachineSucc = GetMachineSuccessor(CurrentJob);
               
                /* debugging: todo, delete
                Console.Write("Job {0} has successors:", CurrentJob.ID);
                foreach (Job Child in CurrentJob.Successors)
                {
                    Console.Write("  {0}",Child.ID);
                }
                if (MachineSucc != null) {
                    Console.Write("   (Msuc:) {0}", MachineSucc.ID);
                }
                Console.WriteLine();
                */

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
             for(int i = 0; i < PrecedenceDAG.N; i++)
            {
                if (!IsVisited[i]) {
                    Console.WriteLine("ERROR! Printing Schedule for debugging then aborting.");
                    Console.WriteLine("Processed {0}/{1} jobs in Prec Order", DebugJobsPopped, PrecedenceDAG.N);
                    Console.WriteLine("Checking for cycles...");
                    bool Cyclesfound = false;
                    foreach (Machine M in this.Machines)
                    {
                        for (int higherindex = 1; higherindex < M.AssignedJobs.Count; higherindex ++)
                        {
                            for (int lowerindex = 0; lowerindex < higherindex; lowerindex++)
                            {
                                if (PrecedenceDAG.PrecPathExists(M.AssignedJobs[higherindex], M.AssignedJobs[lowerindex]))
                                {
                                    Console.WriteLine("Cycle found on M{2}: Job {0} --Marcs--> Job {1} --Precs--> Job {0}", M.AssignedJobs[lowerindex].ID, M.AssignedJobs[higherindex].ID,M.MachineID);
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


        public int[] GetJobIdsTopologically()
        {
            throw new Exception("Potentially bugged, fix before continuing");
            int[] TopoIds = new int[PrecedenceDAG.N];
            int index = 0;
            ForeachJobInPrecOrderDo((j => TopoIds[index++] = j.ID)); // Assing j.ID to TopoIds[index], then increment index.
            return TopoIds;
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
        /// Checks the feasibility of a machine assignment for both machine and prec arcs.
        /// </summary>
        /// <param name="j"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        bool IsFeasibleAssignment(Job j, Machine m)
        {
            if (AssignedMachineID[j.ID] > 0) { throw new System.Exception("Job already assigned!"); }
            else
            {
                if (m.LastJob() == null)
                {
                    // then the machine is empty and the assignment is always feasible.
                    return true;
                }
                else if (PrecAndMachinePathExists(j, m.LastJob()))
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
        private bool PrecAndMachineBFSfrom(Job OriginJob, Func<Job,bool> ApplyLogic)
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

        public bool PrecMachPathExistsWithout(Job OriginJob, Job TargetJob, Tuple<Job,Job> ForbiddenArc)
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

     
        void AssignJobToMachine(Job j, Machine m)
        {
            if (m.MachineID == 0) { throw new Exception("Machines are 1 based"); }
            if (GetMachineArcPointer(j) != null) { throw new System.Exception("Job already assigned! If you want to reassign use Reassign function"); }
            else
            {
                m.AssignedJobs.Add(j);
                MachineArcPointers[j.ID] = new MachineArcPointer(m.MachineID, m.AssignedJobs.Count - 1);
                m.Load += j.MeanProcessingTime;
                AssignedMachineID[j.ID] = m.MachineID;
            }
        }

        public void DeleteJobFromMachine(Job J)
        {
            Machine M = Machines[MachineArcPointers[J.ID].MachineId];
            for(int i = MachineArcPointers[J.ID].ArrayIndex + 1; i < M.AssignedJobs.Count; i++)
            {
                MachineArcPointers[M.AssignedJobs[i].ID].ArrayIndex = i - 1;
            }
            M.AssignedJobs.RemoveAt(MachineArcPointers[J.ID].ArrayIndex);
            MachineArcPointers[J.ID].ArrayIndex = -1;
            MachineArcPointers[J.ID].MachineId = -1;            
        }

        public void InsertJobOnMachineAtIndex(Job J, Machine M, int Index)
        {
            M.AssignedJobs.Insert(Index, J);
            MachineArcPointers[J.ID].MachineId = M.MachineID;
            MachineArcPointers[J.ID].ArrayIndex = Index;
            for (int i = Index + 1; i < M.AssignedJobs.Count; i++)
            {
                MachineArcPointers[M.AssignedJobs[i].ID].ArrayIndex = i;
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

        // You had made this private, probably with some reason. Why?
        public Machine GetMachineByID(int MachineID)
        {
            return Machines[MachineID - 1];
        }
    }
}
