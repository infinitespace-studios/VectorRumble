using System;
using System.IO;
using System.Reflection;
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

        public static string[] GetFilesFromFolders (string [] folders, string extensionFilter)
        {
            var directory = Path.Combine(folders);
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
