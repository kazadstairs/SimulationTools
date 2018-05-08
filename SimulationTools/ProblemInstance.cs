using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    class ProblemInstance
    {
        public List<Job> JobsList;
        public Machine [] Machines;
        public Job DummyStart;

        /// <summary>
        /// Creates a problem instance like that in Pinedo, page 116: figure 5.2. However Job 8 and 7 have been exchanged
        /// </summary>
        public void InstanciatePinedo()
        {
            List<Job> NoPredecessors = new List<Job>();
            JobsList = new List<Job>();
            DummyStart = new Job(0, 0, 0, NoPredecessors);
            JobsList.Add(DummyStart);

            

            JobsList.Add(new Job(1, 4, -1, DummyStart));
            JobsList.Add(new Job(2, 9, -1, JobsList[1]));
            JobsList.Add(new Job(3, 3, -1, DummyStart));
            JobsList.Add(new Job(4, 3, -1, JobsList[3]));
            JobsList.Add(new Job(5, 6, -1, JobsList[4]));

            List<Job> tempPredecessors = new List<Job>();
            tempPredecessors.Add(JobsList[2]);
            tempPredecessors.Add(JobsList[5]);
            JobsList.Add(new Job(6, 8, -1, tempPredecessors));
            tempPredecessors.Clear();
            JobsList.Add(new Job(7, 12, -1, JobsList[5]));
            tempPredecessors.Add(JobsList[6]);
            tempPredecessors.Add(JobsList[7]);
            JobsList.Add(new Job(8, 8, -1, tempPredecessors));
            JobsList.Add(new Job(9, 6, -1, JobsList[7]));

            Machines = new Machine[2];
            for(int i = 0; i < Machines.Length; i++)
            {
                Machines[i] = new Machine(i);
            }


        }
    }
}
