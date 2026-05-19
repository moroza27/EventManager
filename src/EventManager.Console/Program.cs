using EventManager.Application.Common;
using EventManager.Application.Services;
using EventManager.Domain.Entities;
using EventManager.Domain.Enums;
using EventManager.Domain.Interfaces;
using EventManager.Infrastructure.Repositories;

namespace EventManager.ConsoleApp;

public class InMemoryRegistrationRepository : IRegistrationRepository
{
    private readonly List<Registration> _registrations = new();
    public void Add(Registration registration) => _registrations.Add(registration);
    public List<Registration> GetAll() => _registrations;
}

class Program
{
    static async Task Main(string[] args)
    {
        var eventRepository = new JsonEventRepository();
        var registrationRepository = new InMemoryRegistrationRepository();
        var observer = new EmailNotificationObserver();
        
        var service = new EventService(eventRepository, registrationRepository, observer);
        await service.LoadDataAsync();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Loaded {service.GetAllEvents().Count} event(s) from persistent storage.");
        Console.ResetColor();

        bool running = true;
        while (running)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== EVENT MANAGER MENU ===");
            Console.ResetColor();
            Console.WriteLine("1. Create New Event");
            Console.WriteLine("2. Register Participant to Event");
            Console.WriteLine("3. Cancel Event (Trigger Observer)");
            Console.WriteLine("4. View All Events & Statuses");
            Console.WriteLine("5. Open Analytics & LINQ Dashboard");
            Console.WriteLine("6. Save & Exit");
            Console.Write("\nChoose an option: ");

            string? choice = Console.ReadLine();
            Console.WriteLine();

            // Визначаємо перше доступне значення з твого енуму EventCategory динамічно,
            // щоб не закладатися на конкретні назви на кшталт Tech чи Business
            var defaultCategory = Enum.GetValues<EventCategory>().Cast<EventCategory>().First();

            switch (choice)
            {
                case "1":
                    Console.Write("Enter event title: ");
                    string title = Console.ReadLine() ?? "";
                    Console.Write("Enter description: ");
                    string desc = Console.ReadLine() ?? "";
                    Console.Write("Enter days from today: ");
                    if (!int.TryParse(Console.ReadLine(), out int days)) days = 1;
                    Console.Write("Enter capacity: ");
                    if (!int.TryParse(Console.ReadLine(), out int cap)) cap = 10;

                    var createResult = service.CreateEvent(
                        title, desc, DateTime.Now.AddDays(days), cap,
                        "Main Hall", "Stepana Bandery St, 12", 
                        "Alex Green", "alex@events.com", 
                        defaultCategory
                    );
                    
                    if (createResult.IsSuccess)
                        Console.WriteLine("Success: Event created successfully!");
                    else
                        Console.WriteLine($"Error: {createResult.Error}");
                    break;

                case "2":
                    ShowShortEventsList(service.GetAllEvents());
                    Console.Write("Enter Event ID: ");
                    if (Guid.TryParse(Console.ReadLine(), out Guid eId))
                    {
                        Console.Write("Enter Name: ");
                        string pName = Console.ReadLine() ?? "Guest";
                        Console.Write("Enter Email: ");
                        string pEmail = Console.ReadLine() ?? "guest@test.com";
                        
                        var regResult = service.RegisterParticipant(eId, pName, pEmail);
                        PrintResult(regResult, "Participant registered successfully!");
                    }
                    break;

                case "3":
                    ShowShortEventsList(service.GetAllEvents());
                    Console.Write("Enter Event ID to CANCEL: ");
                    if (Guid.TryParse(Console.ReadLine(), out Guid cancelId))
                    {
                        var cancelResult = service.CancelEvent(cancelId);
                        PrintResult(cancelResult, "Event has been cancelled!");
                    }
                    break;

                case "4":
                    var list = service.GetAllEvents();
                    if (!list.Any()) Console.WriteLine("No events found.");
                    foreach (var ev in list)
                    {
                        Console.WriteLine($"[{ev.Status}] {ev.Title} (ID: {ev.Id}) - Category: {ev.Category}, Registered: {ev.Registrations.Count}/{ev.Capacity}");
                    }
                    break;

                case "5":
                    OpenAnalyticsDashboard(service);
                    break;

                case "6":
                    await service.SaveDataAsync();
                    Console.WriteLine("State saved to persistent storage.");
                    running = false;
                    break;

                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }
            Console.WriteLine("\nPress any key...");
            Console.ReadKey();
            Console.Clear();
        }
    }

    static void OpenAnalyticsDashboard(EventService service)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("=== LINQ ANALYTICS DASHBOARD ===");
        Console.ResetColor();
        
        var available = service.GetAvailableEvents();
        Console.WriteLine($"\n[1. Available Events]: {available.Count}");

        var top = service.GetTopPopularEvents(3);
        Console.WriteLine($"[2. Top Popular Events]: {top.Count}");

        var totalCapacity = service.GetTotalCapacityOfOpenEvents();
        Console.WriteLine($"[3. Global Capacity of Open Events]: {totalCapacity}");
    }

    static void ShowShortEventsList(List<Event> events)
    {
        foreach (var e in events) Console.WriteLine($"ID: {e.Id} | Title: {e.Title} [{e.Status}]");
    }

    static void PrintResult(Result result, string msg)
    {
        Console.WriteLine(result.IsSuccess ? $"Success: {msg}" : $"Error: {result.Error}");
    }
}