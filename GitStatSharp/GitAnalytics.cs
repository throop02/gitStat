using System;
using System.Collections.Generic;
using System.Linq;

namespace GitStatSharp
{
    public class GitAnalytics
    {
        private readonly GitApi api;

        public GitAnalytics(string gitBinFolderLocation, string repoLocation)
        {
            this.api = new GitApi(new CoreApi(gitBinFolderLocation, repoLocation));
        }

        public GitAnalytics(CoreApi coreApi)
        {
            this.api = new GitApi(coreApi);
        }

        public Dictionary<DateTime, int> CalculateDailyDelta(DateTime startDate, DateTime endDate, string baseBranch, string deltaBranch)
        {

            var deltaCommits = getLatestCommitsPerDayOverPeriod(startDate, endDate, deltaBranch);
            var baseCommits = getLatestCommitsPerDayOverPeriod(startDate, endDate, baseBranch);
            var results = new Dictionary<DateTime, int>();

            foreach (var item in deltaCommits)
            {
                var delta = deltaCommits.Where(x => string.IsNullOrEmpty(x.Value) == false && x.Key >= item.Key).OrderBy(x => x.Key).FirstOrDefault();
                var basec = baseCommits.Where(x => string.IsNullOrEmpty(x.Value) == false && x.Key >= item.Key).OrderBy(x => x.Key).FirstOrDefault();

                if (delta.Key != default(DateTime) && basec.Key != default(DateTime))
                {
                    var result = api.DiffShortStat(delta.Value, basec.Value);
                    results.Add(item.Key, result.insertions + result.deletions);
                }
            }
            return results;
        }

        private Dictionary<DateTime, string> getLatestCommitsPerDayOverPeriod(DateTime startDate, DateTime endDate, string branch)
        {
            var commits = new Dictionary<DateTime, string>();

            DateTime currentDate = startDate;
            while (currentDate < endDate)
            {
                api.CheckoutBranchOrCommit(branch);
                commits.Add(currentDate, api.GetLatestCommitWithinTimespanFirstParent(currentDate, currentDate.AddDays(1)));

                currentDate = currentDate.AddDays(1);
            }

            return commits;
        }
    }
}
