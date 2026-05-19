using System.IO;
using System.Text.Json;
using EventManager.Domain.Entities;
using EventManager.Domain.Interfaces;

namespace EventManager.Infrastructure.Repositories;

public class JsonEventRepository : IEventRepository
{
    private readonly string _filePath;
    private List<Event> _events = new();
    private readonly JsonSerializerOptions _options;

    public JsonEventRepository(string filePath = "events_storage.json")
    {
        _filePath = Path.IsPathRooted(filePath)
            ? filePath
            : Path.Combine(AppContext.BaseDirectory, filePath);

        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    public void Add(Event eventItem) => _events.Add(eventItem);
    public Event? GetById(Guid id) => _events.FirstOrDefault(e => e.Id == id);
    public List<Event> GetAll() => _events;

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath) ?? AppContext.BaseDirectory);
            string json = JsonSerializer.Serialize(_events, _options);
            await File.WriteAllTextAsync(_filePath, json, cancellationToken);
        }
        catch (IOException ex)
        {
            Console.WriteLine($"[Persistence Error] Storage file is locked or inaccessible: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Persistence Error] Critical error during saving: {ex.Message}");
        }
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            _events = new List<Event>();
            return;
        }

        try
        {
            var jsonContent = await File.ReadAllTextAsync(_filePath, cancellationToken);
            var loadedEvents = JsonSerializer.Deserialize<List<Event>>(jsonContent, _options);

            if (loadedEvents != null)
            {
                _events = loadedEvents
                    .DistinctBy(e => e.Id)
                    .ToList();
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"[Persistence Warning] Storage file is corrupted: {ex.Message}");
            Console.WriteLine("[Persistence] Starting with an empty state to prevent crash.");
            _events = new List<Event>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Persistence Error] Could not load data: {ex.Message}");
            _events = new List<Event>();
        }
    }

    private static List<Event> NormalizeLoadedEvents(List<Event> loadedEvents)
    {
        var uniqueEvents = new List<Event>();
        var seenIds = new HashSet<Guid>();

        Console.WriteLine($"[DEBUG] NormalizeLoadedEvents: input count = {loadedEvents.Count}");

        foreach (var ev in loadedEvents)
        {
            if (ev is null)
            {
                continue;
            }

            if (!seenIds.Add(ev.Id))
            {
                Console.WriteLine($"[Persistence Warning] Duplicate event id detected: {ev.Id}. Duplicate instance ignored.");
                continue;
            }

            uniqueEvents.Add(ev);
        }

        Console.WriteLine($"[DEBUG] NormalizeLoadedEvents: output count = {uniqueEvents.Count}");
        return uniqueEvents;
    }
}