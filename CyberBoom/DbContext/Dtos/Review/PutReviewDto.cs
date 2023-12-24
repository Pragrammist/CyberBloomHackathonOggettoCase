public class PutReviewDto
{
    public string Id { get; set; } = null!;
    
    public string Text { get; set; } = null!;

    public int Score { get; set; } = 0;

    DateTime _date;
    
    public DateTime Date { get => _date; set => _date = value.ToUniversalTime(); }
}
