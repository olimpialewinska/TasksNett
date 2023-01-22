using System.ComponentModel.DataAnnotations;

namespace Tasks.Models;

public class Friend
{
    public int Id { get; set; }

    [Required]
    [EmailAddress]
    public string User1 { get; set; }

    [Required]
    [EmailAddress]
    public string User2 { get; set; }

}

