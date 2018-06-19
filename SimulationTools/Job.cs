using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    /// <summary>
    /// Fixed job, part of problem instance. Not to be updated during schedule building or simulation
    /// </summary>
    class Job
    {
        public int ID { get; private set; } // Identifier to recognize the job
        public double EarliestReleaseDate { get; private set; } // earliest start date of the job. Should NEVER be changed
        public double MeanProcessingTime { get; private set; }
        //private Distribution Dist;

        // for graph properties
        public List<Job> Predecessors { get; private set; } // NOT INCLUDING MACHINE PREDECESSOR
        public List<Job> Successors { get; private set; } // NOT INCLUDING MACHINE SUCCESSOR
        //public bool IsBFSVisited;


        // for simulation
        
        //public bool HasBeenMadeAvailable;

        /*
        // Schedule properties: TODO move these out of Job class. (job should not know these).
        public Machine Machine { get; private set; } //Machine to which job is assigned in the schedule
        public bool IsAssigned { get; private set; } //true if the job has been assigned, false if not
        public double DynamicReleaseDate; // earliest start date of the job in a schedule
        public double DynamicDueDate; // earliest start date of the job in a schedule
        // double ScheduleStartTime; //sj: The start time of the job in the schedule
        */
        public Job(int _id, double pj, double rj)
        {
            Init(_id, pj, rj);
        }
        private void Init(int _id, double pj, double rj)
        {
            ID = _id;
            MeanProcessingTime = pj;
            EarliestReleaseDate = rj;
            Successors = new List<Job>();
            //IsBFSVisited = false;
            //IsAssigned = false;
            //HasBeenMadeAvailable = false;
            //ScheduleStartTime = -1;
            Predecessors = new List<Job>();
            //Dist = new Distribution();
        }
    }

    class SimulationJob
    {
        public Job JobParams;
        
        public int nPredComplete { get; private set; }
        public Schedule Sched;
        public List<SimulationJob> Predecessors;
        public List<SimulationJob> Successors;

        public bool HasBeenMadeAvailable;

        public SimulationJob(Job OriginalJob, Simulation Sim)
        {
            JobParams = OriginalJob;
            Predecessors = new List<SimulationJob>(JobParams.Predecessors.Count + 1);
            Successors = new List<SimulationJob>(JobParams.Successors.Count + 1);

            Job Mpred = Sched.GetMachinePredecessor(OriginalJob);
            Predecessors.Add(Sim.SimulationJobs[Mpred.ID]);
            foreach (Job p in OriginalJob.Predecessors)
            {
                if (p != Mpred) { Predecessors.Add(Sim.SimulationJobs[p.ID]); }
            }

            Job MSucc = Sched.GetMachineSuccessor(OriginalJob);
            Successors.Add(Sim.SimulationJobs[MSucc.ID]);
            foreach (Job succ in OriginalJob.Successors)
            {
                if (succ != MSucc) { Successors.Add(Sim.SimulationJobs[succ.ID]); }
            }

            
        }

        public void PredComplete()
        {
            nPredComplete++;
        }

        public bool IsAvailableAt(double time)
        {
            if (HasBeenMadeAvailable) { return false; } // throw new Exception("Job Available for a second time!"); }
            if ((JobParams.EarliestReleaseDate <= time)
                    && (AllPredComplete()) //|| Predecessors.Count == 0
                    && (Sched.GetStartTimeOfJob(JobParams) <= time)
                    )
            {
                HasBeenMadeAvailable = true;
                return true;
            }
            return false;
        }

        public bool AllPredComplete() // todo, just return comparison
        {
            if (nPredComplete >= Predecessors.Count)
            {
                Console.WriteLine("ALL PREDECESSORS COMPLETE FOR JOB {0}",JobParams.ID);
                return true;
            }
            return false;
        }

        public double SampleProcessingTime()
        {
            //todo: this is arbitrary std dev
            return Distribution.SampleNormal(JobParams.MeanProcessingTime, 1.0);
        }



        public void ResetSimulationVars()
        {
            throw new System.NotImplementedException();
        }


    }
        /*
        public Job(int _id, double pj, double rj, Job predecessor)
        {
            Init(_id, pj, rj);

            Predecessors = new List<Job>();
            Predecessors.Add(predecessor);
            foreach (Job j in Predecessors)
            {
                j.Successors.Add(this);
            }

            nPredComplete = 0;
        }*/
        /*
        public Job(int _id, double pj, double rj, List<Job> predecessors)
        {
            Init(_id, pj, rj);

            Predecessors = predecessors;
            foreach (Job j in Predecessors)
            {
                j.Successors.Add(this);
            }

            nPredComplete = 0;
        }
        */

        /* TODO MOVE THESE TO SIMJOB 
        /// <summary>
        /// Use this function to tell a job that its predecessor has been completed. Will update number of completed predecessors
        /// </summary>
        

 
/*
        public void AssignToMachine(Machine M)
        {
            Machine = M ?? throw new NullReferenceException("Attempting to assign job to a machine that is not yet instanciated"); //sets Machine to M and throws exception if M is null
            IsAssigned = true;
        






    */

}
