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
            var baseCommits = new Dictionary<DateTime, string>();
            var deltaCommits = new Dictionary<DateTime, string>();
            var baseDayMultiplier = 0;
            var deltaDayMultiplier = 0;
            var multiplierMax = 25;

            while (deltaCommits.Count(x => string.IsNullOrEmpty(x.Value) == false) == 0 && deltaDayMultiplier < multiplierMax)
            {
                deltaCommits = getLatestCommitsPerDayOverPeriod(startDate.AddDays((15 * deltaDayMultiplier) * -1), endDate, deltaBranch);
                deltaDayMultiplier += 1;
            }

            while (baseCommits.Count(x => string.IsNullOrEmpty(x.Value) == false) == 0 && baseDayMultiplier < multiplierMax)
            {
                baseCommits = getLatestCommitsPerDayOverPeriod(startDate.AddDays((15 * baseDayMultiplier) * -1), endDate, baseBranch);
                baseDayMultiplier += 1;
            }

            var results = new Dictionary<DateTime, int>();

            foreach (var item in deltaCommits)
            {
                var delta = deltaCommits.Where(x => string.IsNullOrEmpty(x.Value) == false && x.Key <= item.Key).OrderByDescending(x => x.Key).FirstOrDefault();
                var basec = baseCommits.Where(x => string.IsNullOrEmpty(x.Value) == false && x.Key <= item.Key).OrderByDescending(x => x.Key).FirstOrDefault();

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
