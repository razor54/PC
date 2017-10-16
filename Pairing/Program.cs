using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pairing
{
    class Program
    {
        static void Main(string[] args)
        {

            LinkedList<int> test = new LinkedList<int>();
            test.AddLast(1);
            LinkedListNode<int>s = test.AddLast(2);
            test.AddLast(3);

            Console.WriteLine(s.List);
            test.Remove(s);
            Console.WriteLine(s.List);

            Console.ReadKey();

        }
    }
}
