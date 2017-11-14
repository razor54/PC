using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConcurrentQueue
{
    class ConcurrentQueue<T> where T : class
    {
        private class Node<R>
        {
            public  Node<R> next = null;
            public readonly R value;

            public Node(R value)
            {
                this.value = value;
            }
        }

        private  Node<T> head;
        private  Node<T> tail;

        public ConcurrentQueue()
        {
            Node<T>dummyNode = new Node<T>(null);
            head = dummyNode;
            tail = dummyNode;

        }

        // enqueue a datum
        public void enqueue(T v)
        {
            Node<T> node = new Node<T>(v);

            while (true)
            {

                Node<T> currTail = tail;
                Node<T> tailNext = currTail.next;

                Interlocked.MemoryBarrier();

                if (currTail == tail)
                {
                    if (tailNext == null)
                    {//1
                        if (Interlocked.CompareExchange(ref currTail, node, null) == null)
                        {//2
                            Interlocked.CompareExchange(ref tail, node, currTail);
                            return;
                        }
                    }
                    else
                    {
                        //2
                        Interlocked.CompareExchange(ref tail, tailNext, currTail);
                    }

                }


            }
        }

        public T tryDequeue()
        {
            Node<T> currHead = head;
            Node<T> currTail = tail;
            Node<T> headNext = currHead.next;

            Interlocked.MemoryBarrier();

            if (currHead == head)
            { // no dequeue

                if (currHead == currTail)
                {

                    if (headNext == null)
                    {   // No value success
                        return null;
                    }
                    Interlocked.CompareExchange(ref tail, headNext, currTail);
                  
                }
                else
                {

                    T pValue = headNext.value;
                    if (Interlocked.CompareExchange(ref head,headNext,currHead)==currHead)
                    {
                        return pValue;
                    }
                }
            }
            return null;
        }

        // dequeue a datum - spinning if necessary
        public T dequeue()
        {

        }



        public bool isEmpty()
        {
        }
    }
}

