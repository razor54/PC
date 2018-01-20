using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SearchDiskFiles {
    class Program {
        static void Main(string[] args)
        {
           string folder = @"C:\Windows\System32";
            
            long max = 10;
            var cts = new CancellationTokenSource();
            // create a task for the search
            var task = SearchDiskFiles.Find(folder, max, cts.Token);
            try
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                var result = task.Result; // wait for the result
                watch.Stop();
                Console.WriteLine("Time taken: {0}ms", watch.Elapsed.TotalMilliseconds);
                Console.WriteLine("Total Files Found: " + result.TotalFiles);
                Console.WriteLine("Biggest Files: ");
                foreach (var resultFile in result.Files)
                    Console.WriteLine(resultFile);
            }
            catch (AggregateException ex)
            {
                ex.Handle(e => {
                    Console.WriteLine("Exception: " + e.Message);
                    return true;
                });
            }
            Console.WriteLine("Press enter...");
            Console.ReadKey();
        }
    }
}