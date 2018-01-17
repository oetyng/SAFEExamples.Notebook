using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Utils
{
    public class FileOps : IFileOps
    {
        public async Task TransferAssetsAsync(List<(string, string)> fileList)
        {
            foreach (var tuple in fileList)
            {
                using (var reader = new StreamReader(Path.Combine(".", tuple.Item1)))
                {
                //using (var reader = new StreamReader(Forms.Context.Assets.Open(tuple.Item1)))
                //{
                    using (var writer = new StreamWriter(Path.Combine(ConfigFilesPath, tuple.Item2)))
                    {
                        await writer.WriteAsync(await reader.ReadToEndAsync());
                        writer.Close();
                    }
                    reader.Close();
                }
            }
        }

        public string ConfigFilesPath
        {
            get
            {
                string path;
                // Resources -> /Library
                //path = Environment.GetFolderPath(Environment.SpecialFolder.Resources);
                //// Personal -> /data/data/@PACKAGE_NAME@/files
                path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                Debug.WriteLine($"ConfigFilesPath - {path}");
                return path;
            }
        }
    }
}