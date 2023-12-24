public class UserPut
{
    public string Id { get; set; } = null!;

    public IFormFile Avatar { get; set; } = null!;

    public string Fio { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Specialities { get; set; } = null!;

    public string TelegramBotUrl { get; set; } = null!;
}
