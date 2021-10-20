namespace dotnet_pokemon_json_api.Model
{
    public record PokeDexEntry(
        int Num,
        string Name,
        Variation[] Variations,
        string Link);
}
