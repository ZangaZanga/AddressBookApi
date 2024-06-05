using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AddressBookApi.Models
{
public class Adresse
{
    public int Id { get; set; }
    public string Straße { get; set; } = string.Empty;
    public string Stadt { get; set; } = string.Empty;
    public string Bundesland { get; set; } = string.Empty;
    public string PLZ { get; set; } = string.Empty;
    public int PersonId { get; set; }
    [JsonIgnore]
    [ValidateNever]
    public Person Person { get; set; } = null!;
}

}