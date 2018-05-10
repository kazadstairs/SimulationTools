﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    class Job
    {
        public int ID { get; private set; } // Identifier to recognize the job
        public double ReleaseDate { get; private set; } // earliest start date of the job
        private double ProcessingTime;

        // for graph properties
        public List<Job> Predecessors { get; private set; }
        public List<Job> Successors { get; private set; }
        public bool IsBFSVisited;


        // for simulation
        public int nPredComplete { get; private set; }
        // Schedule properties:
        public Machine Machine { get; private set; } //Machine to which job is assigned in the schedule
        public bool IsAssigned { get; private set; } //true if the job has been assigned, false if not
        public double ScheduleStartTime; //sj: The start time of the job in the schedule

        public bool HasBeenMadeAvailable;

        public Job(int _id, double pj, double rj)
        {
            Init(_id, pj, rj);
        }
        private void Init(int _id, double pj, double rj)
        {
            ID = _id;
            ProcessingTime = pj;
            ReleaseDate = rj;
            Successors = new List<Job>();
            IsBFSVisited = false;
            IsAssigned = false;
            HasBeenMadeAvailable = false;
            ScheduleStartTime = -1;
            Predecessors = new List<Job>();
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

        public double GetProcessingTime()
        {
            //todo: make this stochastic
            return ProcessingTime;
        }

        public bool IsAvailableAt(double time)
        {
            if (HasBeenMadeAvailable) { return false; } // throw new Exception("Job Available for a second time!"); }
            else if ((ReleaseDate <= time)
                    && (AllPredComplete()) //|| Predecessors.Count == 0
                    && (ScheduleStartTime <= time)
                    )
            {
                Console.WriteLine("IsAvailable SET TO TRUE");
                HasBeenMadeAvailable = true;
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

        public bool AllPredComplete()
        {
            if (nPredComplete >= Predecessors.Count)
            {
                Console.WriteLine("ALL PREDECESSORS COMPLETE FOR JOB {0}", ID);
                return true;
            }
            return false;
        }


    }
}
