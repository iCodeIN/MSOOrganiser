﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Calculators
{
    public class SeedingScoreCalculator
    {
        class SeedingInfo
        {
            public int ContestantId;
            public string GameCode;
            public int Score;
            public int LastGoldYear;
            public int LastSilverYear;
            public int LastBronzeYear;
        }



        public void CalculateSeedings()
        {
            var context = DataEntitiesProvider.Provide();
            context.Database.ExecuteSqlCommand("TRUNCATE TABLE [Seedings]");

            var seedings = new List<SeedingInfo>();
            var thisOlympiad = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();
            var thisYear = thisOlympiad.YearOf.Value;

            var seedableEntrants = context.Entrants.Where(x => x.Medal != null 
                // careful - digging back into old data
                && x.Mind_Sport_ID != null && x.Game_Code != null && x.Event != null)
                .OrderBy(x => x.Mind_Sport_ID)
                .ThenBy(x => x.Game_Code)
                .ToList();

            int lastContestantId = 0;
            string lastGameCode = "";

            foreach (var entrant in seedableEntrants)
            {
                // Catch duplicates
                if (entrant.Mind_Sport_ID.Value == lastContestantId && entrant.Game_Code == lastGameCode)
                    continue;

                lastContestantId = entrant.Mind_Sport_ID.Value;
                lastGameCode = entrant.Game_Code;

                var equivalentEvents = GetEquivalentEvents(entrant.Game_Code);
                var pastResults = context.Entrants
                    .Where(x => equivalentEvents.Contains(x.Game_Code)
                        // careful - digging back into old data
                        && x.Mind_Sport_ID != null && x.Event != null
                        && entrant.Mind_Sport_ID == x.Mind_Sport_ID && x.Medal != null)
                    .ToList();

                var seeding = GetSeedingScore(entrant, pastResults, thisYear);
                if (seeding == null) continue;
                if (seeding.Score == 0) continue;

                seedings.Add(seeding);

            }

            CalculateRanks(seedings);
        }

        /* Because some events which are equivaent for seeding purposes have changed codes
         * over the years */
        public IEnumerable<string> GetEquivalentEvents(string code)
        {
            switch (code)
            {
                case "ABOC":
                case "ABWC":
                    return new List<string>() { "ABOC", "ABWC" };
                case "KCOC":
                case "KCWC":
                    return new List<string>() { "KCOC", "KCWC" };
                case "SUOC":
                case "SUWC":
                    return new List<string>() { "SUOC", "SUWC" };
                case "TWOC":
                case "TWWC":
                    return new List<string>() { "TWOC", "TWWC" };
                default:
                    return new List<string>() { code };
            }
        }



        /* 
         * Last Year: G:20, S:10, B:5
2 Years Ago: G:16, S:8, B:4
3 Years Ago: G:12, S:6, B:3
4 Years Ago: G:8, S:4, B:2
5 years ago: G:4, S:2, B:1
Any Prior Golds:2
In addition, points for MSO rankings:
GM:10, IM:8, CM:6
He also suggested a caveat that the defending champion is number 1 seed if
they have at least 36 points; not sure how I feel about that. */

        private SeedingInfo GetSeedingScore(Entrant thisEntry, IEnumerable<Entrant> pastEntries, int thisYear)
        {
            int seedingScore = 0;
            int mostRecentGoldYear = 0;
            int mostRecentSilverYear = 0;
            int mostRecentBronzeYear = 0;
            // Part 1 = the points for recent years
            foreach (var pastEntry in pastEntries)
            {
                var pastYear = pastEntry.Event.Olympiad_Info.YearOf.Value;
                // Temporary fix for the 2007/7002 hack
                if (pastYear > 2100)
                    continue;
                if (pastEntry.Medal == "Gold")
                {
                    seedingScore += Math.Max(2, 4 * (pastYear + 6 - thisYear));
                    mostRecentGoldYear = Math.Max(mostRecentGoldYear, pastYear);
                }
                else if (pastEntry.Medal == "Silver")
                {
                    seedingScore += Math.Max(0, 2 * (pastYear + 6 - thisYear));
                    mostRecentSilverYear = Math.Max(mostRecentSilverYear, pastYear);
                }
                else if (pastEntry.Medal == "Bronze")
                {
                    seedingScore += Math.Max(0, 1 * (pastYear + 6 - thisYear));
                    mostRecentBronzeYear = Math.Max(mostRecentBronzeYear, pastYear);
                }
            }

            // Part 2 - the MSO rankings
            var golds = pastEntries.Count(x => x.Medal == "Gold");
            var silvers = pastEntries.Count(x => x.Medal == "Silver");
            var bronzes = pastEntries.Count(x => x.Medal == "Bronze");
            if (golds >= 2 || (golds == 1 && silvers >= 2))
                seedingScore += 10;     // Grandmaster
            else if ((golds == 1 && silvers + bronzes > 0)
                || silvers >= 2
                || (silvers == 1 && bronzes >= 2))
                seedingScore += 8;
            else if ((silvers == 1 && bronzes > 0)
                || bronzes >= 2)
                seedingScore += 6;

            // Part 3 - defending champion
            if (seedingScore >= 36 && 
                pastEntries.Any(x => x.Event.Olympiad_Info.YearOf.Value == thisYear - 1 && x.Medal == "Gold"))
                seedingScore += 1000;

            var seedingInfo = new SeedingInfo()
            {
                ContestantId = thisEntry.Mind_Sport_ID.Value,
                GameCode = thisEntry.Game_Code,
                Score = seedingScore,
                LastGoldYear = mostRecentGoldYear,
                LastSilverYear = mostRecentSilverYear,
                LastBronzeYear = mostRecentBronzeYear
            };

            return seedingInfo;
        }

        private void CalculateRanks(IEnumerable<SeedingInfo> seedings)
        {
            var context = DataEntitiesProvider.Provide();
            var sortedSeedings = seedings.OrderBy(x => x.GameCode)
                .ThenByDescending(x => x.Score)
                .ThenByDescending(x => x.LastGoldYear)
                .ThenByDescending(x => x.LastSilverYear)
                .ThenByDescending(x => x.LastBronzeYear)
                .ToList();

            var lastEventCode = "";
            var rank = 1;

            foreach (var seeding in sortedSeedings)
            {
                if (seeding.GameCode != lastEventCode)
                {
                    lastEventCode = seeding.GameCode;
                    rank = 1;
                }
                else
                {
                    rank++;
                }

                var dbSeeding = new Seeding()
                    {
                        ContestantId = seeding.ContestantId,
                        EventCode = seeding.GameCode,
                        Rank = rank,
                        Score = seeding.Score
                    };

                context.Seedings.Add(dbSeeding);
                context.SaveChanges();
            }
        }
    }
}
