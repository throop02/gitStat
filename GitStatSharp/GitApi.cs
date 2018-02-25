using System;

namespace GitStatSharp
{
    public class GitApi
    {
        private readonly CoreApi core;

        public GitApi(CoreApi coreApi)
        {
            this.core = coreApi;
        }

        public string GetCurrentBranch()
        {
            var response = this.core.ExecuteRequest("branch");
            return parseResponse("* ", "\n", response[1]);
        }

        public string CheckoutBranchOrCommit(string branchOrHash)
        {
            var response = this.core.ExecuteRequest($"checkout {branchOrHash}");
            return response[0] ?? response[1];
        }

        public string DeleteBranch(string branch)
        {
            var response = this.core.ExecuteRequest($"branch -d {branch}");
            return response[0] ?? response[1];
        }

        public string BranchFromCommit(string commit, string branchName)
        {
            var response = this.core.ExecuteRequest($"branch {branchName} {commit}");
            return response[0] ?? response[1];
        }

        public string GetLatestCommitWithinTimespanFirstParent(DateTime start, DateTime end)
        {
            var response = this.core.ExecuteRequest($"--no-pager log --first-parent --after=\"{start.ToString("yyyy-MM-dd")}\" --before=\"{end.ToString("yyyy-MM-dd")}\"");
            return parseResponse("commit ", "\n", response[1]);
        }

        public DiffShortStatResponse DiffShortStat(string c1, string c2)
        {
            var parsedResp = new DiffShortStatResponse();

            try
            {
                var r = this.core.ExecuteRequest($"diff --shortstat {c1} {c2}")[1];

                parsedResp = new DiffShortStatResponse
                {
                    filesChanged = Convert.ToInt32(parseResponse("  ", " files changed, ", r)),
                    deletions = Convert.ToInt32(parseResponse(" insertions(+), ", " deletions(-)", r)),
                    insertions = Convert.ToInt32(parseResponse(" files changed, ", " insertions(+), ", r))
                };
            }
            catch (Exception ex)
            {

            }
            return parsedResp;
        }

        private string parseResponse(string identifier, string terminator, string body)
        {
            body = body.Trim('\n');
            if (string.IsNullOrEmpty(body)) { return ""; }
            string parsedResponse = body.Substring(body.IndexOf(identifier) + identifier.Length);
            parsedResponse = parsedResponse.Substring(0, parsedResponse.IndexOf(terminator));

            int x = parsedResponse.IndexOf("\n");
            if (x > 0)
            {
                parsedResponse = parsedResponse.Remove(x);
            }

            return parsedResponse.Trim();
        }
    }
}
