using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateDotFile
{
    //cmd commands: CreateDotFile.exe < %1 >temp.dotin &&  dot -Tpng <temp.dotin >%2 && %2. 
    //Make sure dot is in the PATH enviroment( use SET PATH=%PATH%;<path to bin folder containing dot>)
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("digraph G {");

            int Machines = int.Parse(Console.ReadLine());
            int Njobs = int.Parse(Console.ReadLine());
            string[] J = new string[Njobs+1];
            string[] jobs = Console.ReadLine().Split();
            for (int jobID = 1; jobID < Njobs+1; jobID++)
            {
                J[jobID] = string.Format("\"{0}|{1}\"", jobID,jobs[jobID]); // replace with better name if wanted , jobs[jobID]
            }


            string[] splitline;
            for (int rowid = 1; rowid < Njobs+1; rowid++)
            {
                splitline = Console.ReadLine().Split();
                for (int colid = 1; colid < Njobs+1; colid++)
                {
                    if (splitline[colid] != "-")
                    {
                        Console.WriteLine("{0} -> {1}", J[rowid], J[colid]);
                    }
                }
            }

            Console.WriteLine("}");

        }//end of Main
    }
}
