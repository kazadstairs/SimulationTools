using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    // MAIN CLASS, with class definition and constructors.
    // Class divided over 5 files:
    // --> this file:                      
    //          Class declaration, constructor, methods for calculating start times.
    // --> ScheduleLowLevelMethods.cs:
    //          Low level methods for assigning and removing jobs
    // --> ScheduleGraphTraversalMethods.cs:
    //          Methods for processing jobs in order of the graph, or getting jobs by graph relation (e.g. successors)
    // --> ScheduleAssignmentMethods.cs:
    //          High level methods to create Assignments (Random,RMA,GLB etc.)
    // --> SchedulePrintFunctions.cs:
    //          Create HTML output, print to console, etc.

    partial class Schedule
    {
        public DirectedAcyclicGraph PrecedenceDAG;
    //    public MachineArcPointer[] MachineArcPointers { get; private set; } // Maps Job Id to machine it is assigned to and its position in the list.
        public List<Machine> Machines; 
        public ProblemInstance Problem;
        // schedule basics:
        public string AssignmentDescription;
        public string StartTimeDescription;
        private double[] Starttimes;
        private int[] AssignedMachineID; // keeps track of which machine each job is on: MachineId = MachineIdForJobId[j.id]

        // useful vars:
        public double EstimatedCmax;
        private double[] ESS;
        private double[] LSS;
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
                Machines.Add(new Machine(i + 1,PrecedenceDAG.N));
            }

            Starttimes = new double[PrecedenceDAG.N];
            for (int i = 0; i < Starttimes.Length; i++)
            {
                Starttimes[i] = -1;
            }

            AssignedMachineID = new int[PrecedenceDAG.N];
            LSS = new double[PrecedenceDAG.N];
            ESS = new double[PrecedenceDAG.N];
          //  MachineArcPointers = new MachineArcPointer[PrecedenceDAG.N];

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
                Machines.Add(new Machine(i + 1,PrecedenceDAG.N));
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
            //MachineArcPointers = new MachineArcPointer[PrecedenceDAG.N];

            //Copy the information:
            Original.ForeachJobInPrecOrderDo(j => AssignJobToMachineById(j.ID, Original.AssignedMachineID[j.ID]));
            CalcESS();
            SetESS();
            for (int i = 0; i < PrecedenceDAG.N; i++)
            {
                if (GetIndexOnMachine(PrecedenceDAG.GetJobById(i)) != Original.GetIndexOnMachine(PrecedenceDAG.GetJobById(i))) { throw new Exception("Copy mistake"); }
                if (AssignedMachine(PrecedenceDAG.GetJobById(i)) != Original.AssignedMachine(PrecedenceDAG.GetJobById(i))) { throw new Exception("Copy mistake"); }
            }
        }

       
   
        

        //
        // End of  Functions to do with assignment to or removal from machines
        //

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
        

        public Machine GetMachineByJobID(int jobID)
        {
            return GetMachineByID(AssignedMachineID[jobID]);
        }


        // You had made this private, probably with some reason. Why?
        public Machine GetMachineByID(int MachineID)
        {
            return Machines[MachineID - 1];
        }
    }
}
