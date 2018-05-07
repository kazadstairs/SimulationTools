using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    class Job
    {
        public int id { get; private set; } // Identifier to recognize the job
        public double ReleaseDate { get; private set; } // earliest start date of the job
        private double ProcessingTime;
        public List<Job> Predecessors { get; private set; }
        public List<Job> Successors;

        public int nPredComplete { get; private set; }
        public bool allPredComplete { get; private set; }

        // Schedule properties:
        public Machine Machine { get; private set; } //Machine to which job is assigned in the schedule
        public bool IsAssigned { get; private set; } //true if the job has been assigned, false if not
        public double ScheduleStartTime; //sj: The start time of the job in the schedule

        bool DebugIsAvailable;

        public Job(int _id, double pj, double rj, Job predecessor)
        {
            id = _id;
            ProcessingTime = pj;
            ReleaseDate = rj;

            Predecessors = new List<Job>();
            Predecessors.Add(predecessor);
            Successors = new List<Job>();

            IsAssigned = false;
            DebugIsAvailable = false;
            ScheduleStartTime = -1;
            foreach (Job j in Predecessors)
            {
                j.Successors.Add(this);
            }

            nPredComplete = 0;
        }

        public Job(int _id, double pj, double rj, List<Job> predecessors)
        {
            id = _id;
            ProcessingTime = pj;
            ReleaseDate = rj;

            Predecessors = predecessors;            
            Successors = new List<Job>();

            IsAssigned = false;
            DebugIsAvailable = false;
            ScheduleStartTime = -1;
            foreach (Job j in Predecessors)
            {
                j.Successors.Add(this);
            }

            nPredComplete = 0;
            if (predecessors.Count == 0) { allPredComplete = true; }
        }

        /// <summary>
        /// Use this function to tell a job that its predecessor has been completed. Will update number of completed predecessors
        /// </summary>
        public void PredComplete()
        {
            nPredComplete++;
            if(nPredComplete == Predecessors.Count) { allPredComplete = true; }
        }

        public double GetProcessingTime()
        {
            //todo: make this stochastic
            return ProcessingTime;
        }

        public bool IsAvailableAt(double time)
        {
            if (DebugIsAvailable) { throw new Exception("Job Available for a second time!"); }
            else if((ReleaseDate <= time) 
                    && (allPredComplete) //|| Predecessors.Count == 0
                    && (ScheduleStartTime <= time) 
                    )
            {
                DebugIsAvailable = true;
                return true;
            }
            return false;
            
        }

        public void AssignToMachine(Machine M)
        {
            Machine = M ?? throw new NullReferenceException("Attempting to assign job to a machine that is not yet instanciated"); //sets Machine to M and throws exception if M is null
            IsAssigned = true;
        }

        public void SetStartTime(double starttime)
        {
            if (!IsAssigned) { throw new Exception("Cannot set start time for a job that is not yet assigned to a machine"); }
            ScheduleStartTime = starttime;
        }

    }
}
