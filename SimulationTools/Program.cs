using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Git.");
            PriorityQueue<int> IntHeap = new PriorityQueue<int>();
            //IntHeap.ExtractMin();
            IntHeap.Insert(4);
            IntHeap.ExtractMin();
            //IntHeap.ExtractMin();
            IntHeap.Insert(3);
            IntHeap.Insert(5);
            IntHeap.Insert(4);
            IntHeap.ExtractMin();
            IntHeap.ExtractMin();
            IntHeap.ExtractMin();

            Console.ReadLine();
        }
    }


    class PriorityQueue<T> where T : IComparable<T> // Requires the object T to be compared on some value (the key of the heap).
    {
        public int Count { private set; get; }
        private T[] Content;
        public int Capacity { private set; get; }

        public PriorityQueue()
        {
            Count = 0;
            Capacity = 2;
            Content = new T[Capacity];
            //Content.Add(default(T)); // To fill the indexed 0 position with junk. List is now a 1 based array.
        }

        public T ExtractMin()
        {
            if (Count > 0)
            {
                T Root = Content[1]; // 1 based: This is the root element of the tree
                Content[1] = Content[Count];
                DecreaseCount();
                MinHeapify(1);
                return Root;
            }
            else
            {
                throw new System.ArgumentException("ExtractMin Called on Empty Priority Queue. Tip: The Count paramater equals 0 when the Priority Queue is empty.");
            }
        }

        private void MinHeapify(int i)
        {
            int l, r, smallest;
            int todoRemoveSafety = 0;

            while (todoRemoveSafety < 100)
            {
                l = LeftChild(i);
                r = RightChild(i);
                smallest = -1;
                if (l <= Count && Content[l].CompareTo(Content[i]) < 0)
                {
                    // leftchild strictly less than current index.
                    smallest = l;
                }
                else
                {
                    // current index smallest so far
                    smallest = i;
                }
                if (r <= Count && Content[r].CompareTo(Content[smallest]) < 0)
                {
                    //rightchild smaller than smallest so far
                    smallest = r;
                }
                if (smallest == i)
                {
                    // Content at current index is smaller than its childern: finished.
                    return;
                }
                else
                {
                    Swap(smallest, i);
                    i = smallest;
                }

                todoRemoveSafety++;
            }

            Console.Error.WriteLine("Max iterations for MinHeapify exceded, remove the safety if you are sure!");
            
        }

        // Replace the contents at index i with contents at index j. No guarantees or error checks.
        private void Swap(int i, int j)
        {
            T temp = Content[i];
            Content[i] = Content[j];
            Content[j] = temp;
        }

        public void Insert(T elem)
        {
            IncreaseCount();
            Content[Count] = elem;
            
        }

        private void BubbleUp(int i) //
        {
            while(i > 1 && ( Content[Parent(i)].CompareTo(Content[i]) < 0) )
            {
                // i is not the root, and its value is strictly less than its parent.
                Swap(i, Parent(i));
                i = Parent(i);

            }
        }

        private void IncreaseCount()
        {
            if(Count < Capacity - 1) { Count++; return; }
            else // O(n), amortized O(1)
            {
                int newCapacity = 2 * Capacity;
                T[] newContent = new T[newCapacity];
                for(int i = 0; i< Capacity; i ++)
                {
                    newContent[i] = Content[i];
                }
                Content = newContent;
                Capacity = newCapacity;
                Count++;
                return;
            }
        }

        private void DecreaseCount()
        {
            if (Count > Capacity / 4 - 1) { Count--; return; }
            else // O(n), amortized O(1)
            {
                int newCapacity = Capacity / 4;
                T[] newContent = new T[newCapacity];
                for (int i = 0; i < Capacity; i++)
                {
                    newContent[i] = Content[i];
                }
                Content = newContent;
                Capacity = newCapacity;
                Count--;
                return;
            }
        }

        private int Parent(int i)
        {
            return i / 2;
        }

        private int LeftChild(int i)
        {
            return i * 2;
        }

        private int RightChild(int i)
        {
            return i * 2 + 1;
        }


    }
}
