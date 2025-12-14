using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MacroEngine.Models;
using Newtonsoft.Json;

namespace MacroMaker.Services
{
    /// <summary>
    /// Handles saving and loading macros and settings
    /// </summary>
    public class StorageService
    {
        private const string MacrosDirectory = "SavedMacros";
        private const string SettingsFile = "AppSettings.json";
        private const string FileExtension = ".macro";

        public StorageService()
        {
            // Ensure macros directory exists
            if (!Directory.Exists(MacrosDirectory))
            {
                Directory.CreateDirectory(MacrosDirectory);
            }
        }

        /// <summary>
        /// Saves a macro to disk
        /// </summary>
        public void SaveMacro(Macro macro)
        {
            if (macro == null)
                throw new ArgumentNullException(nameof(macro));

            string fileName = SanitizeFileName(macro.Name) + FileExtension;
            string filePath = Path.Combine(MacrosDirectory, fileName);

            string json = JsonConvert.SerializeObject(macro, Formatting.Indented);
            File.WriteAllText(filePath, json);

            macro.ModifiedDate = DateTime.Now;
        }

        /// <summary>
        /// Loads a macro from disk
        /// </summary>
        public Macro? LoadMacro(string fileName)
        {
            string filePath = Path.Combine(MacrosDirectory, fileName);

            if (!File.Exists(filePath))
                return null;

            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<Macro>(json);
        }

        /// <summary>
        /// Loads all macros from disk
        /// </summary>
        public List<Macro> LoadAllMacros()
        {
            var macros = new List<Macro>();

            if (!Directory.Exists(MacrosDirectory))
                return macros;

            var files = Directory.GetFiles(MacrosDirectory, "*" + FileExtension);

            foreach (var file in files)
            {
                try
                {
                    string json = File.ReadAllText(file);
                    var macro = JsonConvert.DeserializeObject<Macro>(json);
                    if (macro != null)
                    {
                        macros.Add(macro);
                    }
                }
                catch (Exception ex)
                {
                    // Log error but continue loading other macros
                    Console.WriteLine($"Error loading macro {file}: {ex.Message}");
                }
            }

            return macros.OrderByDescending(m => m.ModifiedDate).ToList();
        }

        /// <summary>
        /// Deletes a macro from disk
        /// </summary>
        public void DeleteMacro(Macro macro)
        {
            if (macro == null)
                throw new ArgumentNullException(nameof(macro));

            string fileName = SanitizeFileName(macro.Name) + FileExtension;
            string filePath = Path.Combine(MacrosDirectory, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        /// <summary>
        /// Exports a macro to a specified location
        /// </summary>
        public void ExportMacro(Macro macro, string destinationPath)
        {
            if (macro == null)
                throw new ArgumentNullException(nameof(macro));

            string json = JsonConvert.SerializeObject(macro, Formatting.Indented);
            File.WriteAllText(destinationPath, json);
        }

        /// <summary>
        /// Imports a macro from a file
        /// </summary>
        public Macro? ImportMacro(string sourcePath)
        {
            if (!File.Exists(sourcePath))
                return null;

            string json = File.ReadAllText(sourcePath);
            var macro = JsonConvert.DeserializeObject<Macro>(json);

            if (macro != null)
            {
                // Generate new ID to avoid conflicts
                macro.Id = Guid.NewGuid();
                macro.CreatedDate = DateTime.Now;
                macro.ModifiedDate = DateTime.Now;

                SaveMacro(macro);
            }

            return macro;
        }

        /// <summary>
        /// Saves application settings
        /// </summary>
        public void SaveSettings(Dictionary<string, object> settings)
        {
            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(SettingsFile, json);
        }

        /// <summary>
        /// Loads application settings
        /// </summary>
        public Dictionary<string, object> LoadSettings()
        {
            if (!File.Exists(SettingsFile))
                return new Dictionary<string, object>();

            try
            {
                string json = File.ReadAllText(SettingsFile);
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
            }
            catch
            {
                return new Dictionary<string, object>();
            }
        }

        private string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            return new string(fileName.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
        }
    }
}
