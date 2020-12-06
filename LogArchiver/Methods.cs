using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Exiled.API.Features;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

namespace LogArchiver
{
    public class Methods
    {
        private readonly Plugin plugin;
        public Methods(Plugin plugin) => this.plugin = plugin;

        public Dictionary<string, List<Tuple<string, DateTime>>> FindExistingArchives()
        {
            Dictionary<string, List<Tuple<string, DateTime>>> dict =
                new Dictionary<string, List<Tuple<string, DateTime>>>();
            
            foreach (string directory in plugin.Config.LogLocations)
            {
                try
                {
                    if (!Directory.Exists(directory))
                    {
                        Log.Warn($"{directory} does not exist. Skipping.");
                        continue;
                    }

                    dict.Add(directory, new List<Tuple<string, DateTime>>());

                    foreach (string file in Directory.GetFiles(directory))
                    {
                        if (file.EndsWith(".tar.gz"))
                        {
                            Log.Debug($"{file} - {File.GetCreationTimeUtc(file)}", plugin.Config.Debug);
                            dict[directory].Add(new Tuple<string, DateTime>(file, File.GetCreationTimeUtc(file)));
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"{e}\n{e.StackTrace}");
                }
            }

            return dict;
        }

        public void CheckLogFiles()
        {
            foreach (string directory in plugin.Config.LogLocations)
            {
                Log.Debug($"Checking {directory}..", plugin.Config.Debug);
                if (!Directory.Exists(directory))
                {
                    Log.Warn($"{directory} does not exist. Skipping.");
                    continue;
                }

                string[] fileNames = Directory.GetFiles(directory);

                Log.Debug($"{directory} count: {fileNames.Length}", plugin.Config.Debug);
                if (fileNames.Length >= plugin.Config.FileLimit)
                    ArchiveFiles(directory);

                foreach (string subDir in Directory.GetDirectories(directory))
                {
                    fileNames = Directory.GetFiles(subDir);
                    Log.Debug($"{directory} sub count: {fileNames.Length}");
                    
                    if (fileNames.Length >= plugin.Config.FileLimit)
                        ArchiveFiles(subDir);
                }
            }
        }

        public void ArchiveFiles(string directory)
        {
            Log.Info($"Archival started for {directory}.");
            CheckArchives(directory);

            using (var outStream = File.Create(Path.Combine(directory, $"LogArchive-{DateTime.UtcNow.Ticks}.tar.gz")))
            using (var gzoStream = new GZipOutputStream(outStream))
            {
                var tarArchive = TarArchive.CreateOutputTarArchive(gzoStream);
                
                tarArchive.RootPath = directory.Replace('\\', '/');
                if (tarArchive.RootPath.EndsWith("/"))
                {
                    tarArchive.RootPath = tarArchive.RootPath.Remove(tarArchive.RootPath.Length - 1);
                }

                AddDirectoryFilesToTgz(tarArchive, directory);

                tarArchive.Close();
                
                Log.Info($"Archival completed for {directory}.");
            }
        }

        private void AddDirectoryFilesToTgz(TarArchive tarArchive, string sourceDirectory)
        {
            AddDirectoryFilesToTgz(tarArchive, sourceDirectory, string.Empty);
        }

        private void AddDirectoryFilesToTgz(TarArchive tarArchive, string sourceDirectory, string currentDirectory)
        {
            string pathToCurrentDirectory = Path.Combine(sourceDirectory, currentDirectory);
            
            string[] filePaths = Directory.GetFiles(pathToCurrentDirectory);
            foreach (string filePath in filePaths)
            {
                if (filePath.EndsWith(".tar.gz") || filePath.Contains("LogArchive-"))
                    continue;
                
                TarEntry tarEntry = TarEntry.CreateEntryFromFile(filePath);
                
                tarEntry.Name = filePath.Replace(sourceDirectory, "");
                
                if (tarEntry.Name.StartsWith("\\"))
                {
                    tarEntry.Name = tarEntry.Name.Substring(1);
                }

                tarArchive.WriteEntry(tarEntry, true);
                
                File.Delete(filePath);
            }
            
            string[] directories = Directory.GetDirectories(pathToCurrentDirectory);
            foreach (string directory in directories)
            {
                AddDirectoryFilesToTgz(tarArchive, sourceDirectory, directory);
            }
        }

        public void CheckArchives(string directory)
        {
            try
            {
                if (plugin.ExistingArchives.ContainsKey(directory))
                {
                    Log.Debug($"Checking {directory} archives..", plugin.Config.Debug);

                    if (plugin.ExistingArchives[directory].Count + 1 >= plugin.Config.ArchiveLimit)
                    {
                        Log.Debug($"{plugin.ExistingArchives[directory].Count + 1}", plugin.Config.Debug);
                        string toDelete = plugin.ExistingArchives[directory].OrderBy(x => x.Item2).FirstOrDefault()
                            ?.Item1;
                        Log.Debug($"{toDelete}", plugin.Config.Debug);
                        if (!string.IsNullOrEmpty(toDelete))
                            File.Delete(toDelete);
                    }
                }
                else
                    plugin.ExistingArchives.Add(directory, new List<Tuple<string, DateTime>>());
            }
            catch (Exception e)
            {
                Log.Error($"{e}\n{e.StackTrace}");
            }
        }
    }
}