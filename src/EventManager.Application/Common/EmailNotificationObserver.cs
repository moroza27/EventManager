using EventManager.Domain.Entities;
using EventManager.Domain.Interfaces;

namespace EventManager.Application.Common;

public class EmailNotificationObserver : IEventObserver
{
    public void OnEventCancelled(Event cancelledEvent)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"\n[Observer Notification] ✉️ Sending emails to all {cancelledEvent.Registrations.Count} registered participants...");
        Console.WriteLine($"[Observer Notification] Notification: 'Dear participant, the event \"{cancelledEvent.Title}\" has been CANCELLED by organizer.'");
        Console.ResetColor();
    }
}