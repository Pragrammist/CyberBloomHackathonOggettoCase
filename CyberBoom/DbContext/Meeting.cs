using System.ComponentModel.DataAnnotations.Schema;

public class Meeting
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public String Id { get; set; } = null!;

    public DateTime Time { get; set; }

    public string Title { get; set; } = null!;

    public string Theme { get; set; } = null!;

    public string SpeakerName { get; set; } = null!;

    public string SpeackerImage { get; set; } = null!;

    public string Splecializations { get; set; } = null!; //speacker specilization

    
    public string Type { get; set; } = "онлайн/офлайн";

    public string SpeakerTelephone { get; set; } = null!;


    public string PlaceImages { get; set; } = null!;

    public string SpeakerEmail { get; set; } = null!;

    public string Tags { get; set; } = null!;

    public string Urls { get; set; } = null!;

    public string PlaceAdress { get; set; } = null!;

    public string Duration { get; set; } = null!;
}   
