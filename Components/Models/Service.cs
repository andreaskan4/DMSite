using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization; // Απαραίτητο

namespace DMsite.Models;

public class Service
{
    // ΑΥΤΟ ΕΛΕΙΠΕ: Λέει στη C# "αν το Id είναι 0, μην το στείλεις καθόλου στη βάση"
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public long Id { get; set; }

    [Required(ErrorMessage = "Το όνομα είναι υποχρεωτικό.")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Η τιμή είναι υποχρεωτική.")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Η διάρκεια είναι υποχρεωτική.")]
    public string Duration { get; set; } = "";

    public string Description { get; set; } = "";
}