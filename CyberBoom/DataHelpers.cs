using Microsoft.EntityFrameworkCore;
using static Consts;

public static class DataHelpers
{
    public static string JoinFileNames(this IEnumerable<IFormFile> files) => files.Select(s => s.FileName).JoinStrings();

    public static string JoinStrings(this IEnumerable<string> files) => String.Join(TOKENS_SEPORATOR, files.Select(s => s));

    public static TimeSpan ParseDuration(this string duration) 
    {
        var durArr = duration.Split(TOKENS_SEPORATOR, StringSplitOptions.RemoveEmptyEntries);

        var hours = int.Parse(durArr.First());
        var minutes = int.Parse(durArr[1]);
        
        return new TimeSpan(hours, minutes, 0);

    }

    public static async Task WriteFileToDirectory(this IFormFile file)
    {
        var readStream = file.OpenReadStream();
        var memstream = new MemoryStream();
        await readStream.CopyToAsync(memstream);
        await File.WriteAllBytesAsync(Path.Combine("cyber-boom-files", file.FileName), memstream.ToArray());
    }

    public static async Task WriteFilesToDirectory(this IEnumerable<IFormFile> files)
    {   
        foreach(var file in files)
        {
            await file.WriteFileToDirectory();
        }
        
    }

    public static async Task<StatsData> GetStatistic(this ApplicationContext applicationContext, string id)
    {
        var specialities = await applicationContext.UserWriteToMetings.Where(c => c.UserId == id)
            .Join(applicationContext.Meetings, 
                m => m.MeetingId, 
                m => m.Id,
                (c,m) => new {
                    m.Tags,
                    m.Id,
                    m.Duration,
                    m.Time
                }
            ).Where(t => DateTime.UtcNow > t.Time).ToArrayAsync();

        var selectedSpecialities = specialities.Select(s => new {
            s.Id,
            Tags = s.Tags.Split(TOKENS_SEPORATOR, StringSplitOptions.RemoveEmptyEntries),
            Duration = s.Duration.ParseDuration().TotalHours
        });

        var allTags = selectedSpecialities.SelectMany(s => s.Tags).Distinct();
        var count = selectedSpecialities.Count();
        
        StatsData stats = new StatsData{
            Count = count,
            Hours = selectedSpecialities.Sum(m => m.Duration) * count
             
        };
        foreach(var tag in allTags)
        {
            //StatsData.TagStats
            var specByTag = selectedSpecialities.Where(f => f.Tags.Contains(tag));
            var countByTag = specByTag.Count();
            var hours = selectedSpecialities.Sum(s => s.Duration) * countByTag;

            var stat = new StatsData.TagStats
            {
                Count = countByTag,
                Tag = tag,
                Hours = hours
            };
            stats.StatsByTag.Add(stat);
        }

       
        
        return stats;
    }
}
