using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    class Job
    {
        public int ID { get; private set; } // Identifier to recognize the job
        public double EarliestReleaseDate { get; private set; } // earliest start date of the job. Should NEVER be changed
        public double MeanProcessingTime { get; private set; }
        //private Distribution Dist;

        // for graph properties
        public List<Job> Predecessors { get; private set; }
        public List<Job> Successors { get; private set; }
        public bool IsBFSVisited;


        // for simulation
        public int nPredComplete { get; private set; }
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
            IsBFSVisited = false;
            //IsAssigned = false;
            //HasBeenMadeAvailable = false;
            //ScheduleStartTime = -1;
            Predecessors = new List<Job>();
            //Dist = new Distribution();
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


        /// <summary>
        /// Use this function to tell a job that its predecessor has been completed. Will update number of completed predecessors
        /// </summary>
        public void PredComplete()
        {
            nPredComplete++;
        }

        public double SampleProcessingTime()
        {
            //todo: this is arbitrary std dev
            return Distribution.SampleNormal(MeanProcessingTime,1.0);
        }

        /* property of schedule => todo, move to schedule
        public bool IsAvailableAt(double time)
        {
            if (HasBeenMadeAvailable) { return false; } // throw new Exception("Job Available for a second time!"); }
            else if ((EarliestReleaseDate <= time)
                    && (AllPredComplete()) //|| Predecessors.Count == 0
                    && (ScheduleStartTime <= time)
                    )
            {
                HasBeenMadeAvailable = true;
                return true;
            }
            return false;
            
        }
        */
/*
        public void AssignToMachine(Machine M)
        {
            Machine = M ?? throw new NullReferenceException("Attempting to assign job to a machine that is not yet instanciated"); //sets Machine to M and throws exception if M is null
            IsAssigned = true;
        

        public void SetStartTime(double starttime)
        {
            if (!IsAssigned) { throw new Exception("Cannot set start time for a job that is not yet assigned to a machine"); }
            ScheduleStartTime = starttime;
        }
*/
        public bool AllPredComplete() // todo, just return comparison
        {
            if (nPredComplete >= Predecessors.Count)
            {
                //Console.WriteLine("ALL PREDECESSORS COMPLETE FOR JOB {0}", ID);
                return true;
            }
            return false;
        }


        public void ResetSimulationVars()
        {
            nPredComplete = 0;
            //HasBeenMadeAvailable = false;
        }

    }
}
