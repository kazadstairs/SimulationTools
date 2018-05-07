using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    class Machine
    {
        public List<Job> AssignedJobs;
        public bool isAvailable;
        public Queue<Job> JobsWaitingToStart;

        public Machine()
        {
            AssignedJobs = new List<Job>();
            JobsWaitingToStart = new Queue<Job>();
        }
    }
}
