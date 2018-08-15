using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateDotFile
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("digraph G {");

            int Machines = int.Parse(Console.ReadLine());
            int Njobs = int.Parse(Console.ReadLine());
            string[] J = new string[Njobs];
            string[] jobs = Console.ReadLine().Split();
            for (int jobID = 0; jobID < Njobs; jobID++)
            {
                J[jobID] = string.Format("Id:{0}-info:{1}", jobID, jobs[jobID]); // replace with better name if wanted
            }


            string[] splitline;
            for (int rowid = 0; rowid < Njobs; rowid++)
            {
                splitline = Console.ReadLine().Split();
                for (int colid = 0; colid < Njobs; colid++)
                {
                    if (splitline[colid] == "") break; // empty thing
                    else if (splitline[colid] != "-")
                    {
                        Console.WriteLine("{0} -> {1}", J[rowid], J[colid]);
                    }
                }
            }

            Console.WriteLine("}");

        }//end of Main
    }
}
