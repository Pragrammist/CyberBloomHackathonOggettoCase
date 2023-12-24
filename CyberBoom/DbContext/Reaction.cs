using System.ComponentModel.DataAnnotations.Schema;

public class Reaction
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; } = null!;

    public string QuestionId { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public bool IsLike { get; set; } = true;
}
