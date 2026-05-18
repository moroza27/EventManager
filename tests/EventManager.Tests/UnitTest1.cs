using EventManager.Application.Services;
using EventManager.Domain.Enums;
using EventManager.Infrastructure.Repositories;

namespace EventManager.Tests;

public class EventServiceTests
{
    private readonly EventService _service;
    private readonly InMemoryEventRepository _eventRepo;

    public EventServiceTests()
    {
        _eventRepo = new InMemoryEventRepository();
        var regRepo = new InMemoryRegistrationRepository();
        _service = new EventService(_eventRepo, regRepo);
    }

    [Fact]
    public void CreateEvent_With_Invalid_Data_Should_Return_Failure_Result()
    {
        var result = _service.CreateEvent(
            "", "Desc", DateTime.Now.AddDays(1), 10, 
            "Hall", "Rivne", "Admin", "admin@gmail.com", EventCategory.Conference);

        Assert.True(result.IsFailure);
        Assert.Equal("Event title cannot be empty.", result.Error);
    }

    [Fact]
    public void RegisterParticipant_To_NonExistent_Event_Should_Return_Failure()
    {
        var result = _service.RegisterParticipant(Guid.NewGuid(), "Alex", "alex@gmail.com");

        Assert.True(result.IsFailure);
        Assert.Equal("Event not found.", result.Error);
    }

    [Fact]
    public void RegisterParticipant_With_Invalid_Email_Should_Return_Failure()
    {
        var ev = _service.CreateEvent(
            "Fest", "Desc", DateTime.Now.AddDays(1), 10, 
            "Hall", "Rivne", "Admin", "admin@gmail.com", EventCategory.Concert).Value!;

        var result = _service.RegisterParticipant(ev.Id, "Alex", "invalid_email_format");

        Assert.True(result.IsFailure);
        Assert.Contains("Invalid email format.", result.Error);
    }

    [Fact]
    public void RegisterParticipant_When_No_Places_Left_Should_Return_Failure()
    {
        var ev = _service.CreateEvent(
            "Workshop", "Desc", DateTime.Now.AddDays(1), 1, 
            "Hall", "Rivne", "Admin", "admin@gmail.com", EventCategory.Workshop).Value!;

        _service.RegisterParticipant(ev.Id, "User 1", "user1@gmail.com");

        var result = _service.RegisterParticipant(ev.Id, "User 2", "user2@gmail.com");

        Assert.True(result.IsFailure);
        Assert.Equal("No available places.", result.Error);
    }

    [Fact]
    public void RegisterParticipant_Duplicate_Should_Return_Failure()
    {
        var ev = _service.CreateEvent(
            "Meetup", "Desc", DateTime.Now.AddDays(1), 5, 
            "Hall", "Rivne", "Admin", "admin@gmail.com", EventCategory.Meetup).Value!;

        var result1 = _service.RegisterParticipant(ev.Id, "Danylo", "danylo@gmail.com");
        
        Assert.True(result1.IsSuccess);
    }
}