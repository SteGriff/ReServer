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
        /// Given a file path which may be missing the file extension, search the directory:
        /// first for the exact file specified, then if not found,
        /// for a matching file (based on HTTP Accept preference)
        /// and return its full path (with extension) or null 
        /// </summary>
        /// <param name="localPath">File path which may be missing file extension</param>
        /// <param name="acceptableTypes">The client-specified Accept types from HTTP header</param>
        /// <returns>Path to the most preferred file, including extension, or null</returns>
        public static string BestMatchingFile(string localPath, IList<MediaTypeWithQualityHeaderValue> acceptableTypes)
        {
            if (File.Exists(localPath))
            {
                return localPath;
            }

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
                //Absolutely nothing found
                return null;
            }
            else if (filesWithThatName.Count == 1)
            {
                //Exactly one match for that extensionless file name
                return filesWithThatName.Single();
            }
            else
            {
                //More than one file with that extensionless name, 
                // so pick the best one by Accept header:

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

        /// <summary>
        /// Wrapper for BestMatchingFile(string localPath, IList[MediaTypeWithQualityHeaderValue] acceptableTypes)
        /// better for deleting a file matching a specific contentType
        /// </summary>
        /// <param name="localPath"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static string BestMatchingFile(string localPath, string contentType)
        {
            contentType = contentType.RemoveSubtypes();

            var acceptableTypes = new List<MediaTypeWithQualityHeaderValue>();

            var thisMedia = new MediaTypeWithQualityHeaderValue(contentType, 1.0d);
            acceptableTypes.Add(thisMedia);

            return BestMatchingFile(localPath, acceptableTypes);
        }
    }
}
