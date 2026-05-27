using System;
using EventManager.Domain.Entities;

namespace EventManager.Tests.TestData;

public static class TestDataFactory
{
    public static Event CreateEvent()
    {
        var venue = new Venue(
            "Conference Hall",
            "Lviv",
            100);

        var organizer = new Organizer(
            "Admin",
            "admin@gmail.com");

        return new Event(
            "Conference",
            "Description",
            DateTime.UtcNow.AddDays(5),
            50,
            venue,
            organizer,
            EventManager.Domain.Enums.EventCategory.Conference);
    }
}