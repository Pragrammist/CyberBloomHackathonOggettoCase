using Microsoft.AspNetCore.Identity;


public class User : IdentityUser
{
    public string AvatarUrl { get; set; } = null!;

    public string Fio { get; set; } = null!;

    public string Specialities { get; set; } = null!;

    public string TelegramBotUrl { get; set; } = null!;

    public int Level { get; set; }


}
