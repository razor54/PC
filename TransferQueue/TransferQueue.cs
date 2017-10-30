using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SyncUtils;
using SyncUtils = SyncUtils.SyncUtils;

namespace TransferQueue
{
    public class TransferQueue<T>
    {
        private readonly LinkedList<T> _queue = new LinkedList<T>();

        private readonly LinkedList<object> _takeQueue = new LinkedList<object>();
        private readonly object _monitor = new object();

        public void Put(T msg)
        {
            //never locks

            //delivers message to queue

            lock (_monitor)
            {
                _queue.AddLast(msg);
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
            TimeoutInstant timeoutInstant = new TimeoutInstant(timeout);

            Monitor.Enter(_monitor);

            LinkedListNode<T> node = _queue.AddLast(msg);

           
            try
            {
                //if takeQueue is not empty notify the first elem
                if (_takeQueue.Count != 0 && node == _queue.First)
                {
                    object takeQueueCondition = _takeQueue.First.Value;
                    global::SyncUtils.SyncUtils.Pulse(_monitor, takeQueueCondition);
                }


                if (timeoutInstant.IsTimeout) return false;
                global::SyncUtils.SyncUtils.Wait(_monitor, node,timeoutInstant.Remaining);

                return true;
            }
            catch (ThreadInterruptedException)
            {
                _queue.Remove(node);
                Console.WriteLine("EXCEPTION TRANSFER");
                throw;
            }
            finally
            {
                Monitor.Exit(_monitor);
            }
        }

        public bool Take(int timeout, out T rmsg)
        {
            TimeoutInstant timeoutInstant = new TimeoutInstant(timeout);
            Monitor.Enter(_monitor);
            //when there is a message and is first in takeQueue -- condition
            object condition = new object();
            LinkedListNode<object> node = _takeQueue.AddLast(condition);
            try
            {
               
                //if first and message queue not empty -> can get message right away
                if (_takeQueue.First == node && _queue.Count != 0)
                {
                    ReceiveMessageSucessfuly(out rmsg);
                    return true;
                }

                if (timeoutInstant.IsTimeout)
                {
                    rmsg = default(T);
                    return false;
                }
               
                global::SyncUtils.SyncUtils.Wait(_monitor, condition, timeoutInstant.Remaining);


                ReceiveMessageSucessfuly(out rmsg);
                return true;
            }
            catch (ThreadInterruptedException e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                _takeQueue.Remove(node);
               
                Monitor.Exit(_monitor);
            }


            // throws ThreadInterruptedException

            //receives message

            //return true if message received

            //return false if timeout

            //throws exception if Interrupted thread
        }

        private void ReceiveMessageSucessfuly(out T rmsg)
        {
            var message = _queue.First;
            rmsg = message.Value;
            global::SyncUtils.SyncUtils.Pulse(_monitor, message);
            _queue.Remove(message);
        }
    }
}
