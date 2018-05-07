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
        public bool isAssigned { get; private set; } //true if the job has been assigned, false if not
        public double ScheduleStartTime; //sj: The start time of the job in the schedule (

        public Job(int id, double pj, double rj, Job predecessor)
        {
            ProcessingTime = pj;
            ReleaseDate = rj;
            Predecessors = new List<Job>();
            Predecessors.Add(predecessor);
            Successors = new List<Job>();

            foreach (Job j in Predecessors)
            {
                j.Successors.Add(this);
            }

            nPredComplete = 0;
        }

        public Job(int id, double pj, double rj, List<Job> predecessors)
        {
            ProcessingTime = pj;
            ReleaseDate = rj;
            Predecessors = predecessors;
            Successors = new List<Job>();

            foreach (Job j in Predecessors)
            {
                j.Successors.Add(this);
            }

            nPredComplete = 0;
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

        public bool isAvailableAt(double time)
        {
            return (ReleaseDate <= time) && allPredComplete && (ScheduleStartTime <= time);
        }

        public void AssignToMachine(Machine M)
        {
            Machine = M ?? throw new NullReferenceException("Attempting to assign job to a machine that is not yet instanciated"); //sets Machine to M and throws exception if M is null
            isAssigned = true;
        }

        public void SetStartTime(double starttime)
        {
            if (!isAssigned) { throw new Exception("Cannot set start time for a job that is not yet assigned to a machine"); }
            ScheduleStartTime = starttime;
        }

    }
}
