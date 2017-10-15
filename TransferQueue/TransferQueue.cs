using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

            Monitor.Enter(_monitor);

            LinkedListNode<T> node = _queue.AddLast(msg);

            Monitor.Enter(node);
            try
            {
                //if takeQueue is not empty notify the first elem
                if (_takeQueue.Count != 0 && node == _queue.First)
                {
                    object takeQueueCondition = _takeQueue.First.Value;
                    Monitor.Enter(takeQueueCondition);
                    Monitor.Pulse(takeQueueCondition);
                    Monitor.Exit(takeQueueCondition);
                }

                //else wait
                Monitor.Exit(_monitor);

                bool isNotTimeout = Monitor.Wait(node, timeout);
                //There might be a thread waiting to enter in the lock node while using lock monitor leading to dead lock...
                Monitor.Exit(node);
                //...so exit here and then reenter solves that problem
                Monitor.Enter(_monitor);

                Monitor.Enter(node);

                if (isNotTimeout)
                    return true;

                Console.WriteLine("WAS TIMEOUT on {0}", Thread.CurrentThread.ManagedThreadId);

                if (node.List != null)
                    _queue.Remove(node);
                return false;
            }
            catch (ThreadInterruptedException)
            {
                _queue.Remove(node);
                Console.WriteLine("EXCEPTION TRANSFER");
                throw;
            }
            finally
            {
                // _queue.Remove(node);
                Monitor.Exit(node);
                Monitor.Exit(_monitor);
            }
        }

        public bool Take(int timeout, out T rmsg)
        {
            Monitor.Enter(_monitor);
            //when there is a message and is first in takeQueue -- condition
            object condition = new object();
            LinkedListNode<object> node = _takeQueue.AddLast(condition);
            try
            {
                Monitor.Enter(condition);
                //if first and message queue not empty -> can get message right away
                if (_takeQueue.First == node && _queue.Count != 0)
                {
                    ReceiveMessageSucessfuly(out rmsg);
                    return true;
                }


                Monitor.Exit(_monitor);
                bool notTimeout = Monitor.Wait(condition, timeout);
                Monitor.Enter(_monitor);
                if (!notTimeout)
                {
                    Console.WriteLine("IS TIMEOUT ON TAKE ----> {0}", Thread.CurrentThread.ManagedThreadId);
                    rmsg = default(T);
                    return false;
                }

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
                Monitor.Exit(condition);
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

            Monitor.Enter(message);
            rmsg = message.Value;
            Monitor.Pulse(message);
            Monitor.Exit(message);

            _queue.Remove(message);
        }
    }
}
