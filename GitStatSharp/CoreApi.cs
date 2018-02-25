using System;
using System.Diagnostics;

namespace GitStatSharp
{
    public class CoreApi
    {
        private readonly string _gitBinLocation;
        private readonly string _repoLocation;

        public CoreApi(string gitBinLocation, string repoLocation)
        {
            this._gitBinLocation = gitBinLocation;
            this._repoLocation = repoLocation;
        }

        public string[] ExecuteRequest(string gitCommand)
        {
            ProcessStartInfo gitInfo = new ProcessStartInfo();
            gitInfo.CreateNoWindow = true;
            gitInfo.RedirectStandardError = true;
            gitInfo.RedirectStandardOutput = true;
            gitInfo.FileName = _gitBinLocation + @"\git.exe";
            gitInfo.UseShellExecute = false;


            Process gitProcess = new Process();
            gitInfo.Arguments = gitCommand;
            gitInfo.WorkingDirectory = _repoLocation;

            gitProcess.StartInfo = gitInfo;
            gitProcess.Start();

            var response = new string[2];
            //response[0] = gitProcess.StandardError.ReadToEnd();
            response[1] = gitProcess.StandardOutput.ReadToEnd();

            gitProcess.WaitForExit();
            gitProcess.Close();

            return response;
        }
    }
}
