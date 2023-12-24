public class PostReviewDto
{
    public String MeetingId { get; set; } = null!;
    
    public String UserId { get; set; } = null!;

    public string Text { get; set; } = null!;

    public int Score { get; set; } = 0;

    DateTime _date;
    
    public DateTime Date { get => _date; set => _date = value.ToUniversalTime(); }
}
