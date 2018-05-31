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
        public bool isAvailable;
        public Queue<Job> JobsWaitingToStart;

        public Machine(int _id)
        {
            AssignedJobs = new List<Job>();
            JobsWaitingToStart = new Queue<Job>();
            MachineID = _id;
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
    }
}
