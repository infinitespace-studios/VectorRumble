using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

namespace VectorRumble
{
    internal static class Helper
    {
        const string MyDocumentsDirectory = "Documents"; // Making this a constant in case we need to IFDEF this per platform later

        public static string GetMyDocumentsFolder ()
        {
            // Once our app is signed, we can't change it, so let's look in the Special "MyDocuments" folder for user created data
            var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (!myDocuments.ToLower().Contains("documents")) {
                myDocuments = Path.Combine(myDocuments, MyDocumentsDirectory);
            }

            return myDocuments;
        }

        public static string GetAssemblyTitle ()
        {
            var assembly = typeof(Arena).Assembly;
            var assemblyAttributes = (AssemblyTitleAttribute[])assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), true);
            if (assemblyAttributes.Length > 0) {
                return assemblyAttributes[0].Title;
            }
            else {
                return string.Empty;
            }
        }

        /// <summary>
        /// Get files from the Content directory using TitleContainer.
        /// This is a more cross-platform way to access content files.
        /// </summary>
        /// <param name="contentDirectory">The subdirectory within Content (e.g., "Ships", "Arenas", etc)</param>
        /// <param name="manifestFileName">The manifest file listing all files (e.g., "ships.txt")</param>
        /// <returns>Array of paths that we can then use with TitleContainer.OpenStream()</returns>
        public static string[] GetFilesFromContent(string contentDirectory, string manifestFileName)
        {
            var manifestPath = Path.Combine("Content", contentDirectory, manifestFileName);

            try
            {
                using (var manifestStream = TitleContainer.OpenStream(manifestPath))
                using (var manifestStreamReader = new StreamReader(manifestStream))
                {
                    var files = new List<string>();
                    string line;
                    while ((line = manifestStreamReader.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (!string.IsNullOrEmpty(line))
                        {
                            // Return the full content path
                            files.Add(Path.Combine("Content", contentDirectory, line));
                        }
                    }
                    return files.ToArray();
                }
            }
            catch (FileNotFoundException)
            {
                // Manifest file doesn't exist
                return null;
            }
        }

        public static string[] GetFilesFromFolders (string [] folders, string extensionFilter)
        {
            string root = Path.GetDirectoryName (System.AppContext.BaseDirectory);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                string resourceFolder = Path.Combine (root, "..", "..", "Resources");
                if (Directory.Exists(resourceFolder)) {
                    root = resourceFolder;
                }
                resourceFolder = Path.Combine (root, "..", "Resources");
                if (Directory.Exists(resourceFolder)) {
                    root = resourceFolder;
                }
            }
            var directory = Path.Combine (root, Path.Combine(folders));
            if (Directory.Exists(directory)) {
                return Directory.GetFiles(directory, extensionFilter);
            }
            else {
                return null;
            }
        }

        public static Color ColorParse (string colourString)
        {
            var value = uint.Parse (colourString, System.Globalization.NumberStyles.HexNumber);
            return new Color ((int)(value >> 16 & 0xFF),
                            (int)(value >> 8 & 0xFF),
                            (int)(value >> 0 & 0xFF),
                            (int)(value >> 24 & 0xFF));
        }
    }
}
