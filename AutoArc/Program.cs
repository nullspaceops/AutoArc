using System;
using System.Net;
using System.Security.Cryptography;
using System.Diagnostics;
using System.IO;

namespace AutoArc
{
    public class Program
    {
        public const string OldSuffix = ".old";
        public const string ChecksumSuffix = ".md5sum";

        public readonly static string[] ArcURI = { "https://www.deltaconnected.com/arcdps/x64/", "http://martionlabs.com/wp-content/uploads/" };
        public readonly static string[] ArcNames = { "d3d9.dll", "d3d9_arcdps_mechanics.dll" };

        //  Custom Path
        private const string LocalPath = "C:/Spiele/Guild Wars 2/bin64/";
        //public const string LocalPath = "bin64/";
        public readonly static string[] GameExeName = {"Gw2", "Gw2-64"};


        private static void Main(string[] args)
        {
            foreach(string Proc in GameExeName)
            {
                KillGame(Proc);                
            }

            foreach(string ArcDll in ArcNames)
            {
                Remove(LocalPath, ArcDll);
            }

            for (int k = 0; k < ArcURI.Length;) 
            {
                Download(ArcURI[k], LocalPath, ArcNames[k], false);
                k += 1;
            }
        }

        private static void Download(string remotePath, string localPath, string name, bool hasChecksum)
        {

            Console.WriteLine("Checking for {0} updates...", name);

            {
                HttpWebResponse resp = (HttpWebResponse)WebRequest.Create(remotePath + name).GetResponse();
               

                if (File.Exists(localPath + name) && File.GetLastWriteTimeUtc(localPath + name) >= resp.LastModified)
                {
                    Console.WriteLine("Up to date ({0}).", resp.LastModified);
                    return;
                }

                Console.WriteLine("Downloading update ({0})...", resp.LastModified);
                resp.Close();
            }

            if (File.Exists(localPath + name))
                File.Copy(localPath + name, localPath + name + OldSuffix, true);

            byte[] file;
            using (WebClient wc = new WebClient())
            {
                file = wc.DownloadData(remotePath + name);

                if (hasChecksum)
                {
                    using (MD5 md5 = MD5.Create())
                    {
                        string checksum = wc.DownloadString(remotePath + name + ChecksumSuffix).Split(' ')[0];
                        string downloadChecksum = BitConverter.ToString(md5.ComputeHash(file)).Replace("-", "").ToLower();
                        bool equal = checksum == downloadChecksum;
                        Console.WriteLine("{0} == {1} ({2})", checksum, downloadChecksum, equal);
                        if (!equal)
                        {
                            Console.ReadKey();
                            return;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No checksum verification.");
                }
                wc.CancelAsync();
            }
            

            File.WriteAllBytes(localPath + name, file);
        }
        private static void Remove(string LocalPath, string name)
        {
            // Need it to remove all files to download "d3d9_arcdps_extras.dll" properly?...
            System.IO.File.Delete(LocalPath + name);

            Console.WriteLine("Removing {0}...", name);
        }

        private static void KillGame(string GameExeName)
        {
            foreach (Process process in Process.GetProcessesByName(GameExeName))
            {
                process.Kill();
                Console.WriteLine("Sleeping...");
                Console.WriteLine("################");
                System.Threading.Thread.Sleep(5000);
            }
        }

    }
}
