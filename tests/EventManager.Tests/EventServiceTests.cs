using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventManager.Domain.Interfaces;
using EventManager.Application.Services;
using EventManager.Domain.Entities;
using EventManager.Tests.TestData;
using FluentAssertions;
using Moq;
using Xunit;

namespace EventManager.Tests.EventServiceTests;

public class EventServiceTests
{
    private readonly Mock<IEventRepository> _repositoryMock;
    private readonly EventService _service;

    public EventServiceTests()
    {
        _repositoryMock = new Mock<IEventRepository>();
        _service = new EventService(_repositoryMock.Object);
    }

    [Fact]
    public async Task CreateEvent_ReturnsSuccess()
    {
        var eventEntity = TestDataFactory.CreateEvent();

        var result = await _service.CreateEventAsync(eventEntity);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task RegisterParticipant_ReturnsFailure_WhenEventMissing()
    {
        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync((Event?)null);

        var participant = new Participant(
            "User",
            "user@gmail.com");

        var result = await _service.RegisterParticipantAsync(
            Guid.NewGuid(),
            participant);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEvents()
    {
        var events = new List<Event>
        {
            TestDataFactory.CreateEvent(),
            TestDataFactory.CreateEvent()
        };

        _repositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(events);

        var result = await _service.GetAllAsync();

        result.Count.Should().Be(2);
    }
}