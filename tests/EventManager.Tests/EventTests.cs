using System;
using EventManager.Domain.Entities;
using EventManager.Domain.Enums;
using EventManager.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace EventManager.Tests.EventTests;

public class EventTests
{
    private readonly Venue _venue;
    private readonly Organizer _organizer;

    public EventTests()
    {
        _venue = new Venue(
            "Conference Hall",
            "Kyiv",
            100);

        _organizer = new Organizer(
            "Admin",
            "admin@gmail.com");
    }

    [Fact]
    public void Event_CannotBeCreated_WithPastDate()
    {
        Action action = () =>
            new Event(
                "Test",
                "Description",
                DateTime.UtcNow.AddDays(-1),
                10,
                _venue,
                _organizer,
                EventCategory.Conference);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Event_CannotExceedCapacity()
    {
        var eventEntity = CreateEvent(1);

        var participant1 = new Participant(
            "User1",
            "user1@gmail.com");

        var participant2 = new Participant(
            "User2",
            "user2@gmail.com");

        var reg1 = new Registration(eventEntity.Id, participant1.Id);
        var reg2 = new Registration(eventEntity.Id, participant2.Id);

        eventEntity.AddRegistration(reg1);

        Action action = () => eventEntity.AddRegistration(reg2);

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void Event_CannotRegister_DuplicateParticipant()
    {
        var eventEntity = CreateEvent(10);

        var participant = new Participant(
            "User",
            "user@gmail.com");

        var reg1 = new Registration(eventEntity.Id, participant.Id);
        var reg2 = new Registration(eventEntity.Id, participant.Id);

        eventEntity.AddRegistration(reg1);

        Action action = () => eventEntity.AddRegistration(reg2);
        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void Event_CancelEvent_ChangesStatus()
    {
        var eventEntity = CreateEvent(10);

        eventEntity.CancelEvent();

        eventEntity.Status.Should().Be(EventStatus.Cancelled);
    }

    [Fact]
    public void Event_CannotRegister_WhenCancelled()
    {
        var eventEntity = CreateEvent(10);

        eventEntity.CancelEvent();

        var participant = new Participant(
            "User",
            "user@gmail.com");

        var reg = new Registration(eventEntity.Id, participant.Id);

        Action action = () => eventEntity.AddRegistration(reg);

        action.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void InvalidCapacity_ShouldThrow(int capacity)
    {
        Action action = () => CreateEvent(capacity);

        action.Should().Throw<ArgumentException>();
    }

    private Event CreateEvent(int capacity)
    {
        return new Event(
            "Conference",
            "Description",
            DateTime.UtcNow.AddDays(5),
            capacity,
            _venue,
            _organizer,
            EventCategory.Conference);
    }
}
