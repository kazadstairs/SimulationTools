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
            
            
            
            Parallel.For(0, Nruns, (i) =>
            {
                Sims[i] = new Simulation();
                Sims[i].Run();
            });

            Console.ReadLine();
        }
    }


    
}
