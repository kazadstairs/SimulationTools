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
            Console.WriteLine("Hello Git.");
            int Nruns = 100;

            Simulation [] Sims = new Simulation[Nruns];
            

            Stopwatch watch = Stopwatch.StartNew();
            
            Parallel.For(0, Nruns, (i) =>
            {
                Sims[i] = new Simulation();
                Sims[i].Run();
            });
            Console.WriteLine(watch.ElapsedMilliseconds);

            Console.ReadLine();
        }
    }


    
}
