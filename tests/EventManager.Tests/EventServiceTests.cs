using Xunit;
using EventManager.Domain.Entities;
using EventManager.Domain.Enums;
using EventManager.Domain.Interfaces;
using EventManager.Application.Services;
using EventManager.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace EventManager.Tests;

public class EventServiceTests
{
    private readonly Venue _fakeVenue = new("Test Venue", "Test Address", 50);
    private readonly Organizer _fakeOrganizer = new("Test Organizer", "test@test.com");
    private readonly FakeObserver _fakeObserver = new();
    
    private readonly EventCategory _cat = Enum.GetValues<EventCategory>().Cast<EventCategory>().First();

    private class InMemoryEventRepository : IEventRepository
    {
        public List<Event> Events { get; set; } = new();
        public void Add(Event eventItem) => Events.Add(eventItem);
        public Event? GetById(Guid id) => Events.Find(e => e.Id == id);
        public List<Event> GetAll() => Events;
        public Task SaveAsync(CancellationToken c = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken c = default) => Task.CompletedTask;
    }

    private class InMemoryRegistrationRepository : IRegistrationRepository
    {
        public List<Registration> Registrations { get; set; } = new();
        public void Add(Registration r) => Registrations.Add(r);
        public List<Registration> GetAll() => Registrations;
    }

    private class FakeObserver : IEventObserver
    {
        public bool WasNotified { get; private set; }
        public void OnEventCancelled(Event cancelledEvent) => WasNotified = true;
    }


    [Fact]
    public void Event_Constructor_ShouldThrowException_WhenTitleIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => 
            new Event("", "Desc", DateTime.Now.AddDays(1), 10, _fakeVenue, _fakeOrganizer, _cat));
    }

    [Fact] // 2
    public void Event_Constructor_ShouldThrowException_WhenDateIsInThePast()
    {
        Assert.Throws<ArgumentException>(() => 
            new Event("Title", "Desc", DateTime.Now.AddDays(-5), 10, _fakeVenue, _fakeOrganizer, _cat));
    }

    [Fact]
    public void Event_Constructor_ShouldThrowException_WhenCapacityIsZeroOrLess()
    {
        Assert.Throws<ArgumentException>(() => 
            new Event("Title", "Desc", DateTime.Now.AddDays(1), 0, _fakeVenue, _fakeOrganizer, _cat));
    }


    [Fact]
    public void AddRegistration_ShouldThrowException_WhenEventIsCancelled()
    {
        var ev = new Event("Title", "Desc", DateTime.Now.AddDays(1), 10, _fakeVenue, _fakeOrganizer, _cat);
        ev.CancelEvent();
        var reg = new Registration(ev.Id, Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() => ev.AddRegistration(reg));
    }

    [Fact]
    public void AddRegistration_ShouldThrowException_WhenEventIsFull()
    {
        var ev = new Event("Title", "Desc", DateTime.Now.AddDays(1), 1, _fakeVenue, _fakeOrganizer, _cat);
        ev.AddRegistration(new Registration(ev.Id, Guid.NewGuid()));

        Assert.Throws<InvalidOperationException>(() => ev.AddRegistration(new Registration(ev.Id, Guid.NewGuid())));
    }

    [Fact]
    public void AddRegistration_ShouldAddSuccessfully_WhenRulesAreMet()
    {
        var ev = new Event("Title", "Desc", DateTime.Now.AddDays(1), 10, _fakeVenue, _fakeOrganizer, _cat);
        ev.AddRegistration(new Registration(ev.Id, Guid.NewGuid()));

        Assert.Single(ev.Registrations); 
    }


    [Fact]
    public void CancelEvent_ShouldChangeStatusToCancelled()
    {
        var ev = new Event("Title", "Desc", DateTime.Now.AddDays(1), 10, _fakeVenue, _fakeOrganizer, _cat);
        ev.CancelEvent();

        Assert.Equal(EventStatus.Cancelled, ev.Status);
    }

    [Fact]
    public void CancelEvent_ShouldNotifyAttachedObservers()
    {
        var ev = new Event("Title", "Desc", DateTime.Now.AddDays(1), 10, _fakeVenue, _fakeOrganizer, _cat);
        ev.AttachObserver(_fakeObserver);

        ev.CancelEvent();

        Assert.True(_fakeObserver.WasNotified);
    }

    [Fact]
    public void CancelEvent_ShouldThrowException_WhenEventIsAlreadyCancelled()
    {
        var ev = new Event("Title", "Desc", DateTime.Now.AddDays(1), 10, _fakeVenue, _fakeOrganizer, _cat);
        ev.CancelEvent();

        Assert.Throws<InvalidOperationException>(() => ev.CancelEvent());
    }


    [Fact]
    public void EventService_CreateEvent_ShouldReturnSuccess()
    {
        var repo = new InMemoryEventRepository();
        var regRepo = new InMemoryRegistrationRepository();
        var service = new EventService(repo, regRepo, _fakeObserver);

        var result = service.CreateEvent("Title", "Desc", DateTime.Now.AddDays(1), 10, 
            "Venue", "Address", "Organizer", "organizer@test.com", _cat);

        Assert.True(result.IsSuccess);
        Assert.Single(repo.GetAll());
    }

    [Fact]
    public void EventService_CancelEvent_ShouldReturnFailure_WhenEventDoesNotExist()
    {
        var repo = new InMemoryEventRepository();
        var service = new EventService(repo, new InMemoryRegistrationRepository(), _fakeObserver);

        var result = service.CancelEvent(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal("Event not found.", result.Error);
    }


    [Fact]
    public void LINQ_GetAvailableEvents_ShouldFilterOutCancelledAndFullEvents()
    {
        var repo = new InMemoryEventRepository();
        var service = new EventService(repo, new InMemoryRegistrationRepository(), _fakeObserver);

        var openEvent = new Event("Open", "Desc", DateTime.Now.AddDays(1), 10, _fakeVenue, _fakeOrganizer, _cat);
        var cancelledEvent = new Event("Cancelled", "Desc", DateTime.Now.AddDays(1), 10, _fakeVenue, _fakeOrganizer, _cat);
        cancelledEvent.CancelEvent();

        var fullEvent = new Event("Full", "Desc", DateTime.Now.AddDays(1), 1, _fakeVenue, _fakeOrganizer, _cat);
        fullEvent.AddRegistration(new Registration(fullEvent.Id, Guid.NewGuid()));

        repo.Add(openEvent);
        repo.Add(cancelledEvent);
        repo.Add(fullEvent);

        var available = service.GetAvailableEvents();

        Assert.Single(available);
        Assert.Equal("Open", available[0].Title);
    }

    [Fact]
    public void LINQ_GetTopPopularEvents_ShouldOrderDescendingByRegistrations()
    {
        var repo = new InMemoryEventRepository();
        var service = new EventService(repo, new InMemoryRegistrationRepository(), _fakeObserver);

        var popularEvent = new Event("Popular", "Desc", DateTime.Now.AddDays(1), 10, _fakeVenue, _fakeOrganizer, _cat);
        popularEvent.AddRegistration(new Registration(popularEvent.Id, Guid.NewGuid()));
        popularEvent.AddRegistration(new Registration(popularEvent.Id, Guid.NewGuid()));

        var normalEvent = new Event("Normal", "Desc", DateTime.Now.AddDays(1), 10, _fakeVenue, _fakeOrganizer, _cat);
        normalEvent.AddRegistration(new Registration(normalEvent.Id, Guid.NewGuid()));

        repo.Add(normalEvent);
        repo.Add(popularEvent);

        var top = service.GetTopPopularEvents(2);

        Assert.Equal("Popular", top[0].Title);
        Assert.Equal("Normal", top[1].Title);
    }

    [Fact]
    public void LINQ_GetEventCountByCategory_ShouldGroupAccurately()
    {
        var repo = new InMemoryEventRepository();
        var service = new EventService(repo, new InMemoryRegistrationRepository(), _fakeObserver);

        repo.Add(new Event("E1", "Desc", DateTime.Now.AddDays(1), 10, _fakeVenue, _fakeOrganizer, _cat));
        repo.Add(new Event("E2", "Desc", DateTime.Now.AddDays(1), 10, _fakeVenue, _fakeOrganizer, _cat));

        var groups = service.GetEventCountByCategory();

        Assert.True(groups.ContainsKey(_cat));
        Assert.Equal(2, groups[_cat]);
    }

    [Fact]
    public void LINQ_GetTotalCapacityOfOpenEvents_ShouldSumOnlyOpenEvents()
    {
        var repo = new InMemoryEventRepository();
        var service = new EventService(repo, new InMemoryRegistrationRepository(), _fakeObserver);

        repo.Add(new Event("E1", "Desc", DateTime.Now.AddDays(1), 15, _fakeVenue, _fakeOrganizer, _cat));
        repo.Add(new Event("E2", "Desc", DateTime.Now.AddDays(1), 25, _fakeVenue, _fakeOrganizer, _cat));
        
        var cancelled = new Event("E3", "Desc", DateTime.Now.AddDays(1), 100, _fakeVenue, _fakeOrganizer, _cat);
        cancelled.CancelEvent();
        repo.Add(cancelled);

        int totalCapacity = service.GetTotalCapacityOfOpenEvents();

        Assert.Equal(40, totalCapacity); 
    }

    [Fact]
    public async Task JsonEventRepository_ShouldPersistEventsAcrossSaveAndLoad()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"event_storage_test_{Guid.NewGuid()}.json");
        try
        {
            var repo = new JsonEventRepository(filePath);
            var newEvent = new Event("Saved Event", "Desc", DateTime.Now.AddDays(1), 5, _fakeVenue, _fakeOrganizer, _cat);
            newEvent.AddRegistration(new Registration(newEvent.Id, Guid.NewGuid()));
            repo.Add(newEvent);

            await repo.SaveAsync();

            Console.WriteLine($"DEBUG JSON FILE: {filePath}");
            Console.WriteLine(await File.ReadAllTextAsync(filePath));

            var loader = new JsonEventRepository(filePath);
            await loader.LoadAsync();
            var loaded = loader.GetAll();

            Assert.Single(loaded);
            Assert.Equal(newEvent.Title, loaded[0].Title);
            Assert.Single(loaded[0].Registrations);
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public async Task JsonEventRepository_ShouldReturnEmptyWhenFileMissing()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"event_storage_missing_{Guid.NewGuid()}.json");
        if (File.Exists(filePath)) File.Delete(filePath);

        var repo = new JsonEventRepository(filePath);
        await repo.LoadAsync();

        Assert.Empty(repo.GetAll());
    }

    [Fact]
    public async Task JsonEventRepository_ShouldRecoverFromCorruptJson()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"event_storage_corrupt_{Guid.NewGuid()}.json");
        try
        {
            await File.WriteAllTextAsync(filePath, "{ invalid json content }");
            var repo = new JsonEventRepository(filePath);
            await repo.LoadAsync();

            Assert.Empty(repo.GetAll());
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public async Task JsonEventRepository_ShouldIgnoreDuplicateEventIdsOnLoad()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"event_storage_dup_{Guid.NewGuid()}.json");
        try
        {
            var first = new Event("First", "Desc", DateTime.Now.AddDays(1), 10, _fakeVenue, _fakeOrganizer, _cat);
            var duplicateId = first.Id;

            var jsonContent = $@"[
  {{
    ""Id"": ""{duplicateId}"",
    ""Title"": ""First"",
    ""Description"": ""Desc"",
    ""Date"": ""2026-05-20T00:00:00Z"",
    ""Capacity"": 10,
    ""Venue"": null,
    ""Organizer"": null,
    ""Status"": 1,
    ""Category"": 0,
    ""RegistrationsInternal"": []
  }},
  {{
    ""Id"": ""{duplicateId}"",
    ""Title"": ""FirstDuplicate"",
    ""Description"": ""Desc"",
    ""Date"": ""2026-05-20T00:00:00Z"",
    ""Capacity"": 10,
    ""Venue"": null,
    ""Organizer"": null,
    ""Status"": 1,
    ""Category"": 0,
    ""RegistrationsInternal"": []
  }}
]";
            await File.WriteAllTextAsync(filePath, jsonContent);

            var repo = new JsonEventRepository(filePath);
            await repo.LoadAsync();

            var loaded = repo.GetAll();
            Console.WriteLine($"DEBUG: Loaded {loaded.Count} events");
            foreach (var ev in loaded)
            {
                Console.WriteLine($"  - ID: {ev.Id}, Title: {ev.Title}");
            }

            Assert.Single(loaded);
            Assert.Equal("First", loaded[0].Title);
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }
}
