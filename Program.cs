using Microsoft.OpenApi.Models;
using System.Collections.Immutable;
using System.Text.Json;

//
// Initialize the builder for the server, which binds the URL that we will
// will use for images
//
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Api", Version = "v1" });
});

//
// Look for static files in the webroot folder
//
builder.WebHost.UseWebRoot("webroot");

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Api v1"));

//
// Load the data from json
//
using var pokemonStream = typeof(Program).Assembly.GetManifestResourceStream("dotnet_pokemon_json_api.pokemon.json")
    ?? throw new InvalidOperationException("Cannot open pokemon json file");

var serializerOptions = new JsonSerializerOptions()
{
    PropertyNameCaseInsensitive = true
};

var rawPokemans = await JsonSerializer.DeserializeAsync<PokeDexEntry[]>(pokemonStream, serializerOptions)
    ?? throw new InvalidOperationException("Could not parse json file");

// TODO: How do you find a bound url to actually use here? 
// I guess it would be settings...
var pokemans = FixImageLinks(rawPokemans, "~/");

//
// Routes
//
app.MapGet("/api/pokedex", (int? pageOpt, int? perPageOpt) => Paginate(pokemans, pageOpt ?? 1, perPageOpt ?? 20));
app.MapGet("/api/pokedex/all", () => pokemans);
app.MapGet("/api/pokedex/{name}", (string name) => pokemans.FirstOrDefault(el => CompareName(el, name)));

app.MapGet("/api/search", (string query, int? pageOpt, int? perPageOpt) =>
{
    var matching = pokemans.Where(el => ContainsName(el, query));

    return Paginate(matching, pageOpt ?? 1, perPageOpt ?? 20);
});
app.Run();

//
// Local Functions
//

static IEnumerable<T> Paginate<T>(IEnumerable<T> entries, int page, int perPage)
{
    var showFrom = perPage * (page - 1);
    var showTo = showFrom + perPage;

    return entries.Take(showFrom..showTo);
}

static bool CompareName(PokeDexEntry entry, string name) => string.Equals(entry.Name, name, StringComparison.OrdinalIgnoreCase);
static bool ContainsName(PokeDexEntry entry, string name) => entry.Name.Contains(name, StringComparison.OrdinalIgnoreCase);

static ImmutableArray<PokeDexEntry> FixImageLinks(IEnumerable<PokeDexEntry> currentEntries, string baseUrl)
{
    var builder = ImmutableArray.CreateBuilder<PokeDexEntry>();
    foreach (var entry in currentEntries)
    {
        if (entry.Variations.Any())
        {
            var newVariations = new List<Variation>();
            foreach (var variation in entry.Variations)
            {
                newVariations.Add(variation with
                {
                    Image = baseUrl + variation.Image
                });
            }

            builder.Add(entry with
            {
                Variations = newVariations.ToArray()
            });
        }
        else
        {
            builder.Add(entry);
        }
    }

    return builder.ToImmutableArray();
}