using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    static class LocalSearch
    {
        /// <summary>
        /// SwapHill climb uses LS to increase the fitnessfunction.
        /// </summary>
        /// <param name="Original"></param>
        /// <param name="FitnessFunction">f(S)->R, if A is a better schedule than B, then it should hold that f(A) > f(B) (to invert, add a minus sign to the fitness function!)</param>
        /// <returns></returns>
        static public Schedule SwapHillClimb(ref Schedule Original, Func<Schedule, double> FitnessFunction)
        {
            Schedule CurrentSchedule = Original;
            bool LocalOptimum = false;

            Console.WriteLine("DEBUG: Starting LS *********************");
            Console.WriteLine("Original Schedule with fitness {0}", FitnessFunction(CurrentSchedule));
            //CurrentSchedule.Print();

            while (!LocalOptimum)
            {
                int StartMachineId = Distribution.UniformInt(CurrentSchedule.Problem.NMachines-1)+1; // 1 to 11 (incl)
                int CurrentMachineId = StartMachineId;
                int NMachinesTried = 0;
                bool improvementFound = false;
                Machine CurrentMachine = CurrentSchedule.Machines[CurrentMachineId-1];
                //Console.WriteLine("TODO LAZY: MAKE above assignment a CALL A FUNCTION IN SCHEDULE!");
                while (!improvementFound && NMachinesTried < CurrentSchedule.Problem.NMachines)
                {
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
                            if (improvementFound) break;

                            J1 = CurrentMachine.AssignedJobs[J1index];

                            for (int J2index = J1index + 1; J2index < JobsOnMachine; J2index++)
                            {
                                J2 = CurrentMachine.AssignedJobs[J2index];
                                // try the swap
                                double OriginalFitness = FitnessFunction(CurrentSchedule);
                                if (SameMachineSwap(J1, J2, CurrentMachine, CurrentSchedule))
                                {
                                    
                                    double NewFitness = FitnessFunction(CurrentSchedule); // current schedule is updated during the SameMachineSwap function. So this works.
                                    if (NewFitness > OriginalFitness) // Trying to minimize here, so maybe VD more appropriate than HC! This maximizes CMAX!
                                    {
                                        improvementFound = true;
                                      //  Console.WriteLine("OPTIMIZING MOVE FOUND (Swap J{0},J{1}, on M{2}). Schedule fitness: {3}",J1.ID,J2.ID,CurrentMachine.MachineID, NewFitness);
                                      //  CurrentSchedule.Print();

                                        break;
                                        // keep it
                                    }
                                    else
                                    {
                                        // undo swap
                                       // Console.WriteLine("No improvement, undoing..");
                                        SameMachineSwap(J1, J2, CurrentMachine, CurrentSchedule);
                                        //undoing should always be feasible
                                    }
                                }
                            }
                        }
                    }

                    // update the number of tries and move to next machine, which may require resetting index to 0 if it goes out of bounds
                    NMachinesTried++;
                    if (NMachinesTried == CurrentSchedule.Problem.NMachines)
                    {
                        // Tried swaps on all machines without success;
                        LocalOptimum = true;
                    }
                    CurrentMachineId++;
                    CurrentMachine = CurrentSchedule.Machines[CurrentMachineId - 1];
                    if (CurrentMachineId == CurrentSchedule.Problem.NMachines) { CurrentMachineId = 0; }


                }// end of While over machines (single improvement step)
            }// End of HC loop

            Console.WriteLine("Local optimum found with fitness: {0}", FitnessFunction(CurrentSchedule));
            return CurrentSchedule;
        }


        static public Schedule ReassignHillClimb(ref Schedule Original, Func<Schedule, double> FitnessFunction)
        {
            throw new System.NotImplementedException("WORK In PROGRESS");
            Schedule CurrentSchedule = Original;
            bool LocalOptimum = false;

            Console.WriteLine("DEBUG: Starting LS *********************");
            Console.WriteLine("Original Schedule with fitness {0}", FitnessFunction(CurrentSchedule));
            CurrentSchedule.Print();

            while (!LocalOptimum)
            {
                Job MoveJob = CurrentSchedule.PrecedenceDAG.GetJobById(Distribution.UniformInt(CurrentSchedule.PrecedenceDAG.N));
                int StartMachineId = Distribution.UniformInt(CurrentSchedule.Problem.NMachines);
                int CurrentMachineId = StartMachineId;
                int NMachinesTried = 0;
                bool improvementFound = false;
                while (!improvementFound && NMachinesTried < CurrentSchedule.Problem.NMachines)
                {
                    int JobsOnMachine = CurrentSchedule.Machines[CurrentMachineId].AssignedJobs.Count;
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
                            if (improvementFound) break;

                            J1 = CurrentSchedule.Machines[CurrentMachineId].AssignedJobs[J1index];

                            for (int J2index = J1index + 1; J2index < JobsOnMachine; J2index++)
                            {
                                J2 = CurrentSchedule.Machines[CurrentMachineId].AssignedJobs[J2index];
                                // try the swap
                                double OriginalFitness = FitnessFunction(CurrentSchedule);
                                if (SameMachineSwap(J1, J2, CurrentSchedule.Machines[CurrentMachineId], CurrentSchedule))
                                {
                                    double NewFitness = FitnessFunction(CurrentSchedule);
                                    if (NewFitness > OriginalFitness)
                                    {
                                        improvementFound = true;
                                        Console.WriteLine("OPTIMIZING MOVE FOUND, resulting schedule with fitness {0} is:", NewFitness);
                                        CurrentSchedule.Print();

                                        break;
                                        // keep it
                                    }
                                    else
                                    {
                                        // undo swap
                                        Console.WriteLine("No improvement, undoing..");
                                        SameMachineSwap(J1, J2, CurrentSchedule.Machines[CurrentMachineId], CurrentSchedule);
                                        //undoing should always be feasible
                                    }
                                }
                            }
                        }
                    }

                    NMachinesTried++;
                    CurrentMachineId++;
                    if (CurrentMachineId == CurrentSchedule.Problem.NMachines) { CurrentMachineId = 0; }


                }// end of While over machines
                // all possible swaps tried on all possible machines, no improvement found.
                LocalOptimum = true;


                // update the number of tries and move to next machine, which may require resetting index to 0 if it goes out of bounds

            }

            return CurrentSchedule;
        }

        static public Schedule HillClimb(Schedule Original, Func<Schedule,GeneralAdress> GetStart, Func<GeneralAdress,GeneralAdress> NextNeighbour, Func<Schedule,double> FitnessFunction)
        {
            Schedule CurrentSchedule = Original;
            bool LocalOptimum = false;
            double CurrentFitness = -double.MaxValue;
            GeneralAdress StartPoint = null;
            GeneralAdress Candidate = null;
            bool ImprovementFound;

            //Pick random job and or random machine:
            while (!LocalOptimum)
            {
                StartPoint = GetStart(CurrentSchedule);
                Candidate = NextNeighbour(StartPoint);
                CurrentFitness = FitnessFunction(CurrentSchedule);
                ImprovementFound = false;
                while (!ImprovementFound && Candidate != StartPoint)
                {
                    Candidate = NextNeighbour(Candidate); // cycle through neighbourhood.
                    if (FitnessFunction(Candidate.Sched) > CurrentFitness)
                    {
                        ImprovementFound = true;
                        CurrentSchedule = Candidate.Sched;
                        // Improving swap found. Explore neighborhood of that schedule.
                    }

                }

                if (!ImprovementFound)
                {
                    //All neighbours explored without improvement: Local Optimum
                    LocalOptimum = true;
                }


            }
            return CurrentSchedule;
        }

        static public Schedule HCSMS(Schedule Original)
        {
            return HillClimb(Original, NeighbourhoodOperators.RandomStartPoints.SameMachineSwap,NeighbourhoodOperators.NextNeighbour.SameMachineSwap, FitnessFunctions.MeanBasedCmax);
        }
 
        

      /*  static private void SameMachineNeighbourhood(Sched CurrentSched, int JobIndex, int MachineIndex)
        {
            Job J1 = null, J2 = null;
            for (int J1index = 0; J1index < JobsOnMachine - 1; J1index++)
            {
                if (improvementFound) break;

                J1 = CurrentSchedule.Machines[CurrentMachineId].AssignedJobs[J1index];

                for (int J2index = J1index + 1; J2index < JobsOnMachine; J2index++)
                {
                    J2 = CurrentSchedule.Machines[CurrentMachineId].AssignedJobs[J2index];
                    // try the swap
                    double OriginalFitness = FitnessFunction(CurrentSchedule);
                    if (SameMachineSwap(J1, J2, CurrentSchedule.Machines[CurrentMachineId], CurrentSchedule))
                    {
                        double NewFitness = FitnessFunction(CurrentSchedule);
                        if (NewFitness > OriginalFitness)
                        {
                            improvementFound = true;
                            Console.WriteLine("OPTIMIZING MOVE FOUND, resulting schedule with fitness {0} is:", NewFitness);
                            CurrentSchedule.Print();

                            break;
                            // keep it
                        }
                        else
                        {
                            // undo swap
                            Console.WriteLine("No improvement, undoing..");
                            SameMachineSwap(J1, J2, CurrentSchedule.Machines[CurrentMachineId], CurrentSchedule);
                            //undoing should always be feasible
                        }
                    }
                }
            }

        }*/

        /// <summary>
        /// If feasible, will perform a swap and return true. If not feasible will not perform sawp. Does not consider fitness.
        /// </summary>
        /// <param name="J1">Left Job to be swapped</param>
        /// <param name="J2">Right Job to be swapped</param>
        /// <param name="M">Machine to swap on</param>
        /// <param name="Sched">Schedule that is being updated</param>
        /// <returns></returns>
        static private bool SameMachineSwap(Job J1, Job J2, Machine M, Schedule Sched)
        {
          
            //todo: Check feasibility of the swaps. Give each job a dictionary of all its transitive descendants. Check in almost O(1) if swap is feasible. (MUCH BETTER THAN BFS).
            int OldJ1Index = Sched.MachineArcPointers[J1.ID].ArrayIndex;
            int OldJ2Index = Sched.MachineArcPointers[J2.ID].ArrayIndex;
            int LeftIndex = OldJ1Index;
            int RightIndex = OldJ2Index;
            if (OldJ2Index < OldJ1Index) { LeftIndex = OldJ2Index; RightIndex = OldJ1Index; }
            // check feasibility of the swap, Assuming an already feasible setup.
            for (int i = LeftIndex; i <= RightIndex - 1; i++)
            {
                
                for (int j = i+1; j <= RightIndex; j++)
                {
                    // Between [J1 and J2] (inlusive) the arcs get reversed. Check all combos.
                    if (Sched.PrecedenceDAG.PrecPathExists(M.AssignedJobs[i],M.AssignedJobs[j])) { return false; } // infeasible.
                }
            }
            //Console.WriteLine("Swap J{0}, J{1} on M{2}", J1.ID, J2.ID, M.MachineID);

            //update the position of the jobs in the list.
            M.AssignedJobs[OldJ1Index] = J2;
            M.AssignedJobs[OldJ2Index] = J1;
            //update the pointers:
            Sched.MachineArcPointers[J1.ID].ArrayIndex = OldJ2Index;
            Sched.MachineArcPointers[J2.ID].ArrayIndex = OldJ1Index;
            
            return true;
        }

        static private bool ToMachineAt(Job J, Machine NewM, int JindexOnM, Schedule Sched)
        {
            // check feasibility:
            for (int i = JindexOnM; i < NewM.AssignedJobs.Count; i++)
            {
                if (Sched.PrecedenceDAG.PrecPathExists(Sched.PrecedenceDAG.GetJobById(i), J)) { return false; }
            }

            Sched.DeleteJobFromMachine(J);
            Sched.InsertJobOnMachineAtIndex(J, NewM, JindexOnM);

            return true;

        }

    }


}


