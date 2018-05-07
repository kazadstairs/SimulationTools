using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    class Simulation
    {
        public PriorityQueue<Event> EventList;
        public State CurrentState;


        public void Run()
        {
            Console.WriteLine("Placeholder!");
            SetupSimulation();
            PerformSimulation();
            

        }

        private void SetupSimulation()
        {
            EventList = new PriorityQueue<Event>();

            for (int id = 0; id < Constants.NumberOfEvents; id++)
            {
                EventList.Insert(new EGeneric(id));

            }
        }

        private void PerformSimulation()
        {
            while(EventList.Count > 0)
            {
                EventList.ExtractMin().Handle();
            }
            Console.WriteLine("Simluation Complete");
        }
    }
}
