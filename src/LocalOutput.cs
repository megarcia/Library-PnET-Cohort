// NOTE: File --> System.IO
// NOTE: IOException --> System.IO
// NOTE: StreamWriter --> System.IO

using System.Collections.Generic;
using System.IO;

namespace Landis.Library.PnETCohorts
{
    public class LocalOutput
    {
        public static string PnETOutputSites;
        private List<string> FileContent;
        public string FileName { get; private set; }
        public string SiteName { get; private set; }
        public string Path { get; private set; }
        
        public LocalOutput(string SiteName, string FileName, string Header)
        {
            this.SiteName = SiteName;
            Path = "Output" + System.IO.Path.DirectorySeparatorChar + PnETOutputSites + System.IO.Path.DirectorySeparatorChar + SiteName + System.IO.Path.DirectorySeparatorChar;
            this.FileName = FileName;
            if (File.Exists(Path + FileName))
                File.Delete(Path + FileName);
            if (Directory.Exists(Path) == false)
                Directory.CreateDirectory(Path);
            FileContent = new List<string>(new string[] { Header });
            Write();
        }

        public LocalOutput(LocalOutput localOutput)
        {
            SiteName = localOutput.SiteName;
            Path = localOutput.Path;
            FileName = localOutput.FileName;
        }
        public void Add(string s)
        {
            FileContent.Add(s);
        }

        public void Write()
        {
            while (true)
            {
                try
                {
                    StreamWriter sw = new StreamWriter(System.IO.Path.Combine(Path, FileName), true);
                    foreach (string line in FileContent)
                    {
                        sw.WriteLine(line);
                    }
                    sw.Close();
                    FileContent.Clear();
                    return;
                }
                catch (IOException e)
                {
                    Globals.ModelCore.UI.WriteLine("Cannot write to " + System.IO.Path.Combine(Path, FileName) + " " + e.Message);
                }
            }           
        }
    }
}
