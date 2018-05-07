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
        public Machine Machine; //Machine to which job is assigned in the schedule
        public bool isAssigned; //true if the job has been assigned, false if not
        public double ScheduleStartTime; //sj: The start time of the job in the schedule (

        public Job(int id, double pj, double rj, List<Job> predecessors)
        {
            ProcessingTime = pj;
            ReleaseDate = rj;
            Predecessors = predecessors;

            foreach(Job j in Predecessors)
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
            if(nPredComplete == Predecessors.Count) { allPredComplete = true};
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

    }
}
