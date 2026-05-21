using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EventManager.Application.Services;
using EventManager.Domain.Entities;
using EventManager.Domain.Enums;
using EventManager.Domain.Exceptions;
using EventManager.Domain.Interfaces;
using EventManager.Infrastructure.Persistence;
using EventManager.Infrastructure.Repositories;
using EventManager.Tests.TestData;
using FluentAssertions;
using Moq;
using Xunit;

namespace EventManager.Tests.Additional
{
    public class AdditionalTests
    {
        [Fact]
        public void Event_Creation_WithValidData_SetsProperties()
        {
            var venue = new Venue("Hall", "City", 100);
            var org = new Organizer("Org", "org@test.com");

            var ev = new Event("Title", "Desc", DateTime.UtcNow.AddDays(2), 50, venue, org, EventCategory.Conference);

            ev.Title.Should().Be("Title");
            ev.Capacity.Should().Be(50);
            ev.Status.Should().Be(EventStatus.Open);
            ev.Category.Should().Be(EventCategory.Conference);
        }

        [Fact]
        public void Event_HasAvailablePlaces_InitiallyTrue()
        {
            var ev = TestDataFactory.CreateEvent();

            ev.HasAvailablePlaces().Should().BeTrue();
        }

        [Fact]
        public void Event_CancelEvent_CannotCancelTwice()
        {
            var ev = TestDataFactory.CreateEvent();

            ev.CancelEvent();

            Action act = () => ev.CancelEvent();
            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void CancelEvent_ShouldNotifyObservers()
        {
            var observerMock = new Mock<IEventObserver>();
            var ev = TestDataFactory.CreateEvent();
            ev.AttachObserver(observerMock.Object);

            ev.CancelEvent();

            observerMock.Verify(x => x.OnEventCancelled(ev), Times.Once);
        }

        [Fact]
        public void Registration_RegistrationDate_IsSet()
        {
            var reg = new Registration(Guid.NewGuid(), Guid.NewGuid());

            reg.RegistrationDate.Should().NotBe(default(DateTime));
            (DateTime.UtcNow - reg.RegistrationDate.ToUniversalTime()).Should().BeLessThan(TimeSpan.FromSeconds(10));
        }

        [Fact]
        public async Task InMemoryRepository_AsyncWrappers_Work()
        {
            var repo = new InMemoryEventRepository();
            var ev = TestDataFactory.CreateEvent();

            await repo.AddAsync(ev);

            var all = await repo.GetAllAsync();
            all.Should().ContainSingle().Which.Title.Should().Be(ev.Title);

            var byId = await repo.GetByIdAsync(ev.Id);
            byId.Should().NotBeNull();
            byId!.Id.Should().Be(ev.Id);
        }

        [Fact]
        public async Task JsonEventRepository_SaveLoad_PreservesEvent()
        {
            var temp = Path.GetTempFileName();
            try
            {
                var repo = new JsonEventRepository(temp);
                var ev = TestDataFactory.CreateEvent();
                await repo.AddAsync(ev);
                await repo.SaveAsync();

                var repo2 = new JsonEventRepository(temp);
                await repo2.LoadAsync();
                var loaded = await repo2.GetAllAsync();

                loaded.Should().ContainSingle().Which.Title.Should().Be(ev.Title);
            }
            finally
            {
                File.Delete(temp);
            }
        }

        [Fact]
        public async Task EventService_CreateEventAsync_AddsEventToRepository()
        {
            var repo = new InMemoryEventRepository();
            var service = new EventService(repo);

            var ev = TestDataFactory.CreateEvent();

            var res = await service.CreateEventAsync(ev);
            res.IsSuccess.Should().BeTrue();

            var all = await repo.GetAllAsync();
            all.Should().ContainSingle().Which.Id.Should().Be(ev.Id);
        }

        [Fact]
        public async Task EventService_GetEventCountByCategory_ReturnsCounts()
        {
            var repo = new InMemoryEventRepository();
            var service = new EventService(repo);

            var conference = TestDataFactory.CreateEvent();
            var workshop = new Event(
                "Workshop",
                "Desc",
                DateTime.UtcNow.AddDays(3),
                100,
                new Venue("Hall 2", "City", 100),
                new Organizer("Org", "org@test.com"),
                EventCategory.Workshop);

            await repo.AddAsync(conference);
            await repo.AddAsync(workshop);

            var counts = service.GetEventCountByCategory();

            counts[EventCategory.Conference].Should().Be(1);
            counts[EventCategory.Workshop].Should().Be(1);
        }

        [Fact]
        public async Task EventService_RegisterParticipantAsync_AddsRegistrationToEvent()
        {
            var repo = new InMemoryEventRepository();
            var service = new EventService(repo);

            var ev = TestDataFactory.CreateEvent();
            await repo.AddAsync(ev);

            var participant = new Participant("P", "p@test.com");
            var res = await service.RegisterParticipantAsync(ev.Id, participant);

            res.IsSuccess.Should().BeTrue();
            var loaded = await repo.GetByIdAsync(ev.Id);
            loaded.Should().NotBeNull();
            loaded!.Registrations.Should().HaveCount(1);
        }

        [Fact]
        public async Task EventService_CreateEvent_ReturnsFailure_WhenRepositoryFails()
        {
            var repositoryMock = new Mock<IEventRepository>();
            repositoryMock.Setup(x => x.AddAsync(It.IsAny<Event>(), It.IsAny<System.Threading.CancellationToken>()))
                .ThrowsAsync(new IOException("Disk error"));

            var service = new EventService(repositoryMock.Object);
            var ev = TestDataFactory.CreateEvent();

            var res = await service.CreateEventAsync(ev);

            res.IsSuccess.Should().BeFalse();
            res.Error.Should().Contain("Disk error");
        }

        [Fact]
        public async Task JsonDataStore_SaveAsync_ReturnsFailure_OnIOException()
        {
            var temp = Path.GetTempFileName();
            try
            {
                File.SetAttributes(temp, FileAttributes.ReadOnly);
                var store = new JsonDataStore<string>(temp);
                var result = await store.SaveAsync(new List<string> { "Test" });

                result.IsSuccess.Should().BeFalse();
                result.Error.Should().Contain("Unable to save file");
            }
            finally
            {
                File.SetAttributes(temp, FileAttributes.Normal);
                File.Delete(temp);
            }
        }
    }
}
