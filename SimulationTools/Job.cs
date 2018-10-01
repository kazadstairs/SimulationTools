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

        // Precedence relations
        public List<Job> Predecessors { get; private set; } // NOT INCLUDING MACHINE PREDECESSOR
        public List<Job> Successors { get; private set; } // NOT INCLUDING MACHINE SUCCESSOR
        
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
            Predecessors = new List<Job>();
            //Dist = new Distribution();
        }
        
    }      
}
