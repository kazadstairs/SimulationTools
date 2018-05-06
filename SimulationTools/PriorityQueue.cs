using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
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
            //I made a change
        }

        public T PeekMin()
        {
            if(Count > 0)
            {
                return Content[1];
            }
            else
            {
                throw new System.ArgumentException("PeekMin Called on Empty Priority Queue. Tip: The Count paramater equals 0 when the Priority Queue is empty.");
            }            
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

        public void Print()
        {
            if(1 <= Count)
            {
                Console.WriteLine("-------------------------");
                PrintRec(1,0);
                Console.WriteLine("-------------------------");
            }
            else
            {
                Console.WriteLine("Empty PrioQueue");
            }
        }

        private void PrintRec(int i, int depth)
        {
            if (i <= Count)
            {
                Console.WriteLine(string.Concat(new string(' ',depth),Content[i]));
                PrintRec(LeftChild(i), depth + 1);
                PrintRec(RightChild(i), depth + 1);
            }
        }

   

        public void Insert(T elem)
        {
            IncreaseCount();
            Content[Count] = elem;
            BubbleUp(Count);
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



        private void BubbleUp(int i) //
        {
            while (i > 1 && (Content[i].CompareTo(Content[Parent(i)]) < 0))
            {
                // i is not the root, and its value is strictly less than its parent.
                Swap(i, Parent(i));
                i = Parent(i);

            }
        }

        private void IncreaseCount()
        {
            if (Count < Capacity - 1) { Count++; return; }
            else // O(n), amortized O(1)
            {
                int newCapacity = 2 * Capacity;
                T[] newContent = new T[newCapacity];
                for (int i = 0; i < Capacity; i++)
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
                int newCapacity = Capacity / 2;
                T[] newContent = new T[newCapacity];
                for (int i = 0; i < newCapacity; i++)
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
