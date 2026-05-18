using EventManager.Application.Services;
using EventManager.Domain.Enums;
using EventManager.Infrastructure.Repositories;

var eventRepository = new InMemoryEventRepository();
var registrationRepository = new InMemoryRegistrationRepository();
var eventService = new EventService(eventRepository, registrationRepository);

bool isRunning = true;

var menuActions = new Dictionary<string, (string Description, Action Action)>
{
    { "1", ("Create event", new Action(() => CreateEvent(eventService))) },
    { "2", ("Register participant", new Action(() => RegisterParticipant(eventService))) },
    { "3", ("Show events", new Action(() => ShowEvents(eventService))) },
    { "0", ("Exit", new Action(() => isRunning = false)) }
};

while (isRunning)
{
    Console.WriteLine("\n=== EventManager ===");
    foreach (var item in menuActions)
    {
        Console.WriteLine($"{item.Key}. {item.Value.Description}");
    }
    Console.Write("Choose option: ");

    string? option = Console.ReadLine();

    if (!string.IsNullOrWhiteSpace(option) && menuActions.ContainsKey(option))
    {
        try
        {
            menuActions[option].Action();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
    else
    {
        Console.WriteLine("Invalid option. Please enter a valid menu item.");
    }
}

static void CreateEvent(EventService eventService)
{
    string title = ReadRequiredString("Title: ");
    string description = ReadRequiredString("Description: ");
    int capacity = ReadPositiveInt("Capacity: ");
    DateTime date = ReadFutureDate("Event date (yyyy-MM-dd): ");
    string venueName = ReadRequiredString("Venue name: ");
    string venueAddress = ReadRequiredString("Venue address: ");
    string organizerName = ReadRequiredString("Organizer name: ");
    string organizerEmail = ReadRequiredString("Organizer email: ");

    var result = eventService.CreateEvent(
        title,
        description,
        date,
        capacity,
        venueName,
        venueAddress,
        organizerName,
        organizerEmail,
        EventCategory.Conference);

    if (result.IsSuccess)
    {
        Console.WriteLine($"Event created: {result.Value!.Title}");
    }
    else
    {
        Console.WriteLine($"Failed to create event: {result.Error}");
    }
}

static void RegisterParticipant(EventService eventService)
{
    var events = eventService.GetAllEvents();

    if (!events.Any())
    {
        Console.WriteLine("No events available.");
        return;
    }

    Console.WriteLine("Available events:");
    foreach (var eventItem in events)
    {
        Console.WriteLine($"{eventItem.Id} | {eventItem.Title} | Date: {eventItem.Date:yyyy-MM-dd} | Registered: {eventItem.Registrations.Count}/{eventItem.Capacity} | Status: {eventItem.Status}");
    }

    Guid eventId = ReadGuid("Enter event id: ");
    string name = ReadRequiredString("Participant name: ");
    string email = ReadRequiredString("Participant email: ");

    var result = eventService.RegisterParticipant(eventId, name, email);

    if (result.IsSuccess)
    {
        Console.WriteLine("Participant registered successfully.");
    }
    else
    {
        Console.WriteLine($"Registration failed: {result.Error}");
    }
}

static void ShowEvents(EventService eventService)
{
    var events = eventService.GetAllEvents();

    if (!events.Any())
    {
        Console.WriteLine("No events found.");
        return;
    }

    foreach (var eventItem in events)
    {
        Console.WriteLine("----------------------");
        Console.WriteLine($"Id: {eventItem.Id}");
        Console.WriteLine($"Title: {eventItem.Title}");
        Console.WriteLine($"Date: {eventItem.Date:yyyy-MM-dd}");
        Console.WriteLine($"Venue: {eventItem.Venue.Name}, {eventItem.Venue.Address}");
        Console.WriteLine($"Organizer: {eventItem.Organizer.FullName} ({eventItem.Organizer.Email})");
        Console.WriteLine($"Capacity: {eventItem.Capacity}");
        Console.WriteLine($"Registered: {eventItem.Registrations.Count}");
        Console.WriteLine($"Available places: {Math.Max(0, eventItem.Capacity - eventItem.Registrations.Count)}");
        Console.WriteLine($"Status: {eventItem.Status}");
        Console.WriteLine($"Category: {eventItem.Category}");
    }
}

static string ReadRequiredString(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string? input = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(input))
        {
            return input.Trim();
        }

        Console.WriteLine("Value cannot be empty. Please try again.");
    }
}

static int ReadPositiveInt(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string? input = Console.ReadLine();
        if (int.TryParse(input, out var value) && value > 0)
        {
            return value;
        }

        Console.WriteLine("Please enter a positive integer.");
    }
}

static DateTime ReadFutureDate(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string? input = Console.ReadLine();
        if (DateTime.TryParse(input, out var date) && date.Date > DateTime.Now.Date)
        {
            return date;
        }

        Console.WriteLine("Please enter a valid future date in the format yyyy-MM-dd.");
    }
}

static Guid ReadGuid(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string? input = Console.ReadLine();
        if (Guid.TryParse(input, out var id))
        {
            return id;
        }

        Console.WriteLine("Please enter a valid GUID.");
    }
}
