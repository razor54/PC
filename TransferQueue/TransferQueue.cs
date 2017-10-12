using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferQueue
{
    public class TransferQueue<T>
    {
        private LinkedList<T> queue = new LinkedList<T>();
        private object monitor = new object();
        public void Put(T msg)
        {
            //never locks

            //delivers message to queue

            lock(monitor)
            {
                queue.AddLast(msg);
            }
            

        }

        public bool Transfer(T msg, int timeout)
        {
            // throws ThreadInterruptedException

            //delivers message

            //waits for receit

            //return true if another thread receives it

            //return false if timeout

            //throws exception if Interrupted thread

            //if !sucess message should not stay on list

        }

        public bool Take(int timeout, out T rmsg)
        {
            // throws ThreadInterruptedException

            //receives message

            //return true if message received

            //return false if timeout

            //throws exception if Interrupted thread
            
        } 
    }

}
