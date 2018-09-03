using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
   class GeneralAdress
    {
        public Schedule Sched;
    }

    class SameMachineSwapAdress : GeneralAdress
    {
        public int J1index;
        public int J2index;
        public Machine M;

        
    }

    class OtherAdress : GeneralAdress
    {
        public Job J;
    }
  

  

    static class NeighbourhoodOperators
    {
        public static class RandomStartPoints
        {
            public static SameMachineSwapAdress SameMachineSwap(Schedule S)
            {
                return null;
                SameMachineSwapAdress Ad = new SameMachineSwapAdress();
                Ad.Sched = S;
                int MachinesTried = 0;
                int MachineID = Distribution.UniformInt(S.Problem.NMachines - 1) + 1;
                bool ViableMachine = false;
                while (MachinesTried < S.Problem.NMachines)
                {
                    Ad.M = S.Machines[MachineID - 1];
                    if (Ad.M.AssignedJobs.Count <= 1)
                    {
                        // cant do swaps, next machine
                        MachinesTried++;
                    }
                    else
                    {
                        ViableMachine = true;
                        break;
                    }
                }

                if (!ViableMachine)
                {
                    throw new Exception("Not enough jobs to do any swapping. (TODO) Not a hard error, but LS should fail");
                }
                else
                {
                    Ad.J1index = 0;
                    Ad.J2index = 1;
                }
                return Ad;
            }

        }

        public static class NextNeighbour
        {
            public static SameMachineSwapAdress SameMachineSwap(SameMachineSwapAdress Adr)
            {
                return null;
            }

            public static OtherAdress OtherSwap(OtherAdress Adr)
            {
                return null;
            }

        }
        
        

    }   
}
