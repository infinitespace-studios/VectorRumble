using System;
#if WINDOWS_UAP
using System.Linq;
#endif
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
#if WINDOWS_UAP
            return String.Empty; // TODO Find correct UWP folder we can use for user created data
#else
            var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (!myDocuments.ToLower().Contains("documents")) {
                myDocuments = Path.Combine(myDocuments, MyDocumentsDirectory);
            }

            return myDocuments;
#endif
        }

        public static string GetAssemblyTitle ()
        {
#if WINDOWS_UAP
            var assemblyAttributes = typeof(Arena).GetTypeInfo().GetCustomAttributes<AssemblyTitleAttribute>(true).ToArray();
#else
            var assembly = typeof(Arena).Assembly;
            var assemblyAttributes = (AssemblyTitleAttribute[])assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), true);
#endif
            if (assemblyAttributes.Length > 0) {
                return assemblyAttributes[0].Title;
            }
            else {
                return string.Empty;
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
