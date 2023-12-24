using System.ComponentModel.DataAnnotations.Schema;

public class UserWriteToMeting
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public String Id { get; set; } = null!;

    public String UserId { get; set; } = null!;

    public String MeetingId { get; set; } = null!;
}
