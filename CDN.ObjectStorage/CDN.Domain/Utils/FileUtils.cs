using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CDN.Domain.Utils
{
    public static class FileUtils
    {
        public static void MergeFiles(string outpuFilePath, List<string> filesToMerge)
        {
            using (var output = File.OpenWrite(outpuFilePath))
            {
                foreach (var inputFile in filesToMerge)
                {
                    using (var input = File.OpenRead(inputFile))
                    {
                        input.CopyTo(output);
                    }

                    //Free up disk space
                    File.Delete(inputFile);
                }
            }
        }
            
        /// <summary>
        /// Split chunks by pairs, so we will merge chunks in parallel
        /// for example we have chunks: 1,2,3,4,5,6,7
        /// this algorythm will merge 1->2, 3->4, 5->6 chunks in parallel
        /// and then recursively merge result
        /// </summary>
        /// <param name="filesToMerge"></param>
        /// <returns>output file path</returns>
        public static string MergeFilesRecursively(List<string> filesToMerge)
        {
            var tasks = new List<Task>();

            var mergedFiles = new List<string>();

            // Split chunks by pairs, so we will merge chunks in parallel
            // for example we have chunks: 1,2,3,4,5,6,7
            // this algorythm will merge 1->2, 3->4, 5->6 chunks in parallel
            for (var i = 0; i < filesToMerge.Count / 2; i += 2)
            {
                var currentFileIndex = i;
                var nextFileIndex = i == 0 ? 1 : i + 1;

                var task = Task.Factory.StartNew(() =>
                {
                    var firstFile = filesToMerge[currentFileIndex];
                    var secondFile = filesToMerge[nextFileIndex];

                    //Save chunks which we merged
                    mergedFiles.Add(secondFile);

                    //Merge chunks
                    MergeFiles(firstFile, secondFile);

                    //Delete second file, to free up disk space
                    File.Delete(secondFile);
                });

                tasks.Add(task);
            }

            //Wait for result
            Task.WaitAll(tasks.ToArray());

            var notMergedFiles = filesToMerge.Except(mergedFiles).ToList();

            //if there is only one not merged files, means that it's an output file
            if (notMergedFiles.Count == 1)
                return mergedFiles[0];

            //recursively merge already merged chunks in parallel
            return MergeFilesRecursively(notMergedFiles);
        }

        public static string MergeFiles(string firstFile, string secondFile)
        {
            using (var outputStream = File.OpenWrite(firstFile))
            using (var inputStream = File.OpenRead(secondFile))
            {
                // Buffer size can be passed as the second argument.
                inputStream.CopyTo(outputStream);
            }
            
            return firstFile;
        }

        public static bool IsValidFileName(string fileName)
        {
            return !string.IsNullOrEmpty(fileName) &&
                   fileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
        }
    }
}