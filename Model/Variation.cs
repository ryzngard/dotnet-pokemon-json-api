namespace dotnet_pokemon_json_api.Model
{
    public record Variation(
        string Name,
        string Descriptions,
        string Image,
        string[] Types,
        string Specie,
        double Height,
        double Weight,
        string[] Abilities,
        Stats Stats,
        string[] Evolutions);
}
