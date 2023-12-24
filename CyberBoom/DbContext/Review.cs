using System.ComponentModel.DataAnnotations.Schema;

public class Review
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; } = null!;

    public string MeetingId { get; set; } = null!;
    
    public string UserId { get; set; } = null!;

    public string Text { get; set; } = null!;

    public int Score { get; set; } = 0;

    public User User { get; set; } = null!;

    DateTime _date;
    
    public DateTime Date { get => _date; set => _date = value.ToUniversalTime(); }
}
