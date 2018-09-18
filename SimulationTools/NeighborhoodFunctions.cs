using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    static class NeighborhoodFunctions
    {
        /// <summary>
        /// Modifies CurrentSchedule. Returns the modified version after the first improvement, or null if no improvement was possible.
        /// Select a random machine, and then a random pair of jobs on that machine. Swap those jobs if feasible. 
        /// </summary>
        /// <param name=""></param>
        /// <param name="FitnessFunction"></param>
        /// <returns></returns>
        public static Schedule NeighborSwaps(Schedule CurrentSchedule, Func<Schedule, double> FitnessFunction)
        {
            int StartMachineId = Distribution.UniformInt(CurrentSchedule.Problem.NMachines - 1) + 1; // 1 to 11 (incl)
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
                    int StartJobIndex = Distribution.UniformInt(CurrentMachine.AssignedJobs.Count - 1); // -1 because the last job cannot go right
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
            Console.WriteLine("Yohoho");
            //Qk: CurrentSchedu
            //loop over all jobs:
            int MoveJobID = Distribution.UniformInt(CurrentSchedule.PrecedenceDAG.N);
            int NjobsTried = 0;
            Job MoveJob;
            Schedule FullSchedule = new Schedule(CurrentSchedule);
            double OriginalFitness = FitnessFunction(FullSchedule);
            

            while (NjobsTried < CurrentSchedule.PrecedenceDAG.N)
            {
                CurrentSchedule = FullSchedule; // reset the current schedule to the original graph
                MoveJob = CurrentSchedule.PrecedenceDAG.GetJobById(MoveJobID);
                Machine CurrentMachine = CurrentSchedule.GetMachineByJobID(MoveJob.ID);
                
                CurrentSchedule.DeleteJobFromMachine(MoveJob);
                CurrentSchedule.CalcESS();
                double EarliestStartofMoveJob = CurrentSchedule.GetEarliestStart(MoveJob);

                CurrentSchedule.EstimateCmax();
                CurrentSchedule.CalcLSS();
                double TailTimeofMoveJob = CurrentSchedule.EstimatedCmax - CurrentSchedule.GetLatestStart(MoveJob) - MoveJob.MeanProcessingTime;

                Machine NewMachineCandidate = null; //todo
                bool CaseB = false; //L,R Intersection Nonempty case
                bool CaseA = false;

                int NMachinesTried = 0;
                int CandidateMachineID = Distribution.UniformInt(FullSchedule.Machines.Count - 1) + 1; //-1 because we do not want to select the current machine, +1 because machines are 1 based
                if (CandidateMachineID >= CurrentMachine.MachineID) { CandidateMachineID++; } // correct for the -1.
                while (NMachinesTried < FullSchedule.Machines.Count - 1) // -1, because we do not try to reinsert on the same machine.
                {
                    Console.WriteLine("Trying to assign J{0} to M{1}", MoveJob.ID, CandidateMachineID);
                    NewMachineCandidate = FullSchedule.GetMachineByID(CandidateMachineID);
                    foreach (Job X in NewMachineCandidate.AssignedJobs)
                    {
                        if (!CaseA && FullSchedule.XIsInL(X, TailTimeofMoveJob))
                        {
                            if (!(FullSchedule.XIsInR(X, EarliestStartofMoveJob)))
                            { continue; } //try next job
                            else
                            {
                                //
                                CurrentSchedule.AssignJbeforeX(MoveJob, NewMachineCandidate, X);
                                double NewFitness = FitnessFunction(CurrentSchedule);
                                // todo: Speedup with upperbound?
                                if (NewFitness > OriginalFitness)
                                {
                                    // improvement found, return
                                    return CurrentSchedule;
                                }
                                else
                                {
                                    // undo assignment
                                    CurrentSchedule.DeleteJobFromMachine(MoveJob);
                                }
                                CaseB = true;
                                continue; //try next job
                            }
                        }
                        if (FullSchedule.XIsInL(X, TailTimeofMoveJob)) { throw new Exception("Continue is not doing what you think it is"); }
                        if (CaseB)
                        {
                            // CaseB but not returned: No improvement found, move on to next machine

                        }
                        if (!CaseB)
                        {
                            if (!FullSchedule.XIsInR(X, EarliestStartofMoveJob))
                            {
                                //Case A, feasible
                                CaseA = true;
                                CurrentSchedule.AssignJbeforeX(MoveJob, NewMachineCandidate, X);
                                double NewFitness = FitnessFunction(CurrentSchedule);
                                // todo: Speedup with upperbound?
                                if (NewFitness > OriginalFitness)
                                {
                                    // improvement found, return
                                    return CurrentSchedule;
                                }
                                else
                                {
                                    // undo assignment
                                    CurrentSchedule.DeleteJobFromMachine(MoveJob);
                                }
                                continue; //try next job
                            }
                            else
                            {
                                // no more feasible solutions exist on this machine.
                                Console.WriteLine("Failed, moving on to next machine.");
                            }
                        }
                    }//end of trying all jobs on Machine

                    NMachinesTried++;
                    CandidateMachineID++;
                    if (CandidateMachineID == CurrentMachine.MachineID) { CandidateMachineID++; } //Skip the original machine
                    if (CandidateMachineID > FullSchedule.Machines.Count) { CandidateMachineID = 0; } // Loop round

                }//end of loop over all machines

                //No improvement for this job on any machine.
                //try the next job
                Console.WriteLine("No improvement by remove and insert for J{0} on any machine",MoveJobID);
                NjobsTried++;
                MoveJobID++;
                if (MoveJobID > CurrentSchedule.PrecedenceDAG.N) { MoveJobID = 0; }
            }//end of loop over all jobs

            //no job has any improvement on any machine: Local optimum. Return null to HC function.
            Console.WriteLine("LOCAL OPTIMUM found");
            return null;
            
        }

        public static Schedule SameMachineSwap(Schedule CurrentSchedule, Func<Schedule, double> FitnessFunction)
        {
            throw new Exception("ERROR, function as is does not work: It can create cycles");
            int StartMachineId = Distribution.UniformInt(CurrentSchedule.Problem.NMachines - 1) + 1; // 1 to 11 (incl)
            int CurrentMachineId = StartMachineId;
            int NMachinesTried = 0;
           // bool improvementFound = false;
            Machine CurrentMachine = null;  
            //Console.WriteLine("TODO LAZY: MAKE above assignment a CALL A FUNCTION IN SCHEDULE!");
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
                    // try all potential swaps
                    Job J1 = null, J2 = null;
                    for (int J1index = 0; J1index < JobsOnMachine - 1; J1index++)
                    {

                        J1 = CurrentMachine.AssignedJobs[J1index];

                        for (int J2index = J1index + 1; J2index < JobsOnMachine; J2index++)
                        {
                            J2 = CurrentMachine.AssignedJobs[J2index];
                            // try the swap
                            double OriginalFitness = FitnessFunction(CurrentSchedule);
                            if (SwapJobPair(J1, J2, CurrentMachine, CurrentSchedule))
                            {

                                double NewFitness = FitnessFunction(CurrentSchedule); // current schedule is updated during the SameMachineSwap function. So this works.
                                if (NewFitness > OriginalFitness) // Trying to minimize here, so maybe VD more appropriate than HC! This maximizes CMAX!
                                {
                                   // improvementFound = true;
                                    //  Console.WriteLine("OPTIMIZING MOVE FOUND (Swap J{0},J{1}, on M{2}). Schedule fitness: {3}",J1.ID,J2.ID,CurrentMachine.MachineID, NewFitness);
                                    //  CurrentSchedule.Print();
                                    return CurrentSchedule;
                                    // keep it
                                }
                                else
                                {
                                    // undo swap
                                    Console.WriteLine("No improvement, undoing..");
                                    SwapJobPair(J1, J2, CurrentMachine, CurrentSchedule);
                                    //undoing should always be feasible
                                }
                            }
                        }
                    }
                }

                NMachinesTried++;
                CurrentMachineId++;
                if (CurrentMachineId > CurrentSchedule.Problem.NMachines)
                { CurrentMachineId = 1; }

            }
            //No improving schedule found
            return null;
        }

        static private bool NeighborSwapFeasible(Job LeftJob, Job RightJob, Machine M, Schedule Sched)
        {
           // Console.WriteLine("Testing J{0},J{1} (M{2}) swap for creating cycles...",LeftJob.ID,RightJob.ID,M.MachineID);
            int OldJ1Index = Sched.MachineArcPointers[LeftJob.ID].ArrayIndex;
            if (M.AssignedJobs[OldJ1Index] != LeftJob) { throw new Exception("Indexing wrong. J1 index not pointing to J1"); }
            int OldJ2Index = Sched.MachineArcPointers[RightJob.ID].ArrayIndex;
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

            int OldJ1Index = Sched.MachineArcPointers[J1.ID].ArrayIndex;
            int OldJ2Index = Sched.MachineArcPointers[J2.ID].ArrayIndex;
            M.AssignedJobs[OldJ1Index] = J2;
            M.AssignedJobs[OldJ2Index] = J1;
            //update the pointers:
            Sched.MachineArcPointers[J1.ID].ArrayIndex = OldJ2Index;
            Sched.MachineArcPointers[J2.ID].ArrayIndex = OldJ1Index;
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
            int OldJ1Index = Sched.MachineArcPointers[J1.ID].ArrayIndex;
            if (M.AssignedJobs[OldJ1Index] != J1) { throw new Exception("Indexing wrong. J1 index not pointing to J1"); }
            int OldJ2Index = Sched.MachineArcPointers[J2.ID].ArrayIndex;
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
