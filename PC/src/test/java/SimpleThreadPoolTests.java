import org.junit.Assert;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.ExpectedException;

import java.util.concurrent.RejectedExecutionException;

/**
 * @author andre
 * on 27/10/2017.
 */
public class SimpleThreadPoolTests {

    private static final int DEBUG_TIMEOUT = 999999999;
    private static final int RUN_TIMEOUT = 1000;

    @Test()
    public void SimpleThreadPoolTest1() {
        int timeout = RUN_TIMEOUT;

        SimpleThreadPoolExecutor threadPoolExecutor =
                new SimpleThreadPoolExecutor(2,timeout);
        Thread t1 = new Thread(() -> {
            try {
                threadPoolExecutor.execute(this::runable, timeout);
            } catch (InterruptedException e) {
                e.printStackTrace();
            }
        });

        Thread t2 = new Thread(() -> run(threadPoolExecutor));

        Thread t3 = new Thread(() -> run(threadPoolExecutor));

        Thread t4 = new Thread(threadPoolExecutor::shutdown);

        //supposed to cause err
        Thread t6 = new Thread(() -> run(threadPoolExecutor));



        Thread t5 = new Thread(() -> {
            try {
                boolean term = threadPoolExecutor.awaitTermination(timeout);
                System.out.println(term);
                Assert.assertTrue(term);
            } catch (InterruptedException e) {
                e.printStackTrace();
            }
        });


        t1.start();
        t2.start();
        t3.start();





        try {
            t1.join();
            t2.join();
            t3.join();

            t4.start();
            t5.start();

            t4.join();
            t5.join();
            //Thread.sleep(1000);

        } catch (InterruptedException e) {
            e.printStackTrace();
        }




    }

    private void run(SimpleThreadPoolExecutor threadPoolExecutor) {
        try {
            threadPoolExecutor.execute(()->{
                try {
                    Thread.sleep(100);
                } catch (InterruptedException e) {
                    e.printStackTrace();
                }
                System.out.println("The Command was successfully finished");
            },1);
        } catch (InterruptedException e) {
            e.printStackTrace();
        }
    }

    private void runable(){

            try {
                Thread.sleep(100);
            } catch (InterruptedException e) {
                e.printStackTrace();
            }
        System.out.println("EXECUTED");
    }
}
