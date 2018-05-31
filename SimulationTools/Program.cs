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

            //
            // SETUP
            //
            int Nruns = 10;
            //
            // End of Setup
            //

            Simulation [] Sims = new Simulation[Nruns];
            ProblemInstance Pinedo = new ProblemInstance();
            Pinedo.InstanciatePinedo();

            Schedule Sched = new Schedule(Pinedo);
            Sched.Build();
            Sched.Print();
            Sched.SetReleaseDates();
            Sched.SetESS();
            Sched.MakeHTMLImage("Nonoptimal ESS schedule for Pinedo Instance");

            List<Schedule> SchedulesToSimulate = new List<Schedule>();
            SchedulesToSimulate.Add(Sched);
            
            Sched.EstimateCmax();
            Sched.SetDeadlines(Sched.EstimatedCmax);
            Sched.SetLSS();
            Sched.MakeHTMLImage("Nonoptimal LSS schedule for Pinedo Instance");
            //SchedulesToSimulate.Add(Sched);
            

            Parallel.ForEach(SchedulesToSimulate, (currentSched) =>
            {
                new Simulation(Nruns, currentSched).Perform();
            });


            


            //PinedoSched.SetReleaseDates();
            //PinedoSched.SetDeadlines(32);





            Console.ReadLine();
        }
    }


    
}
