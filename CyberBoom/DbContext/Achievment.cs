using System.ComponentModel.DataAnnotations.Schema;

public class Achievment
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Text { get; set; } = null!;
}
