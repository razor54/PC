/**
 * @author andre
 * on 08/11/2017.
 */
/***
 *
 *  ISEL, LEIC, Concurrent Programming, Verão 2016/17
 *
 *	Carlos Martins
 *
 *  Codigo anexo ao exercício 1 da SE#2
 *
 ***/

import java.util.Random;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.atomic.*;

/**
 *  Fake implementation of Michael-Scott Non-blocking Queue Algorithm (Michael and Scott, 1996)
 */

public class ConcurrentQueue<T> {

    private static class Node<T>{
        public AtomicReference<Node<T>> next = new AtomicReference<>();
        public final T value;
        public Node(T value){
            this.value = value;
        }
    }

    private AtomicReference<Node<T>> head;
    private AtomicReference<Node<T>> tail;

    public ConcurrentQueue(){
        Node<T> dummyNode = new Node<>(null);
        head = new AtomicReference<>(dummyNode);
        tail = new AtomicReference<>(dummyNode);
    }

    // enqueue a datum
    public void enqueue(T v) {
        Node<T> node = new Node<>(v);

        while(true) {

            Node<T> currTail = tail.get();
            Node<T> tailNext = currTail.next.get();

            if(currTail == tail.get()){
                if (tailNext == null) {
                    // (1)
                    if (currTail.next.compareAndSet(null, node)) {
                        // (2)
                        tail.compareAndSet(currTail, node);
                        return;
                    }
                }else{
                    // (2) try to finish another Thread's enqueue
                    tail.compareAndSet(currTail, tailNext);
                }
            }
        }
    }

    public T tryDequeue() {
        Node<T> currHead = head.get();
        Node<T> currTail = tail.get();
        Node<T> headNext = currHead.next.get();

        if(currHead == head.get()){ // no dequeue

            if(currHead == currTail){

                if(headNext == null){   // No value success
                    return null;
                }
                tail.compareAndSet(currTail, headNext); // Check if queued, finish another thread's enqueue
            } else {

                T pValue = headNext.value;
                if(head.compareAndSet(currHead, headNext)){
                    return pValue;
                }
            }
        }
        return null;
    }

    // dequeue a datum - spinning if necessary
    public T dequeue() throws InterruptedException {
        T v;
        while ((v = tryDequeue()) == null) {
            Thread.sleep(0);
        }
        return v;
    }

    public boolean isEmpty() {
        Node<T> currTail = tail.get();
        return head.compareAndSet(currTail, currTail);
    }

    /**
     * Test method.
     */

    public static boolean testMichaelScottQueue() throws InterruptedException {

        final int CONSUMER_THREADS = 2;
        final int PRODUCER_THREADS = 1;
        final int MAX_PRODUCE_INTERVAL = 100;
        final int MAX_CONSUME_TIME = 25;
        final int FAILURE_PERCENT = 5;
        final int JOIN_TIMEOUT = 100;
        final int RUN_TIME = 5 * 1000;
        final int POLL_INTERVAL = 20;


        Thread[] consumers = new Thread[CONSUMER_THREADS];
        Thread[] producers = new Thread[PRODUCER_THREADS];
        final ConcurrentQueue<String> msqueue = new ConcurrentQueue<String>();
        final int[] productions = new int[PRODUCER_THREADS];
        final int[] consumptions = new int[CONSUMER_THREADS];
        final int[] failuresInjected = new int[PRODUCER_THREADS];
        final int[] failuresDetected = new int[CONSUMER_THREADS];

        System.out.printf("%n%n--> Start test of Michael-Scott queue in producer/consumer context...%n%n");

        // create and start the consumer threads.
        for (int i = 0; i < CONSUMER_THREADS; i++) {
            final int tid = i;
            consumers[i] = new Thread(() -> {
                Random rnd = new Random(Thread.currentThread().getId());
                int count = 0;

                System.out.printf("-->c#%d starts...%n", tid);
                do {
                    try {
                        String data = msqueue.dequeue();
                        if (!data.equals("hello")) {
                            failuresDetected[tid]++;
                            System.out.printf("[f#%d]", tid);
                        }

                        if (++count % 10 == 0)
                            System.out.printf("[c#%d]", tid);

                        // Simulate the time needed to process the data.

                        if (MAX_CONSUME_TIME > 0)
                            Thread.sleep(rnd.nextInt(MAX_CONSUME_TIME));

                    } catch (InterruptedException ie) {
                        //do {} while (tid == 0);
                        break;
                    }
                } while (true);

                // display the consumer thread's results.
                System.out.printf("%n<--c#%d exits, consumed: %d, failures: %d",
                        tid, count, failuresDetected[tid]);
                consumptions[tid] = count;
            });
            consumers[i].setDaemon(true);
            consumers[i].start();
        }

        // create and start the producer threads.
        for (int i = 0; i < PRODUCER_THREADS; i++) {
            final int tid = i;
            producers[i] = new Thread( () -> {
                Random rnd = new Random(Thread.currentThread().getId());
                int count = 0;

                System.out.printf("-->p#%d starts...%n", tid);
                do {
                    String data;

                    if (rnd.nextInt(100) >= FAILURE_PERCENT) {
                        data = "hello";
                    } else {
                        data = "HELLO";
                        failuresInjected[tid]++;
                    }

                    // enqueue a data item
                    msqueue.enqueue(data);

                    // increment request count and periodically display the "alive" menssage.
                    if (++count % 10 == 0)
                        System.out.printf("[p#%d]", tid);

                    // production interval.

                    try {
                        Thread.sleep(rnd.nextInt(MAX_PRODUCE_INTERVAL));
                    } catch (InterruptedException ie) {
                        //do {} while (tid == 0);
                        break;
                    }
                } while (true);

                // display the producer thread's results
                System.out.printf("%n<--p#%d exits, produced: %d, failures: %d",
                        tid, count, failuresInjected[tid]);
                productions[tid] = count;
            });
            producers[i].setDaemon(true);
            producers[i].start();
        }

        // run the test RUN_TIME milliseconds.

        Thread.sleep(RUN_TIME);

        // interrupt all producer threads and wait for for until each one finished.
        int stillRunning = 0;
        for (int i = 0; i < PRODUCER_THREADS; i++) {
            producers[i].interrupt();
            producers[i].join(JOIN_TIMEOUT);
            if (producers[i].isAlive())
                stillRunning++;

        }

        // wait until the queue is empty
        while (!msqueue.isEmpty())
            Thread.sleep(POLL_INTERVAL);

        // interrupt each consumer thread and wait for a while until each one finished.
        for (int i = 0; i < CONSUMER_THREADS; i++) {
            consumers[i].interrupt();
            consumers[i].join(JOIN_TIMEOUT);
            if (consumers[i].isAlive())
                stillRunning++;
        }

        // if any thread failed to fisnish, something is wrong.
        if (stillRunning > 0) {
            System.out.printf("%n*** failure: %d thread(s) did answer to interrupt%n", stillRunning);
            return false;
        }

        // compute and display the results.

        long sumProductions = 0, sumFailuresInjected = 0;
        for (int i = 0; i < PRODUCER_THREADS; i++) {
            sumProductions += productions[i];
            sumFailuresInjected += failuresInjected[i];
        }
        long sumConsumptions = 0, sumFailuresDetected = 0;
        for (int i = 0; i < CONSUMER_THREADS; i++) {
            sumConsumptions += consumptions[i];
            sumFailuresDetected += failuresDetected[i];
        }
        System.out.printf("%n%n<-- successful: %d/%d, failed: %d/%d%n",
                sumProductions, sumConsumptions, sumFailuresInjected, sumFailuresDetected);

        return sumProductions == sumConsumptions && sumFailuresInjected == sumFailuresDetected;
    }

    public static void main(String[] args) throws Throwable {
        System.out.printf("%n--> Test Michael-Scott concurrent queue: %s%n",
                (testMichaelScottQueue() ? "passed" : "failed"));
    }
}



