namespace AddressBookApi.Models
{
public class Person
{
    public int Id { get; set; }
    public string Vorname { get; set; } = string.Empty;
    public string Nachname { get; set; } = string.Empty;
    public List<Adresse> Adressen { get; set; } = new();
}
}