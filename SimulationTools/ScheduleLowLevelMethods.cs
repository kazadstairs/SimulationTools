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
           // Console.WriteLine("Assigning J{0} to M{1}", PrecedenceDAG.GetJobById(Jid).ID, GetMachineByID(Mid).MachineID);
            AssignJobToMachine(PrecedenceDAG.GetJobById(Jid), GetMachineByID(Mid));
        }

        private void AssignJobToMachine(Job J, Machine M)
        {
            if (M.MachineID == 0) { throw new Exception("Machines are 1 based"); }
            if (AssignedMachine(J)!= null) { throw new System.Exception("Job already assigned! If you want to reassign use Reassign function"); }
            else
            {
                M.AssignedJobs.Add(J);
                M.SetJobIndex(J, M.AssignedJobs.Count - 1);
                //MachineArcPointers[J.ID] = new MachineArcPointer(M.MachineID, M.AssignedJobs.Count - 1);

                M.Load += J.MeanProcessingTime;
                AssignedMachineID[J.ID] = M.MachineID;
                if (GetMachineByID(M.MachineID) != M) { throw new Exception("Indexing bug"); }
            }
        }

        /// <summary>
        /// Insert J into machine M at the position of X, move all other jobs backwards by one position.
        /// </summary>
        /// <param name="J"></param>
        /// <param name="M"></param>
        /// <param name="X"></param>
        public void AssignJbeforeX(Job J, Machine M, Job X)
        {
            ///Debug:
            ///
           // Console.WriteLine("Assing J{0} to M{1} before J{2}...", J.ID, M.MachineID, X.ID);
           // Console.WriteLine("Before assignment:");
           // this.Print();
            ///End of debug
            int CurrentXindex = GetIndexOnMachine(X);
            AssignJatIndex(J, M, CurrentXindex);

            //Debug:
           // Console.WriteLine("After Assignment:");
           // this.Print();
        }

        public void AssignJatIndex(Job J, Machine M, int Index)
        {
            Console.WriteLine("Assing J{0} to M{1} at index {2}", J.ID, M.MachineID, Index);
            M.AssignedJobs.Insert(Index, J);
            M.SetJobIndex(J, Index);
            AssignedMachineID[J.ID] = M.MachineID; // GetMachineArcPointer(J).MachineId = M.MachineID;
            M.Load += J.MeanProcessingTime;

            for (int i = Index + 1; i < M.AssignedJobs.Count; i++)
            {
                M.SetJobIndex(M.AssignedJobs[i], i);
            }

        }

        public void DeleteJobFromMachine(Job J)
        {
            int DEBUG_AssignedMachineID = AssignedMachineID[J.ID];
            Machine M = AssignedMachine(J);
            if (DEBUG_AssignedMachineID != M.MachineID) {
                throw new Exception("Inconsistency in indexing..");
            }
            for (int i = GetIndexOnMachine(J) + 1; i < M.AssignedJobs.Count; i++)
            {
                M.SetJobIndex(M.AssignedJobs[i], i - 1); // all indeces set to 1 below their actual position
            }
            M.AssignedJobs.RemoveAt(GetIndexOnMachine(J)); // remove J, now all indices are correct again
            M.SetJobIndex(J,-1);
            M.Load -= J.MeanProcessingTime;
            AssignedMachineID[J.ID] = -1;
        }

        /// <summary>
        /// Return the Machine J is assigned to, or NULL if J is not assigned.
        /// </summary>
        /// <param name="J"></param>
        /// <returns></returns>
        public Machine AssignedMachine(Job J)
        {
            if (AssignedMachineID[J.ID] <= 0)
            {
                //J unasigned.
                return null;
            }
            else
            {
                return GetMachineByID(AssignedMachineID[J.ID]);
            }
        }



        public double GetLatestStart(Job J)
        {
            return LSS[J.ID];
        }

        public double GetEarliestStart(Job J)
        {
            return ESS[J.ID];
        }

        public double CalcTailTime(Job J)
        {
            return EstimatedCmax - GetLatestStart(J) - J.MeanProcessingTime;
        }

        /// <summary>
        /// Compares job X with the TailTime (Cmax- - LSSv-  - pv) of V IN THE REDUCED GRAPH to determine if X is in L
        /// </summary>
        /// <param name="X"></param>
        /// <param name="_TailTimeofV"></param>
        /// <returns></returns>
        public bool XIsInL(Job X, double _TailTimeofV)
        {
            return (this.CalcTailTime(X) + X.MeanProcessingTime > _TailTimeofV);
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

    /*    public void InsertJobOnMachineAtIndex(Job J, Machine M, int Index)
        {
            M.AssignedJobs.Insert(Index, J);
            MachineArcPointers[J.ID].MachineId = M.MachineID;
            MachineArcPointers[J.ID].ArrayIndex = Index;
            for (int i = Index + 1; i < M.AssignedJobs.Count; i++)
            {
                MachineArcPointers[M.AssignedJobs[i].ID].ArrayIndex = i;
            }
        }*/

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

   //     private MachineArcPointer GetMachineArcPointer(Job j)
    //    {
     //       return MachineArcPointers[j.ID];
     //   }

        public double GetStartTimeOfJob(Job j)
        {
            return Starttimes[j.ID];
        }
        
        
    }
}
