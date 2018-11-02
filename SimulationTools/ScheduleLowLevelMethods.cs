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
            //Console.WriteLine("Assigning J{0} to M{1}",J.ID,M.MachineID);
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
            //Console.WriteLine("Assing J{0} to M{1} at index {2}", J.ID, M.MachineID, Index);
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

        //Test if J is on a critical path.
        public bool IsCritical(Job J)
        {
            return (CalcTailTime(J) + J.MeanProcessingTime + GetEarliestStart(J) >= EstimatedCmax - 0.001);
        }

        public bool IsAlmostCritical(Job J,double relativeIncreaseAllowed)
        {
            return (CalcTailTime(J) + J.MeanProcessingTime + GetEarliestStart(J) >= EstimatedCmax * (1 - relativeIncreaseAllowed));
        }



        public double GetLatestStart(Job J)
        {
            return LSS[J.ID];
        }   

        public double GetEarliestStart(Job J)
        {
        //    if (ESS[J.ID] == 0) { Console.WriteLine("Warning, ESS[J{0}] = 0",J.ID); }
            return ESS[J.ID];
        }

        public double CalcTailTime(Job J)
        {
           // if (GetLatestStart(J) == 0)
           // { Console.WriteLine("WARNING, LSS[J{0}] = 0", J.ID); }
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
            //px + tx > tv
          //  Console.WriteLine("Ltest: T{3} + P{3} > Sv- == {0} + {1} > {2}", CalcTailTime(X), X.MeanProcessingTime, _TailTimeofV, X.ID);
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
            //Console.WriteLine("Rtest: S{3} + P{3} > Sv- == {0} + {1} > {2}", GetStartTimeOfJob(X), X.MeanProcessingTime, _StartTimeofV,X.ID);
            return (GetEarliestStart(X) + X.MeanProcessingTime > _StartTimeofV);
        }

        public double CalcStartTimeOfMoveJob(Job MoveJob)
        {
            double EarliestStartofMoveJob = 0.0;
            double tempStarttime;
            foreach (Job Pred in MoveJob.Predecessors) //intentionally exclude machine pred. Using Sv- = MAX(Si- + pi) = MAX(Si + pi)
            {
          //      Console.WriteLine("Pred: J{0} with S{0} = {1}, P{0} = {2}",Pred.ID,GetEarliestStart(Pred),Pred.MeanProcessingTime);
                tempStarttime = this.GetEarliestStart(Pred) + Pred.MeanProcessingTime;
                if (tempStarttime > EarliestStartofMoveJob) { EarliestStartofMoveJob = tempStarttime; }
            }
          //  Console.WriteLine("No other preds: Sv- = {0}", EarliestStartofMoveJob);
            return EarliestStartofMoveJob;

        }

         public double CalcTailTimeOfMoveJob(Job MoveJob)
        {
            if (MoveJob.ID == 0)
            {
            }
            double MaxTailTime = 0.0;
            double tempTailtime;
            foreach (Job Suc in MoveJob.Successors) //intentionally exclude machine pred. Using Sv- = MAX(Si- + pi) = MAX(Si + pi)
            {
           //     Console.WriteLine("Suc: J{0} with T{0} = {1}, P{0} = {2}",Suc.ID,CalcTailTime(Suc),Suc.MeanProcessingTime);
                tempTailtime = this.CalcTailTime(Suc) + Suc.MeanProcessingTime;
                if (tempTailtime > MaxTailTime) { MaxTailTime = tempTailtime; }
            }
           // Console.WriteLine("No other successors: Tv- = {0}",MaxTailTime);
            return MaxTailTime;
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
