using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    class DirectedAcyclicGraph
    {
        public int N { get; private set; }
        public int M { get; private set; }

        public List<Job> Jobs { get; private set; }
        private List<Arc> PrecedenceArcs;

        public HashSet<int>[] TransitiveSuccessorsOf;
        // MACHINE ARCS ARE PART OF THE SCHEDULE private List<MachineArc> MachineArcs;

        public DirectedAcyclicGraph()
        {
            N = 0;
            M = 0;
            Jobs = new List<Job>();
            PrecedenceArcs = new List<Arc>();
            
        }

        public DirectedAcyclicGraph(int Nvertices, int Medges)
        {
            N = Nvertices;
            M = Medges;
            Jobs = new List<Job>(N);
            PrecedenceArcs = new List<Arc>(M);
        }

        public void AddJob(Job j)
        {
           // Console.WriteLine("Adding J{0}",j.ID);
            Jobs.Add(j);
            N++;
        }

        public void AddArc(Job u, Job v)
        {
            if (SlowPrecPathExists(v, u))
            {
                throw new Exception(string.Format("Adding arc from vertex {0} to vertex {1} would create a cycle in DAG", u.ID, v.ID));
            }
            M++;
            if (u.Successors.Count > 0 && u.Successors.Contains(v))
            { //Arc already present, no need to add it again 
                return;
            }
            PrecedenceArcs.Add(new Arc(u, v));
            u.Successors.Add(v);
            v.Predecessors.Add(u);
        }

        public Job GetJobById(int id)
        {
            if (Jobs[id].ID != id) { throw new Exception("Id does not match index."); }
            else
            {
                return Jobs[id];
            }
        }

        public void AddArcById(int uid, int vid)
        {
           // Console.WriteLine("Adding arc ({0}->{1})",uid,vid);
            AddArc(GetJobById(uid), GetJobById(vid));
        }


        //In O(V + E) return all jobs in precedence order, IGNORING MACHINE Assignments!

            /// <summary>
            /// Perform BFS on the Precedence graph from OriginJob. Logic should return true when search is successfull and false if the search is exhaustive and unsuccessfull.
            /// </summary>
            /// <param name="OriginJob"></param>
            /// <param name="ApplyLogic"></param>
            /// <returns></returns>
        private bool PrecBFS(Job OriginJob, Func<Job, bool> ApplyLogic)
        {
            bool[] BFSVisited = new bool[N];
            Queue<Job> BFSQueue = new Queue<Job>();
            BFSQueue.Enqueue(OriginJob);
            Job CurrentJob;
            while (BFSQueue.Count > 0)
            {
                CurrentJob = BFSQueue.Dequeue();
                foreach (Job Succ in CurrentJob.Successors)
                {
                    if (ApplyLogic(Succ) == true) { return true; }
                    else if (!BFSVisited[Succ.ID])
                    {
                        BFSVisited[Succ.ID] = true;
                        BFSQueue.Enqueue(Succ);
                    }

                }
            }
            return false;
        }

        public bool SlowPrecPathExists(Job u, Job v)
        {
            return PrecBFS(u, (Job Curr) => Curr == v);

        }

        public bool PrecPathExists(Job u, Job v)
        {
            return TransitiveSuccessorsOf[u.ID].Contains(v.ID);

        }

        public void FillSuccessorDictionaries()
        {
            TransitiveSuccessorsOf = new HashSet<int>[N];
            for (int i = 0; i < N; i++)
            {
                TransitiveSuccessorsOf[i] = new HashSet<int>();
                for (int j = 0; j < N; j++)
                {
                    if (SlowPrecPathExists(GetJobById(i), GetJobById(j))) // slow but only once per problem instance.
                    {
                        TransitiveSuccessorsOf[i].Add(j); // hashset automatically adds each element only once.
                    }
                }
            }
        }



    }

    class Arc
    {
        Job U; // from this vertex
        Job V; // to this vertex

        public Arc()
        {
            U = null;
            V = null;
        }
        public Arc(Job u, Job v)
        {
            U = u;
            V = v;
        }
    }

 /*   class MachineArc : Arc
    {
        Job U;
        Job V;
        Machine M;

        public Job GetOrigin()
        {
            return U;
        }

        public Job GetTarget()
        {
            return V;
        }

        public MachineArc(Job u, Job v, Machine m)
        {
            U = u;
            V = v;
            M = m;
        }
    }

    */

}
