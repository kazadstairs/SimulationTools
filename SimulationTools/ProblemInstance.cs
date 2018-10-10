using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    class ProblemInstance
    {
        public DirectedAcyclicGraph DAG;
        public int NMachines;
        //public List<Machine> Machines;
        public string Description;

        public ProblemInstance()
        {
            DAG = new DirectedAcyclicGraph();
        }
        public void InstanciatePinedo()
        {
            DAG.AddJob(new Job(0, 0, 0));

            DAG.AddJob(new Job(1, 4, 0));
            DAG.AddJob(new Job(2, 9, 0));
            DAG.AddJob(new Job(3, 3, 0));
            DAG.AddJob(new Job(4, 3, 0));
            DAG.AddJob(new Job(5, 6, 0));
            DAG.AddJob(new Job(6, 8, 0));
            DAG.AddJob(new Job(7, 12, 0));
            DAG.AddJob(new Job(8, 8, 0));
            DAG.AddJob(new Job(9, 6, 0));

            


            // precedence arcs
            DAG.AddArcById(1, 2);
            DAG.AddArcById(2, 6);
            DAG.AddArcById(3, 4);
            DAG.AddArcById(4, 5);
            DAG.AddArcById(5, 6);
            DAG.AddArcById(5, 7);
            DAG.AddArcById(6, 8);
            DAG.AddArcById(7, 8);
            DAG.AddArcById(7, 9);

            DAG.AddArcById(0, 1);
            DAG.AddArcById(0, 3);

            // machine arcs all match prec arcs in this instance
            NMachines = 2;

            DAG.FillSuccessorDictionaries();

            Description = "Pinedo";            
        }

        public void InstanciateFullP10Blok()
        {
            InstanciateBlokHelper(4, FullDependency);
            Description += "_FullDependency";
        }

        public void InstanciateFullP1Blok()
        {
            InstanciateBlokHelper(40, FullDependency);
            Description += "_FullDependency";
        }

        public void Instanciate4CycleP10Blok()
        {
            InstanciateBlokHelper(4, FourCycles);
            Description += "_FourCycles";
        }

        public void Instanciate4CycleP1Blok()
        {
            InstanciateBlokHelper(40, FourCycles);
            Description += "_FourCycles";
        }

        public void Instanciate1CycleP10Blok()
        {
            InstanciateBlokHelper(4, SingleCycle);
            Description += "_SingleCycle";
        }

        public void Instanciate1CycleP1Blok()
        {
            InstanciateBlokHelper(40, SingleCycle);
            Description += "_SingleCycle";
        }

        public void InstanciateNoInterMachineP10Blok()
        {
            InstanciateBlokHelper(4, NoInterMachine);
            Description += "_NoInterMachine";
        }

        public void InstanciateNoInterMachineP1Blok()
        {
            InstanciateBlokHelper(40, NoInterMachine);
            Description += "_NoInterMachine";
        }

        public void InstanciateDiamondP10Blok()
        {
            InstanciateBlokHelper(4, Diamond);
            Description += "_Diamond";
        }

        public void InstanciateDiamondP1Blok()
        {
            InstanciateBlokHelper(40, Diamond);
            Description += "_Diamond";
        }
        public void InstanciateRollingDiamondP10Blok()
        {
            InstanciateBlokHelper(4, RollingDiamond);
            Description += "_RollingDiamond";
        }

        public void InstanciateRollingDiamondP1Blok()
        {
            InstanciateBlokHelper(40, RollingDiamond);
            Description += "_RollingDiamond";
        }

        private void InstanciateBlokHelper(int JobsPerMachine, Action<int,int,int> MakeInterMachinePrecs)
        {
            int id;
            id = 1;
            DAG.AddJob(new Job(0, 0, 0)); // dummyjob
            Description = string.Format("Blok_{0}_JobsPerMachine",JobsPerMachine);
            double Processingtime = 40 / JobsPerMachine;
            for (int col = 0; col < JobsPerMachine; col++)
            {
                for (int row = 0; row < 4; row++)
                {
                    DAG.AddJob(new Job(id, Processingtime, 0));
                    if (col >= 1)
                    {
                        MakeInterMachinePrecs(id, col, row);
                    }
                    id++;
                }
            }
            DAG.FillSuccessorDictionaries();
            Console.WriteLine(DAG.N);

            NMachines = 4;
        }


        private void FullDependency(int id, int col, int row)
        {
            int parentid;
            for (int parentrow = 0; parentrow < 4; parentrow++)
            {
                parentid = ToID(parentrow, col - 1);
                DAG.AddArcById(parentid, id);
            }
        }

        
        private void FourCycles(int id, int col, int row)
        {
            //cycle:
            int parentid, parentrow;
            if (row == 0) { parentrow = 3; }
            else { parentrow = row - 1; }
            parentid = ToID(parentrow, col - 1);
            DAG.AddArcById(parentid, id);
            //"machine arc": same row, 1 col back
            DAG.AddArcById(ToID(row, col - 1), id);
        }

        private void SingleCycle(int id, int col, int row)
        {
            if (col % 4 == row && col >= 1)
            {
                int parentid;
                if (row == 0)
                {
                    parentid = ToID(3, col - 1);
                }
                else
                {
                    parentid = ToID(row - 1, col - 1);
                }
                DAG.AddArcById(parentid, id);
            }
            //"machine arc": same row, 1 col back
            DAG.AddArcById(ToID(row, col - 1), id);
        }

        private void NoInterMachine(int id, int col, int row)
        {
            DAG.AddArcById(ToID(row, col - 1), id);
        }

        private void Diamond(int id, int col, int row)
        {
            if (col % 2 == 1)
            {
                if (row == 0)
                {
                    for (int parentrow = 1; parentrow < 4; parentrow++)
                    {
                        DAG.AddArcById(ToID(parentrow, col - 1), id);
                    }

                }
            }
            else
            {
                if (row > 0)
                {
                    DAG.AddArcById(ToID(0, col - 1), id);
                }
            }
            DAG.AddArcById(ToID(row, col - 1), id);
        }

        private void RollingDiamond(int id, int col, int row)
        {
            int MergeRow = ((col+1) / 2) % 4;
            if (col % 2 == 1)
            {
                if (row == MergeRow)
                {
                    for (int parentrow = 0; parentrow < 4; parentrow++)
                    {
                        if (parentrow != row)
                        { DAG.AddArcById(ToID(parentrow, col - 1), id); }
                    }

                }
            }
            else
            {
                if (row != MergeRow)
                {
                    DAG.AddArcById(ToID(MergeRow, col - 1), id);
                }
            }
            DAG.AddArcById(ToID(row, col - 1), id);
        }

        private int ToID(int RowNr, int ColNr)
        {
            return RowNr + 1 + 4 * ColNr;
        }
        




        public void InstanciateLSTest()
        {
            DAG.AddJob(new Job(0, 0, 0));
            DAG.AddJob(new Job(1, 1, 0));
            DAG.AddJob(new Job(2, 1, 0));
            DAG.AddJob(new Job(3, 1, 0));

            DAG.AddArcById(0, 1);
            DAG.AddArcById(0, 2);
            DAG.AddArcById(0, 3);
            DAG.AddArcById(2, 3);

            NMachines = 2;

            Description = "LSTest";

            DAG.FillSuccessorDictionaries();
        }

        public void ReadFromFile(string FileName, string descr)
        {
            Description = descr;
            DAG.AddJob(new Job(0, 0, 0)); // dummy job
            

            string[] lines = System.IO.File.ReadAllLines(FileName);
            this.NMachines = int.Parse(lines[0]);
            int Njobs = int.Parse(lines[1]);
            // create jobs
            string[] jobs = lines[2].Split();
            string[] jobtemp;
            for (int jobID = 1; jobID < Njobs + 1; jobID++)
            { 
                jobtemp = jobs[jobID].Split(',');                
                DAG.AddJob(new Job(jobID, int.Parse(jobtemp[0]), int.Parse(jobtemp[1]))); // job id, pj, rj
                Console.WriteLine("Added J{0} with P{0}={1}, R{0}={2}",jobID,int.Parse(jobtemp[0]),int.Parse(jobtemp[1]));
            }

            // create prec relations
            string [] splitline;
            for (int rowid = 1; rowid < Njobs+1; rowid++)
            {
                splitline = lines[2 + rowid].Split();
                for (int colid = 1; colid < Njobs+1; colid++)
                {
                    if (splitline[colid] != "-")
                    {
                        DAG.AddArcById(rowid, colid);
                    }
                }
            }

            DAG.FillSuccessorDictionaries();
            
        }

        public void CreateDotFile()
        {
            string OutputPath = string.Format("{0}\\probleminstances\\{1}.dotin", Program.BASEPATH, Description);
            using (StreamWriter sw = File.CreateText(OutputPath))
            {
                sw.WriteLine("digraph G {");
                foreach (Job u in DAG.Jobs)
                {
                    foreach (Job v in u.Successors)
                    {
                        sw.WriteLine("{0} -> {1}", u.ID, v.ID);
                    }

                }
                sw.WriteLine("}");
            }
        }


    }
    
}
