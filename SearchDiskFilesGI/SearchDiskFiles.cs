using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SearchDiskFilesGI {
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
        public static Task<SearchResult> Find(string folder,long numberofFiles, CancellationToken token, IProgress<CustomProgress> progress)
        {
            var task = Task.Factory.StartNew<SearchResult>(() =>
            {
                var result = new SearchResult();
                string[] files = null;
                try
                {
                    // get all the files in that directory (& subdirectories) to count
                    files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);
                }
                catch (UnauthorizedAccessException)
                {
                    // if we try to access a subdirectory which we don't have permission
                    // forget subdirectories, just search in the root folder
                    files = Directory.GetFiles(folder);
                }
                result.TotalFiles = files.Length; // save the count

                // if canceled at this point, we dont need to read all the files
                token.ThrowIfCancellationRequested();
                // used to throttled the IO concurrency in case we have many files
                SemaphoreSlim semaphore = new SemaphoreSlim(20);



                var biggestFiles = new FileRes[numberofFiles];
                var thislock = new object();


                Parallel.ForEach(files, (file, loopState) =>
                {
                    semaphore.Wait(token); // cancel the wait if token is signaled
                    var contains = false;
                    using (var reader = File.OpenRead(file))
                    {
                        lock (thislock)
                        {

                            var actualCount = biggestFiles.Count(e => e != null);

                            if (actualCount < numberofFiles)
                            {
                                biggestFiles[actualCount] = new FileRes { FileName = file, Length = reader.Length };
                                contains = true;
                            }
                                
                            else
                            {
                                biggestFiles = biggestFiles.OrderByDescending(f => f.Length).ToArray();
                                var last = biggestFiles[numberofFiles - 1];

                                if (reader.Length > last.Length)
                                {
                                    biggestFiles[numberofFiles - 1] = new FileRes { FileName = file, Length = reader.Length };
                                    contains = true;
                                }

                            }
                        }
                    }
                    var state = new CustomProgress(biggestFiles.Count(e => e != null), contains ? file : null);
                    progress.Report(state); // will run in the ui thread
                    if (token.IsCancellationRequested)
                        loopState.Stop(); // cancel all the remaining loop tasks
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
