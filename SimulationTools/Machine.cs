using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    class Machine
    {
        public int MachineID;
        public List<Job> AssignedJobs;
        public double Load;
        public bool isAvailable;
        public Queue<Job> JobsWaitingToStart;
        private int[] IndexOfJob;

        public Machine(int _id, int Njobs)
        {
            AssignedJobs = new List<Job>();
            JobsWaitingToStart = new Queue<Job>();
            MachineID = _id;
            Load = 0;
            IndexOfJob = new int[Njobs];
        }

        public Machine(Machine Original)
        {
            AssignedJobs = new List<Job>();
            for (int i = 0; i < Original.AssignedJobs.Count; i++)
            {
                AssignedJobs[i] = Original.AssignedJobs[i];
            }
            JobsWaitingToStart = new Queue<Job>(); //todo: empty?
            MachineID = Original.MachineID;
            Load = Original.Load;
        }

        public Job LastJob()
        {
            if(AssignedJobs.Count == 0)
            {
                return null;
            }
            else
            {
                return AssignedJobs[AssignedJobs.Count - 1];
            }
        }

        public void SetJobIndex(Job J, int index)
        {
            IndexOfJob[J.ID] = index;
        }

        public int GetJobIndex(Job J)
        {
            return IndexOfJob[J.ID];
        }
    }
}
