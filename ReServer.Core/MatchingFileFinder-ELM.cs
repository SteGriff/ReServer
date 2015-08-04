using ReServer.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;

namespace ReServer.Core
{
    public static class MatchingFileFinder
    {
        /// <summary>
        /// Given a file path which is missing the file extension, search the directory
        /// for a matching file based on HTTP Accept preference and return its full path (with extension)
        /// </summary>
        /// <param name="localPath">Path to a file without file extension</param>
        /// <param name="acceptableTypes">The client-specified Accept types from HTTP header</param>
        /// <returns>Path to the most preferred file, including extension</returns>
        public static string BestMatchingFile(string localPath, IList<MediaTypeWithQualityHeaderValue> acceptableTypes)
        {
            string directory = Path.GetDirectoryName(localPath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(localPath);
            List<string> filesWithThatName;

            try
            {
                filesWithThatName = Directory.EnumerateFiles(directory)
                    .Where(f => Path.GetFileNameWithoutExtension(f) == fileNameWithoutExtension)
                    .ToList();
            }
            catch (Exception)
            {
                //Directory not found
                return null;
            }
            
            if (filesWithThatName.Count == 0)
            {
                return null;
            }
            else if (filesWithThatName.Count == 1)
            {
                return filesWithThatName.Single();
            }
            else
            {
                // This structure is a list of lists, arranged by accept types,
                // then by potential extensions for those:

                // "application/json" => [".json"]
                // "text/markdown" => [".markdown", ".md"]

                var acceptableExtensions = acceptableTypes
                    .OrderByDescending(at => at.Quality)
                    .Select(at => at.MediaType.GetFileExtensionCandidates());

                //Check the accept types from best to worst
                foreach (var extensionsForAcceptableType in acceptableExtensions)
                {
                    if (extensionsForAcceptableType.Count == 0)
                    {
                        // This accept type doesn't have any native file extensions
                        // ignore it
                        continue;
                    }

                    // Check whether any file in this dir has an extension matching
                    // an extension owned by this preference
                    foreach (var fileMatchingName in filesWithThatName)
                    {
                        var acceptableMatch = extensionsForAcceptableType
                            .Where(ext => ext == Path.GetExtension(fileMatchingName))
                            .ToList();

                        if (acceptableMatch.Count == 0)
                        {
                            //Check the next fileMatchingName
                            //i.e. "file.md" didn't match ".json", so check next, "file.json"
                            continue;
                        }
                        else
                        {
                            // If there's 1, it is definitely the right file
                            // If more than 1, this is the webmaster's problem;
                            // They shouldn't have file.md AND file.markdown, for example.
                            return fileMatchingName;
                        }
                    }
                }

                // None of the files with that name matched any accept header
                // Just return the first one
                return filesWithThatName.First();
            }

        }
    }
}
