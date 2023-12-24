using System.ComponentModel.DataAnnotations.Schema;

public class Question
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; } = null!;

    public string Text { get; set; } = null!;

    public string MeetingId { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public User User { get; set; } = null!;
}
