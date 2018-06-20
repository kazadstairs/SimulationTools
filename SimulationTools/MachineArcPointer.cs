using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{

    class MachineArcPointer
    {
        public int MachineId;
        public int ArrayIndex;

        public MachineArcPointer(int _MachineId, int _ArrayIndex)
        {
            MachineId = _MachineId;
            ArrayIndex = _ArrayIndex;
        }
    }
}
