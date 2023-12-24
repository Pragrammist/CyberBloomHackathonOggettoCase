public class StatsData
{
    public double Hours{ get; set; }

    public int Count { get; set; }

    public List<TagStats> StatsByTag { get; set; } = new List<TagStats>();

    public class TagStats
    {
        public string Tag { get; set; } = null!;

        public int Count { get; set; }

        public double Hours { get; set; }
    }
}
