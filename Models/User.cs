using System.Text.Json.Serialization;

public record UserInDB(int Id, string Name, string Email);
public record UserRequest(
    [property: JsonRequired] string Name,
    [property: JsonRequired] string Email,
    int? Id = null);
