using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to this Super Sweet Simulation Software");
            int Nruns = 1;

            Simulation [] Sims = new Simulation[Nruns];

            Schedule PinedoSched = new Schedule();
            PinedoSched.InstanciatePinedo();
            PinedoSched.SetReleaseDates();
            PinedoSched.SetDeadlines(32);
    

            /*
            List<Schedule> SchedulesToSimulate = new List<Schedule>();
            SchedulesToSimulate.Add(PinedoSched);

            Parallel.ForEach(SchedulesToSimulate, (currentSched) =>
            {
                new Simulation(Nruns, currentSched).Perform();
            });
            */
            

            Console.ReadLine();
        }
    }


    
}
