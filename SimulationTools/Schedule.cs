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
        public DirectedAcyclicGraph DAG;
        public List<Machine> Machines;
        public double EstimatedCmax;

        public Schedule()
        {
            DAG = new DirectedAcyclicGraph();
            Machines = new List<Machine>();
        }

        public void InstanciatePinedo()
        {
            DAG.AddJob(new Job(1, 4, 0));
            DAG.AddJob(new Job(2, 9, 0));
            DAG.AddJob(new Job(3, 3, 0));
            DAG.AddJob(new Job(4, 3, 0));
            DAG.AddJob(new Job(5, 6, 0));
            DAG.AddJob(new Job(6, 8, 0));
            DAG.AddJob(new Job(7, 12, 0));
            DAG.AddJob(new Job(8, 8, 0));
            DAG.AddJob(new Job(9, 6, 0));


            // precedence arcs
            DAG.AddArcById(1, 2);
            DAG.AddArcById(2, 6);
            DAG.AddArcById(3, 4);
            DAG.AddArcById(4, 5);
            DAG.AddArcById(5, 6);
            DAG.AddArcById(5, 7);
            DAG.AddArcById(6, 8);
            DAG.AddArcById(7, 8);
            DAG.AddArcById(7, 9);

            // machine arcs all match prec arcs in this instance
            Machines.Add(new Machine(0));
            Machines.Add(new Machine(1));

            /*

            AssignJobToMachineById(1, 0);
            AssignJobToMachineById(2, 0);
            AssignJobToMachineById(6, 0);
            AssignJobToMachineById(8, 0);

            AssignJobToMachineById(3, 1);
            AssignJobToMachineById(4, 1);
            AssignJobToMachineById(5, 1);
            AssignJobToMachineById(7, 1);
            AssignJobToMachineById(9, 1);
            */
        }

        /// <summary>
        /// Given a problem with precedence arcs and jobs in a DAG. Use this function to create a schedule.
        /// </summary>
        public void Build()
        {
            if (DAG.Jobs.Count <= 1)
            {
                throw new Exception("No DAG given. Cannot build schedule without problem instance");
            }
            int NJobsAssigned = 0;
            int[] nParentsProcessed = new int[DAG.N + 1];
            Queue<Job> AllPredDone = new Queue<Job>(); // The jobs that will no longer change Rj are those for which all Parents have been considered.           
            foreach (Job j in DAG.Jobs)  //All jobs without predecessors can know their final Rj (it is equal to their own rj).
            {
                if (j.Predecessors.Count == 0)
                {
                    AllPredDone.Enqueue(j);
                }
            }

            int CandidateMachineID = 0;

            while (AllPredDone.Count > 0)
            {
                // todo: this is not a good way of doing machine assignment.
                bool DebugAssignmentSuccess = false;
                CandidateMachineID++;
                if (CandidateMachineID >= Machines.Count) { CandidateMachineID = 0; }

                Job CurrentJob = AllPredDone.Dequeue();

                for (int i = 0; i < Machines.Count; i++)
                {
                    if (!IsFeasibleAssignment(CurrentJob, Machines[CandidateMachineID]))
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
                        if (Machines[CandidateMachineID].AssignedJobs.Count > 0)
                        {
                            DAG.AddArc(Machines[CandidateMachineID].LastJob(), CurrentJob);
                        }
                        //IMPORTANT: Only do this after the queue has been updated and after machine arcs have been updated                         
                        AssignJobToMachine(CurrentJob, Machines[CandidateMachineID]);

                        DebugAssignmentSuccess = true;
                        break;
                    }
                }
                if (!DebugAssignmentSuccess) { throw new Exception("Schedulde.Build was unable to create a feasible schedule: the algorithm is bugged."); }
                NJobsAssigned++;                
            }
            if (NJobsAssigned < DAG.Jobs.Count)
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
                Console.Write("  M {0}: ", m.id);
                foreach(Job j in m.AssignedJobs)
                {
                    Console.Write("{0}, ", j.ID);
                }
                Console.Write(Environment.NewLine);
            }
            Console.WriteLine("Job info:");
            foreach (Job j in DAG.Jobs)
            {
                Console.WriteLine("  Job {0} is on M {1}. OutArcs:", j.ID, j.Machine.id);
                foreach(Job succ in j.Successors)
                {
                    if (succ.Machine == j.Machine)
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

        /// <summary>
        /// In O(|Vertices| + |Arcs|), determine the earliest Rj for all jobs
        /// </summary>
        public void SetReleaseDates()
        {
            //init
            int[] nParentsProcessed = new int[DAG.N + 1]; // Position i contains the number of parents of Job with ID i that have been fully updated.
            Stack<Job> AllPredDone = new Stack<Job>(); // The jobs that will no longer change Rj are those for which all Parents have been considered.           
            foreach (Job j in DAG.Jobs)  //All jobs without predecessors can know their final Rj (it is equal to their own rj).
            {
                if (j.Predecessors.Count == 0)
                {
                    AllPredDone.Push(j);
                    j.DynamicReleaseDate = j.EarliestReleaseDate;
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
                    CandidateRj = CurrentJob.DynamicReleaseDate + CurrentJob.GetProcessingTime();
                    if (CandidateRj > Child.DynamicReleaseDate) // if new Rj is larger, update Rj
                    {
                        Child.DynamicReleaseDate = CandidateRj;
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
            foreach (Job j in DAG.Jobs)
            {
                Console.WriteLine("Job with ID {0} has Rj {1}", j.ID, j.DynamicReleaseDate);
            }

        }

        public void SetDeadlines(double Cmax)
        {
            //init
            int[] nChildrenProcessed = new int[DAG.N + 1]; // Position i contains the number of parents of Job with ID i that have been fully updated.
            Stack<Job> AllSuccDone = new Stack<Job>(); // The jobs that will no longer change due date are those for which all Parents have been considered.           
            foreach (Job j in DAG.Jobs)  //All jobs without successor can know their final duedate (it is equal to Cmax).
            {
                if (j.Successors.Count == 0)
                {
                    AllSuccDone.Push(j);
                    j.DynamicDueDate = Cmax;
                }
                else
                {
                    j.DynamicDueDate = double.MaxValue;
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
                    CandidateDj = CurrentJob.DynamicDueDate - CurrentJob.GetProcessingTime();
                    if (CandidateDj < Parent.DynamicDueDate) // if new Dj is smaller, update Dj
                    {
                        Parent.DynamicDueDate = CandidateDj;
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
            foreach (Job j in DAG.Jobs)
            {
                Console.WriteLine("Job with ID {0} has Dj {1}", j.ID, j.DynamicDueDate);
            }

            Console.WriteLine("*************************************************");
            Console.WriteLine("Printing Latest Start times:");
            foreach (Job j in DAG.Jobs)
            {
                Console.WriteLine("Job with ID {0} has LSS {1}", j.ID, j.DynamicDueDate - j.GetProcessingTime());
            }

            Console.WriteLine("*************************************************");
            Console.WriteLine("Printing Slack:");
            foreach (Job j in DAG.Jobs)
            {
                Console.WriteLine("Job with ID {0} has Slack {1}", j.ID, j.DynamicDueDate - j.GetProcessingTime() - j.DynamicReleaseDate); //LSS - ESS
            }


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

        bool IsFeasibleAssignment(Job j, Machine m)
        {
            if (j.IsAssigned) { throw new System.Exception("Job already assigned!"); }
            else
            {
                if (DAG.PathExists(j, m.LastJob()))
                {
                    //then adding j to m will create a cycle
                    return false;
                }
            }
            return true;
        }

        void AssignJobToMachine(Job j, Machine m)
        {
            if (j.IsAssigned) { throw new System.Exception("Job already assigned!"); }
            else
            {                
                m.AssignedJobs.Add(j);
                j.AssignToMachine(m);

            }
        }

        void AssignJobToMachineById(int Jid, int Mid)
        {
            AssignJobToMachine(DAG.GetJobById(Jid), Machines[Mid]);
        }
    }
}
