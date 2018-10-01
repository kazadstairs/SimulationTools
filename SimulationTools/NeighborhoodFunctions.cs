using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    static class NeighborhoodFunctions
    {
        public static Schedule VNHC(Schedule CurrentSchedule, Func<Schedule, double> FitnessFunction)
        {
           // Console.WriteLine("******************* Starting VNHC ****************" );
            bool NSSaturated = false;
            bool RaRSaturated = false;
            bool ImprovementFound = false;
            for (int _ = 0; _ < 100; _++)
            {
             //   Console.WriteLine("Loopcounter: {0}, NSsaturated: {1}, RaRSaturated:{2}",_,NSSaturated,RaRSaturated);
                if (NSSaturated && RaRSaturated) { break; }
                else
                {
                    while (NeighborSwaps(CurrentSchedule, FitnessFunction) != null)
                    {
                        //keep doing neighbor swaps
                        ImprovementFound = true;
                        RaRSaturated = false;
                    }
                    NSSaturated = true;
                    while (RemoveAndReinstert(CurrentSchedule, FitnessFunction) != null)
                    {
                        // keep doing remove and reinserts
                        NSSaturated = false;
                        ImprovementFound = true;
                    }
                    RaRSaturated = true;
                }

                if (_ == 99) { Console.WriteLine("VNHC loop TIMEOUT"); }
            }
            //Console.WriteLine("**************** Finished VNHC ****************");
            if (ImprovementFound) { return CurrentSchedule; } else return null;
        }

        /// <summary>
        /// Modifies CurrentSchedule. Returns the modified version after the first improvement, or null if no improvement was possible.
        /// Select a random machine, and then a random pair of jobs on that machine. Swap those jobs if feasible. 
        /// </summary>
        /// <param name=""></param>
        /// <param name="FitnessFunction"></param>
        /// <returns></returns>
        public static Schedule NeighborSwaps(Schedule CurrentSchedule, Func<Schedule, double> FitnessFunction)
        {
            int StartMachineId = DistributionFunctions.UniformInt(CurrentSchedule.Problem.NMachines - 1) + 1; // 1 to 11 (incl)
            int CurrentMachineId = StartMachineId;
            int NMachinesTried = 0;
            // bool improvementFound = false;
            Machine CurrentMachine = null;
            Job J1 = null;
            Job J2 = null;
            double CurrentFitness;
            double NewFitness;
            while (NMachinesTried < CurrentSchedule.Problem.NMachines)
            {
                // CurrentMachine = CurrentSchedule.Machines[CurrentMachineId - 1];
                CurrentMachine = CurrentSchedule.GetMachineByID(CurrentMachineId);
                int JobsOnMachine = CurrentMachine.AssignedJobs.Count;
                if (JobsOnMachine <= 1)
                {
                    // swap no good, proceed to update counters
                }
                else
                {
                    int StartJobIndex = DistributionFunctions.UniformInt(CurrentMachine.AssignedJobs.Count - 1); // -1 because the last job cannot go right
                    int NJobstried = 0;
                    int CurrentJobIndex = StartJobIndex;
                    while (NJobstried < CurrentMachine.AssignedJobs.Count - 1)
                    {

                        J1 = CurrentMachine.AssignedJobs[CurrentJobIndex];
                        J2 = CurrentMachine.AssignedJobs[CurrentJobIndex + 1];
                        CurrentFitness = FitnessFunction(CurrentSchedule);
                        if (NeighborSwapFeasible(J1, J2, CurrentMachine, CurrentSchedule))
                        {
                            UNSAFE_SwapOnMachine(J1, J2, CurrentMachine, CurrentSchedule);
                            NewFitness = FitnessFunction(CurrentSchedule);
                            if (NewFitness > CurrentFitness)
                            {
                                // First Improvement:
                                return CurrentSchedule;
                            }
                            else
                            {
                                // undo swap
                                UNSAFE_SwapOnMachine(J1, J2, CurrentMachine, CurrentSchedule);
                                CurrentSchedule.EstimateCmax();

                                //look at other swap:

                            }
                        }


                        CurrentJobIndex++;
                        if (CurrentJobIndex == CurrentMachine.AssignedJobs.Count - 1)
                        {
                            CurrentJobIndex = 0;
                        }
                        NJobstried++;
                    }
                }
                //this machine was no good, try the next
                NMachinesTried++;
                CurrentMachineId++;
                if (CurrentMachineId > CurrentSchedule.Problem.NMachines) { CurrentMachineId = 1; }
            }
            //tried all jobs on all machines, no improvement
            return null;
        }


        /// <summary>
        /// Mastrolilli and Garambella: Remove job from Machine Graph, reinsert feasibly.
        /// </summary>
        /// <param name="CurrentSchedule"></param>
        /// <param name="FitnessFunction"></param>
        /// <returns></returns>
        public static Schedule RemoveAndReinstert(Schedule CurrentSchedule, Func<Schedule, double> FitnessFunction)
        {
            //Qk: CurrentSchedu
            //loop over all jobs:
            int MoveJobID = DistributionFunctions.UniformInt(CurrentSchedule.PrecedenceDAG.N);
            int NjobsTried = 0;
            Job MoveJob;
            //Schedule FullSchedule = new Schedule(CurrentSchedule);
            double OriginalFitness = FitnessFunction(CurrentSchedule);
            CurrentSchedule.CalcLSS();
            

            while (NjobsTried < CurrentSchedule.PrecedenceDAG.N)
            {
              //  CurrentSchedule = FullSchedule; // reset the current schedule to the original graph (doesn´t work?)
                MoveJob = CurrentSchedule.PrecedenceDAG.GetJobById(MoveJobID);
                Machine CurrentMachine = CurrentSchedule.GetMachineByJobID(MoveJob.ID);
                int PositionOfJobBeforeRemoval = CurrentMachine.GetJobIndex(MoveJob);
                Schedule ReducedGraph = new Schedule(CurrentSchedule);
                ReducedGraph.DeleteJobFromMachine(MoveJob);

                double EarliestStartofMoveJob = CurrentSchedule.CalcStartTimeOfMoveJob(MoveJob);
                double TailTimeofMoveJob = CurrentSchedule.CalcTailTimeOfMoveJob(MoveJob);

                Machine NewMachineCandidate = null; //todo

                int NMachinesTried = 0;
                int CandidateMachineID = DistributionFunctions.UniformInt(CurrentSchedule.Machines.Count - 1) + 1; //-1 because we do not want to select the current machine, +1 because machines are 1 based
                if (CandidateMachineID >= CurrentMachine.MachineID) { CandidateMachineID++; } // correct for the -1.
                while (NMachinesTried < CurrentSchedule.Machines.Count - 1) // -1, because we do not try to reinsert on the same machine.
                {

                    //bool CaseB = false; //L,R Intersection Nonempty case
                    //bool CaseA = false;
                   // Console.WriteLine("Trying to assign J{0} to M{1}", MoveJob.ID, CandidateMachineID);
                    NewMachineCandidate = CurrentSchedule.GetMachineByID(CandidateMachineID);
                    // foreach (Job X in NewMachineCandidate.AssignedJobs) FOREACH does not allow editting the list. (conflict with line 139: AssignJbeforeX
                    for(int i = 0; i < NewMachineCandidate.AssignedJobs.Count; i++)
                    {
                       // Console.Write("On M{1} at index {0}... ", i, NewMachineCandidate.MachineID);
                        Job X = NewMachineCandidate.AssignedJobs[i];
                        bool XinL = CurrentSchedule.XIsInL(X, TailTimeofMoveJob);
                        bool XinR = CurrentSchedule.XIsInR(X, EarliestStartofMoveJob);
                        if (!(XinL ^ XinR)) // Not XOR XinL,XinR
                        {
                         //   Console.WriteLine(string.Format("J{0} ",X.ID) + ((XinL) ? "in L" : "not in L"));
                         //   Console.WriteLine(string.Format("J{0} ", X.ID) + ((XinR) ? "in R" : "not in R"));
                           // Console.WriteLine("BEFORE:");
                            //CurrentSchedule.Print();
                            if (MoveAndInsertIsImprovement(CurrentSchedule, MoveJob, EarliestStartofMoveJob, TailTimeofMoveJob, NewMachineCandidate, i, OriginalFitness))
                            {
                                return CurrentSchedule;
                            }
                            else
                            {
                                // undo assignment is done in MoveAndInsertIsImprovement
                            }
                            //Console.WriteLine("AFTER:");
                            //CurrentSchedule.Print();
                            //feasible
                        }
                        else if (!XinL && XinR)
                        {
                           // Console.WriteLine("Not feasible");
                            break;
                            //no feasible exists
                        }
                        else if (XinL && !XinR)
                        {
                           // Console.WriteLine("Not feasible");
                            continue;
                            //skip
                        }
                        else
                        {

                            Console.WriteLine("X " + ((XinL) ? "in L" : "not in L"));
                            Console.WriteLine("X " + ((XinR) ? "in R" : "not in R"));
                            throw new Exception("Go back to Logic 1.01!");
                        }
                        
                    }//end of trying all jobs on Machine

                    NMachinesTried++;
                    CandidateMachineID++;
                    if (CandidateMachineID == CurrentMachine.MachineID) { CandidateMachineID++; } //Skip the original machine
                    if (CandidateMachineID > CurrentSchedule.Machines.Count) { CandidateMachineID = 1; } // Loop round, machine ids start at 1

                }//end of loop over all machines

                //No improvement for this job on any machine.
                //try the next job
                //Console.WriteLine("No improvement by remove and insert for J{0} on any machine. RESETTING",MoveJobID);
                //reinsert the job on the original machine at its original position.
                //CurrentSchedule.AssignJatIndex(MoveJob,CurrentMachine,PositionOfJobBeforeRemoval);
                NjobsTried++;
                MoveJobID++;
                if (MoveJobID >= CurrentSchedule.PrecedenceDAG.N) { MoveJobID = 1; } // no point trying to move the dummy job
            }//end of loop over all jobs

            //no job has any improvement on any machine: Local optimum. Return null to HC function.
            return null;
            
        }

        /// <summary>
        /// ONLY works for MeanBasedCmax
        /// </summary>
        private static bool MoveAndInsertIsImprovement(Schedule ScheduleBeforeMove,Job MoveJob, double ESSv_withoutMachinePred, double Tailtimev_withoutMachinePred, Machine NewMachine, int NewPosIndex, double Currentfitness)
        {
            // note Cmax >= Sx + Px + Tx
            
            double Sv, Tv;
            if (NewPosIndex == 0)
            {
                Sv = ESSv_withoutMachinePred;
            }
            else
            {
                Job Mpred = NewMachine.AssignedJobs[NewPosIndex - 1];
                Sv = Math.Max(ESSv_withoutMachinePred, ScheduleBeforeMove.GetEarliestStart(Mpred) + Mpred.MeanProcessingTime);
            }
            if (NewPosIndex == NewMachine.AssignedJobs.Count - 1)
            {
                Tv = Tailtimev_withoutMachinePred;
            }
            else
            {
                Job Msucc = NewMachine.AssignedJobs[NewPosIndex]; // V has not been inserted yet, so this job will be its successor
                Tv = Math.Min(Tailtimev_withoutMachinePred, ScheduleBeforeMove.CalcTailTime(Msucc) + Msucc.MeanProcessingTime);
            }

            if (-(Sv + MoveJob.MeanProcessingTime + Tv) <= Currentfitness + 0.0000001)
            {
                //Console.WriteLine("Rejected by bound.. no improvement (no swapping needed)");
                return false;
            }
            else
            {
                // Do it the hard way.
                Machine OldMachine = ScheduleBeforeMove.AssignedMachine(MoveJob);
                int OldPosition = ScheduleBeforeMove.GetIndexOnMachine(MoveJob);
                ScheduleBeforeMove.DeleteJobFromMachine(MoveJob);
                ScheduleBeforeMove.AssignJatIndex(MoveJob, NewMachine, NewPosIndex);
                double NewFitness = FitnessFunctions.MeanBasedCmax(ScheduleBeforeMove);
                
                if (NewFitness > Currentfitness)
                {
                    //improvement
                    return true;
                }
                else
                {
                    //no improvement
                    //undo swap
                    //reset start times.
                    ScheduleBeforeMove.DeleteJobFromMachine(MoveJob);
                    ScheduleBeforeMove.AssignJatIndex(MoveJob, OldMachine, OldPosition);
                    ScheduleBeforeMove.EstimateCmax();
                    return false;
                }

            }

        }
        

        static private bool NeighborSwapFeasible(Job LeftJob, Job RightJob, Machine M, Schedule Sched)
        {
            // Console.WriteLine("Testing J{0},J{1} (M{2}) swap for creating cycles...",LeftJob.ID,RightJob.ID,M.MachineID);
            int OldJ1Index = Sched.GetIndexOnMachine(LeftJob);
         //       Sched.MachineArcPointers[LeftJob.ID].ArrayIndex;
            if (M.AssignedJobs[OldJ1Index] != LeftJob) { throw new Exception("Indexing wrong. J1 index not pointing to J1"); }
            int OldJ2Index = Sched.GetIndexOnMachine(RightJob);
            //Sched.MachineArcPointers[RightJob.ID].ArrayIndex;
            if (M.AssignedJobs[OldJ2Index] != RightJob) { throw new Exception("Indexing wrong. J2 index not pointing to J2"); }
            if (OldJ1Index + 1 != OldJ2Index) { throw new Exception("Jobs must be neighbours for this swap!"); }

            // 'remove' the machine arc from LeftJob to RightJob
            // If path still exists from Left To Right, swap is not feasible
            if (Sched.PrecMachPathExistsWithout(LeftJob, RightJob, new Tuple<Job, Job>(LeftJob, RightJob)))
            {
                // not feasible
    //            Console.WriteLine("DENIED (will create cycle)");
                return false;
            }
            else
            {
   //             Console.WriteLine("ALLOWED");
                return true;
            }
        }

        /// <summary>
        /// Swap the jobs on a machine without any checks.
        /// </summary>
        /// <param name="J1"></param>
        /// <param name="J2"></param>
        /// <param name="M"></param>
        /// <param name="Sched"></param>
        static private void UNSAFE_SwapOnMachine(Job J1, Job J2, Machine M, Schedule Sched)
        {/*
            Console.WriteLine("**************************************************");
            Console.WriteLine(",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,");
            Console.WriteLine("**************************************************");
            Console.WriteLine("S W A P   S T A R T I N G ! (JOB J{0} with JOB J{1}",J1.ID,J2.ID);
            Console.WriteLine("Schedule BEFORE Swap:");
            Sched.Print();
            Console.WriteLine("Fitness BEFORE swap: {0}", FitnessFunctions.MeanBasedCmax(Sched));
            */

            int OldJ1Index = Sched.GetIndexOnMachine(J1); // Sched.MachineArcPointers[J1.ID].ArrayIndex;
            int OldJ2Index = Sched.GetIndexOnMachine(J2); // Sched.MachineArcPointers[J2.ID].ArrayIndex;
            M.AssignedJobs[OldJ1Index] = J2;
            M.AssignedJobs[OldJ2Index] = J1;
            //update the pointers:
        //    Sched.MachineArcPointers[J1.ID].ArrayIndex = OldJ2Index;
            Sched.AssignedMachine(J1).SetJobIndex(J1,OldJ2Index);
            Sched.AssignedMachine(J2).SetJobIndex(J2,OldJ1Index);
        //    Sched.MachineArcPointers[J2.ID].ArrayIndex = OldJ1Index;
/*
            Console.WriteLine("Schedule AFTER Swap:");
            Sched.Print();
            Console.WriteLine("Fitness AFTER swap: {0}", FitnessFunctions.MeanBasedCmax(Sched));
            Console.WriteLine("**************************************************");
            Console.WriteLine("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^");
            Console.WriteLine("**************************************************");
            */
        }


        static private bool SwapJobPair(Job J1, Job J2, Machine M, Schedule Sched)
        {
            Console.WriteLine("S W A P   S T A R T I N G !");
            Console.WriteLine("Schedule BEFORE Swap:");
            Sched.Print();
            Console.WriteLine("Fitness BEFORE swap: {0}", FitnessFunctions.MeanBasedCmax(Sched));
            //The bug is that Arrayindex is not the correct position of the job.
            //Give each job a dictionary of all its transitive descendants. Check in almost O(1) if swap is feasible. (MUCH BETTER THAN BFS).
            int OldJ1Index = Sched.GetIndexOnMachine(J1);// Sched.MachineArcPointers[J1.ID].ArrayIndex;
            if (M.AssignedJobs[OldJ1Index] != J1) { throw new Exception("Indexing wrong. J1 index not pointing to J1"); }
            int OldJ2Index = Sched.GetIndexOnMachine(J2);// Sched.MachineArcPointers[J2.ID].ArrayIndex;
            if (M.AssignedJobs[OldJ2Index] != J2) { throw new Exception("Indexing wrong. J2 index not pointing to J2"); }
            int LeftIndex = OldJ1Index;
            int RightIndex = OldJ2Index;
            if (OldJ2Index < OldJ1Index) { LeftIndex = OldJ2Index; RightIndex = OldJ1Index; }
            // check feasibility of the swap, Assuming an already feasible setup.
            for (int i = LeftIndex; i <= RightIndex - 1; i++)
            {

                for (int j = i + 1; j <= RightIndex; j++)
                {
                    // Between [J1 and J2] (inlusive) the arcs get reversed. Check all combos.
                    //Do they though? TODO BUG!
                    if (Sched.PrecedenceDAG.PrecPathExists(M.AssignedJobs[i], M.AssignedJobs[j]))
                    {
                       // Console.WriteLine("BUG?");
                        Console.WriteLine("Swap J{0},J{1} on M{2} denied due to cycle from J{3} to J{4}",J1.ID,J2.ID,M.MachineID,M.AssignedJobs[i].ID,M.AssignedJobs[j].ID);
                        return false;
                    } // infeasible.
                }
            }
            Console.WriteLine("Swapping J{0}, J{1} on M{2}", J1.ID, J2.ID, M.MachineID);

            UNSAFE_SwapOnMachine(J1, J2, M, Sched);

            Console.WriteLine("Schedule AFTER Swap:");
            Sched.Print();
            Console.WriteLine("Fitness AFTER swap: {0}", FitnessFunctions.MeanBasedCmax(Sched));
            return true;
        }

    }
}
