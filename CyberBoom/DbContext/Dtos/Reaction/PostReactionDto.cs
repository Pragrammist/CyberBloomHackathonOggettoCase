public class PostReactionDto
{

    public string QuestionId { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public bool IsLike { get; set; } = true;
}
