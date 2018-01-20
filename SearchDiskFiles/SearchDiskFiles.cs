using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SearchDiskFiles {
    class SearchResult {
        public int TotalFiles;
        
        public ConcurrentBag<String> Files = new ConcurrentBag<string>();
    }

    class FileRes
    {
        public string FileName { get; set; }
        public long Length { get; set; }
    }
    class SearchDiskFiles {
        // Caller must catch the AggregateException
        public static Task<SearchResult> Find(string folder,long maxFiles, CancellationToken token)
        {
            var task = Task.Factory.StartNew<SearchResult>(() => {
                var result = new SearchResult();
                // get all the files in that directory to count
                var files = Directory.GetFiles(folder);

                result.TotalFiles = files.Length; // save the count
 
                // if canceled at this point, we dont need to read all the files
                token.ThrowIfCancellationRequested();
                // used to throttled the IO concurrency in case we have many files
                SemaphoreSlim semaphore = new SemaphoreSlim(20);


                
                var biggestFiles = new FileRes[maxFiles];
                var thislock = new object();

                Parallel.ForEach(files, (file, loopSate) => {
                    semaphore.Wait(token); // cancel the wait if token is signaled
                    using (var reader = File.OpenRead(file))
                    {
                        lock(thislock){

                            var actualCount = biggestFiles.Count(e => e!=null);

                            if (actualCount < maxFiles)
                                biggestFiles[actualCount] = new FileRes {FileName = file, Length = reader.Length};
                            else
                            {
                                biggestFiles = biggestFiles.OrderByDescending(f => f.Length).ToArray();
                                var last = biggestFiles[maxFiles - 1];

                                if (reader.Length > last.Length)
                                {
                                    biggestFiles[maxFiles - 1] = new FileRes {FileName = file, Length = reader.Length};
                                }

                            }
                        }

                    }
                    if (token.IsCancellationRequested)
                        loopSate.Stop(); // cancel all the remaining loop tasks
                    semaphore.Release();
                });
                foreach (var biggestFile in biggestFiles)
                {
                    result.Files.Add(biggestFile.FileName);
                }
                return result;
            }, token);
            return task;
        }
    }
}
