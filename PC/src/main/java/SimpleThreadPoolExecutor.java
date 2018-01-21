import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.ConcurrentLinkedQueue;
import java.util.concurrent.RejectedExecutionException;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.locks.Condition;
import java.util.concurrent.locks.Lock;
import java.util.concurrent.locks.ReentrantLock;

/**
 * @author andre
 * on 22/10/2017.
 */
public class SimpleThreadPoolExecutor {

    private final int maxPoolSize;
    private final int keepAliveTime;

    private final ConcurrentLinkedQueue<Runnable> workQueue = new ConcurrentLinkedQueue<>();
    private final ConcurrentLinkedQueue<ThreadPoolObj> threads = new ConcurrentLinkedQueue<>();
    private boolean shuttingDown = false;

    private boolean threadWaitingForCompletion = false;

    //private final Object monitor = new Object();

    private final Lock lock = new ReentrantLock();
    private final Condition waiterCondition = lock.newCondition();

    private class ThreadPoolObj {
        final Runnable firstCommand;
        final int keepAliveTime;
        final Condition condition;
        boolean available = false;

        Thread thread;

        private ThreadPoolObj(Runnable firstCommand, Condition condition) {
            this.firstCommand = firstCommand;
            this.keepAliveTime = SimpleThreadPoolExecutor.this.keepAliveTime;
            this.condition = condition;
        }
    }

    public SimpleThreadPoolExecutor(int maxPoolSize, int keepAliveTime) {

        this.maxPoolSize = maxPoolSize;
        this.keepAliveTime = keepAliveTime;

    }

    private final ConcurrentLinkedQueue<Condition> waiting_executing_queue = new ConcurrentLinkedQueue<>();

    public boolean execute(Runnable command, int timeout) throws InterruptedException {
        //Might lock the invoking thread
        //Has to guaranty the delivery of the command
        //return true on success
        //throw RejectedExecutionException​, if shutting down pool
        //return false if timeout
        //throw InterruptedException​, if thread lock interrupted
        try {
            lock.lock();

            if (shuttingDown) throw new RejectedExecutionException();
            long time_target = Timeouts.start(timeout);


            List<ThreadPoolObj> availableThreads = Utilities.filter(t -> t.available, threads);

            //no thread available but at least a slot available
            if (threads.size() < maxPoolSize && availableThreads.size() == 0) {
                ThreadPoolObj threadPoolObj = new ThreadPoolObj(command, lock.newCondition());
                Thread t = new Thread(() ->
                        threadWork(threadPoolObj));
                threadPoolObj.thread = t;
                threads.add(threadPoolObj);
                t.start();
                return true;
            }

            Condition condition = lock.newCondition();
            waiting_executing_queue.add(condition);
            workQueue.add(command);

            int threadsAvailable = ((int) threads.stream().filter(t -> t.available).count());
            if (threadsAvailable < threads.size())
                this.waiterCondition.signalAll();

            return condition.await(Timeouts.remaining(time_target), TimeUnit.MILLISECONDS);


        } catch (InterruptedException e) {

            e.printStackTrace();
            throw e;

        } finally {

            lock.unlock();
        }


    }

    private void threadWork(ThreadPoolObj obj) {
        try {
            obj.firstCommand.run();

            long target = Timeouts.start(obj.keepAliveTime);

            obj.available = true;

            lock.lock();

            while (true) {

                //kill the thread and remove from list

                if (Timeouts.isTimeout(Timeouts.remaining(target))) {
                    threads.remove(obj);
                    return;
                }


                if (!workQueue.isEmpty()) {
                    Runnable poll = workQueue.poll();
                    obj.available = false;
                    //notify
                    Condition pollCondition = waiting_executing_queue.poll();
                    pollCondition.signal();

                    lock.unlock();

                    poll.run();

                    lock.lock();
                    obj.available = true;

                    //reset counter porque a thread esteve ativa
                    target = Timeouts.start(keepAliveTime);
                }

                if (shuttingDown) {
                    return;
                }


                try {
                    this.waiterCondition.await(Timeouts.remaining(target),TimeUnit.MILLISECONDS);
                } catch (InterruptedException ex){
                    if (!shuttingDown) //To not change the state of the list
                        threads.remove(obj);
                    return;
                }

            }

        } finally {
            lock.unlock();
        }

    }

    public void shutdown() {
        lock.lock();

        shuttingDown = true;

        lock.unlock();

        //puts executor in shutting down mode
    }

    public boolean awaitTermination(int timeout) throws InterruptedException {
        //return true if shutdown is concluded
        //return false if timeout
        //throws exception if wait is interrupted

        long target = Timeouts.start(timeout);
        lock.lock();
        if (threads.stream().allMatch(t -> t.available)) return true;
        threadWaitingForCompletion = true;
        lock.unlock();

        for (ThreadPoolObj t : threads) {

            //ficar a espera
            if (Timeouts.isTimeout(Timeouts.remaining(target))) return false;
            t.thread.join(Timeouts.remaining(target));

        }

        return !Timeouts.isTimeout(Timeouts.remaining(target));

    }
}
