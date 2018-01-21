/**
 *
 * ISEL, LEIC, Concurrent Programming
 *
 * A TCP multithreading server based on .NET APM.
 * Limits maximum number of simultaneous connectiosn and shutdowns graciously
 * 
 * Carlos Martins, June 2017
 *
 **/

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;

/**
 * A TCP multi-threaded echo server.
 */

public class TcpMultithreadedApmEchoServer
{

    // the listening port
    private const int SERVER_PORT = 13000;

    // minimum and maximum service time
    private const int MIN_SERVICE_TIME = 10;
    private const int MAX_SERVICE_TIME = 250;

    // the server's listetner socket
    private TcpListener server;

    // the maximum allowed number of nested IO calbacks
    private const int MAX_NESTED_IO_CALLBACKS = 2;

    // a thread local variavel to count the number of nested IO callbacks on each thread
    private ThreadLocal<int> nestedIOCallbacks = new ThreadLocal<int>();

    // a thread-local random number generator
    private ThreadLocal<Random> random = new ThreadLocal<Random>(() => new Random(Environment.TickCount));

    /**
	 * Number of active connections and the maximum allowed.
	 */
    private volatile int activeConnections;
    private const int MAX_ACTIVE_CONNECTIONS = 10;

    // Set to true when the shut down of the server is in progress.
    private volatile bool shutdownInProgress;

    // Event used to block the primary thread during shut down.
    private GenericAsyncResult<int> listenAsyncResult;

    /**
	 * Constants used when polling for server idle.
	 */
    private const int WAIT_FOR_IDLE_TIMEOUT = 10000;
    private const int POLLING_INTERVAL = WAIT_FOR_IDLE_TIMEOUT / 100;

    /**
	 * Buils an echo APM multithread server
	 */
    public TcpMultithreadedApmEchoServer()
    {
        // Create a listen socket bound to the server port.
        server = new TcpListener(IPAddress.Loopback, SERVER_PORT);
        // Start listen for connection requests
        server.Start();
    }

    /**
	 * Begins the processing the connection represented by the specified  TcpClient socket.
	 */
    private IAsyncResult BeginProcessConnection(TcpClient connection, AsyncCallback ucb, object ust)
    {
        const int BUFFER_SIZE = 1024;
        GenericAsyncResult<int> gar = new GenericAsyncResult<int>(ucb, ust, false);
        NetworkStream stream = null;
        try
        {

            /**
			 * Get a stream for reading and writing through the socket.
			 */

            stream = connection.GetStream();

            /**
			 * Read asynchronously the client request that we know that is smaller than
			 * BUFFER_SIZE bytes
			 */
            byte[] requestBuffer = new byte[BUFFER_SIZE];
            stream.BeginRead(requestBuffer, 0, requestBuffer.Length, async (ar) => {
                try
                {
                    int bytesRead = stream.EndRead(ar);
                    string request = Encoding.ASCII.GetString(requestBuffer, 0, bytesRead);
                    Console.WriteLine("-->[{0}]", request);

                    /**
					 * We use a task combinator to simulate a random service time before
					 * send the response (this is allowed here because we specified an asynchornous
					 * lambda).
					 */
                    await Task.Delay(random.Value.Next(MIN_SERVICE_TIME, MAX_SERVICE_TIME));
                    try
                    {
                        // compute reponse ant send it to the client
                        string response = request.ToUpper();
                        byte[] responseBuffer = Encoding.ASCII.GetBytes(response);
                        stream.Write(responseBuffer, 0, responseBuffer.Length);
                        Console.WriteLine("<--[{0}]", response);
                        // complete APM asynchronous operation with success
                        gar.SetResult(0);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("***error: {0}", ex.Message);
                        // complete APM asynchronous operation with thrown exception
                        gar.SetException(ex);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("***error: {0}", ex.Message);
                    // complete APM asynchronous operation with thrown exception
                    gar.SetException(ex);
                }
                finally
                {
                    // anyway, close stream and close client socket
                    stream.Close();
                    connection.Close();
                }
            }, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine("***error: {0}", ex.Message);

            // close everything
            connection.Close();
            if (stream != null)
                stream.Close();

            // complete asynchronous operation with the thrown exception
            gar.SetException(ex);
        }
        return gar;
    }

    /**
 	 * Ends the processing represented by the specified IAsyncResult object.
 	 */
    private void EndProcessConnection(IAsyncResult ar)
    {
        // free allocated resources
        int ignored = ((GenericAsyncResult<int>)ar).Result;
    }

    /**
 	 * The callback specified when BeginAcceptTcpClient is called.
	 */
    private void OnAccept(IAsyncResult ar)
    {
        if (!ar.CompletedSynchronously)
        {
            AcceptProcessing(ar);
        }
        else
        {

            /**
		 	 * Recursive call - limit the number of allowed reentrancies
		 	 */

            Console.WriteLine("\n--COMPLETED SYNCHRONOUSLY---\n");
            if (nestedIOCallbacks.Value < MAX_NESTED_IO_CALLBACKS)
            {

                /**
			 	 * Execute processing on the current thread, increment before the private
			 	 * reentrancy counter
			 	 */

                nestedIOCallbacks.Value++;
                AcceptProcessing(ar);
                nestedIOCallbacks.Value--;
            }
            else
            {

                /**
			 	 * We reach the maximum number of nested callbacks on the current thread, so
			 	 * break the nesting, execution AcceptProcessing on worker thread pool's thread.
			 	 */

                ThreadPool.QueueUserWorkItem((_) => AcceptProcessing(ar));
            }
        }
    }

    /**
	 * Processes an accept
	 */
    private void AcceptProcessing(IAsyncResult ar)
    {
        TcpClient connection = null;

        try
        {
            connection = server.EndAcceptTcpClient(ar);

            /**
			 * Increment the number of active connections and, if the we are below of
			 * maximum allowed, start accepting a new connection.
			 */

            int c = Interlocked.Increment(ref activeConnections);
            if (!shutdownInProgress && c < MAX_ACTIVE_CONNECTIONS)
                server.BeginAcceptTcpClient(OnAccept, null);

            /**
			 * Start the processing the previously accepted connection.
			 * Here, we alse use and asynchronous APM interface.
			 * However, we ignore exceptions thrown when processing a client connection.
			 */

            BeginProcessConnection(connection, (ar2) => {
                try
                {
                    // free allocated resources
                    EndProcessConnection(ar2);
                }
                catch (Exception) { /* for now, we ignore exceptions - Not to do! */ }

                /**
   			 	 * When the processing of the connection is completed, decrement the number of active
				 * connections. If a shut down isn't in progress and if the number of active connections
				 * was equal to the maximum allowed, we must accept a new connection.
   			 	 * Otherwise, if the number of active connections drops to zero and the shut down
   			 	 * was initiated, set the server idle event.
   			 	 */

                int c2 = Interlocked.Decrement(ref activeConnections);
                if (!shutdownInProgress && c2 == MAX_ACTIVE_CONNECTIONS - 1)
                    server.BeginAcceptTcpClient(OnAccept, null);
                else if (shutdownInProgress && c2 == 0)
                {
                    Console.WriteLine("\n--Finishing on Callback--\n");
                    listenAsyncResult.SetResult(0);
                }

            }, null);
        }
        catch (SocketException sockex)
        {
            Console.WriteLine("***socket exception: {0}", sockex.Message);
        }
        catch (ObjectDisposedException)
        {
            /**
			 * benign exceptions that occurs when the server shuts down
			 * and stops listening to the server socket.
			 */
        }
    }

    /**
	 * Start listening for client requests.
	 */
    public IAsyncResult BeginListen(AsyncCallback ucb, object ust)
    {
        listenAsyncResult = new GenericAsyncResult<int>(ucb, ust, false);

        // accept the first conenction request and return IAsyncResult for the
        // asynchronous server operation
        server.BeginAcceptTcpClient(OnAccept, null);
        return listenAsyncResult;
    }

    /**
	 * Ends the listening for client connections
	 */
    public void EndListen(IAsyncResult ar)
    {
        // free allocated resources
        int ignored = ((GenericAsyncResult<int>)ar).Result;
    }

    // Shutdown the server..
    private void ShutdownAndWaitTermination()
    {

        /**
 	     * Before close the listener socket, try to process the connections already accepted
		 * by the Operating System's socket layer.
		 * Since that it is possible that we never see no connections pending, due to an
		 * uninterrupted arrival of new connection requests, we poll for a limited amount of time.
 	     */

        for (int i = 0; i < WAIT_FOR_IDLE_TIMEOUT; i += POLLING_INTERVAL)
        {
            if (!server.Pending())
            {
                break;
            }
            Thread.Sleep(POLLING_INTERVAL);
        }

        // Stop listening connection requests.
        server.Stop();

        /**
		 * Signal that shutdown is in progress and then check if all accepted are processed.
		 * We must insert a full fence to prevent the Release/Acquire hazard!
		 */
        shutdownInProgress = true;
        Interlocked.MemoryBarrier();
        if (activeConnections == 0)
        {
            Console.WriteLine("\n--Finishing on Shutdown--\n");
            listenAsyncResult.SetResult(0);
        }

        /**
		 * Wait until all previously accepted connections are processed.
		 * Set shut down in progress, and wait until the processing of all
		 * accepted connections is finished.
		 */

        EndListen(listenAsyncResult);
    }

    public static void Main()
    {
        TcpMultithreadedApmEchoServer echoServer = new TcpMultithreadedApmEchoServer();

        // Start listening for client requests and process them asynchronously

        echoServer.BeginListen(null, null);

        /**
		 * Wait a <enter> from the console to terminate the server. 
		 */

        Console.WriteLine("Hit <enter> to exit the server...");
        Console.ReadLine();

        // Initiate server shutdown
        echoServer.ShutdownAndWaitTermination();
    }
}
