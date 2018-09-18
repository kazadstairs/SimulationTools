using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    //Low level methods for assigning and removing jobs.
    partial class Schedule
    {
        private void AssignJobToMachineById(int Jid, int Mid)
        {
            AssignJobToMachine(PrecedenceDAG.GetJobById(Jid), GetMachineByID(Mid));
        }

        private void AssignJobToMachine(Job j, Machine m)
        {
            if (m.MachineID == 0) { throw new Exception("Machines are 1 based"); }
            if (GetMachineArcPointer(j) != null) { throw new System.Exception("Job already assigned! If you want to reassign use Reassign function"); }
            else
            {
                m.AssignedJobs.Add(j);
                MachineArcPointers[j.ID] = new MachineArcPointer(m.MachineID, m.AssignedJobs.Count - 1);
                m.Load += j.MeanProcessingTime;
                AssignedMachineID[j.ID] = m.MachineID;
            }
        }


        public void DeleteJobFromMachine(Job J)
        {
            Machine M = Machines[MachineArcPointers[J.ID].MachineId];
            for (int i = MachineArcPointers[J.ID].ArrayIndex + 1; i < M.AssignedJobs.Count; i++)
            {
                MachineArcPointers[M.AssignedJobs[i].ID].ArrayIndex = i - 1;
            }
            M.AssignedJobs.RemoveAt(MachineArcPointers[J.ID].ArrayIndex);
            MachineArcPointers[J.ID].ArrayIndex = -1;
            MachineArcPointers[J.ID].MachineId = -1;
            M.Load -= J.MeanProcessingTime;
            AssignedMachineID[J.ID] = -1;
        }



        public double GetLatestStart(Job J)
        {
            return LSS[J.ID];
        }

        public double GetEarliestStart(Job J)
        {
            return ESS[J.ID];
        }

        /// <summary>
        /// Compares job X with the TailTime (Cmax- - LSSv-  - pv) of V IN THE REDUCED GRAPH to determine if X is in L
        /// </summary>
        /// <param name="X"></param>
        /// <param name="_TailTimeofV"></param>
        /// <returns></returns>
        public bool XIsInL(Job X, double _TailTimeofV)
        {
            return (this.EstimatedCmax - GetLatestStart(X) > _TailTimeofV);
        }
        /// <summary>
        /// Compares job X with the starttime of V IN THE REDUCED GRAPH to determine if X is in R
        /// </summary>
        /// <param name="X"></param>
        /// <param name="_StartTimeofV"></param>
        /// <returns></returns>
        public bool XIsInR(Job X, double _StartTimeofV)
        {
            return (GetStartTimeOfJob(X) + X.MeanProcessingTime > _StartTimeofV);
        }

        public void InsertJobOnMachineAtIndex(Job J, Machine M, int Index)
        {
            M.AssignedJobs.Insert(Index, J);
            MachineArcPointers[J.ID].MachineId = M.MachineID;
            MachineArcPointers[J.ID].ArrayIndex = Index;
            for (int i = Index + 1; i < M.AssignedJobs.Count; i++)
            {
                MachineArcPointers[M.AssignedJobs[i].ID].ArrayIndex = i;
            }
        }

        /// <summary>
        /// Checks the feasibility of a machine assignment for both machine and prec arcs.
        /// </summary>
        /// <param name="j"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        private bool IsFeasibleAssignment(Job j, Machine m)
        {
            if (AssignedMachineID[j.ID] > 0) { throw new System.Exception("Job already assigned!"); }
            else
            {
                if (m.LastJob() == null)
                {
                    // then the machine is empty and the assignment is always feasible.
                    return true;
                }
                else if (PrecAndMachinePathExists(j, m.LastJob()))
                {
                    //then adding j to m will create a cycle
                    return false;
                }
            }
            return true;
        }

        private MachineArcPointer GetMachineArcPointer(Job j)
        {
            return MachineArcPointers[j.ID];
        }

        public double GetStartTimeOfJob(Job j)
        {
            return Starttimes[j.ID];
        }
        
        
    }
}
