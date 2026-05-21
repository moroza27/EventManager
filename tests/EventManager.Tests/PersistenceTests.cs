using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EventManager.Infrastructure.Persistence;
using FluentAssertions;
using Xunit;

namespace EventManager.Tests.Integration;

public class PersistenceTests
{
    [Fact]
    public async Task SaveAndReload_PreservesData()
    {
        var tempFile = Path.GetTempFileName();

        var store = new JsonDataStore<string>(tempFile);

        var data = new List<string>
        {
            "Conference",
            "Workshop"
        };

        var saveResult = await store.SaveAsync(data);

        var loadResult = await store.LoadAsync();

        saveResult.IsSuccess.Should().BeTrue();

        loadResult.IsSuccess.Should().BeTrue();

        loadResult.Value.Should().HaveCount(2);

        loadResult.Value.Should().Contain("Conference");

        loadResult.Value.Should().Contain("Workshop");

        File.Delete(tempFile);
    }

    [Fact]
    public async Task MissingFile_ReturnsEmptyCollection()
    {
        var filePath = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid()}.json");

        var store = new JsonDataStore<string>(filePath);

        var result = await store.LoadAsync();

        result.IsSuccess.Should().BeTrue();

        result.Value.Should().NotBeNull();

        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task CorruptedJson_ReturnsFailure()
    {
        var tempFile = Path.GetTempFileName();

        await File.WriteAllTextAsync(
            tempFile,
            "INVALID JSON CONTENT");

        var store = new JsonDataStore<string>(tempFile);

        var result = await store.LoadAsync();

        result.IsSuccess.Should().BeFalse();

        result.Error.Should().Contain("JSON");

        File.Delete(tempFile);
    }

    [Fact]
    public async Task SequentialSaveOperations_DoNotLoseData()
    {
        var tempFile = Path.GetTempFileName();

        var store = new JsonDataStore<string>(tempFile);

        var firstData = new List<string>
        {
            "Event 1"
        };

        var secondData = new List<string>
        {
            "Event 1",
            "Event 2",
            "Event 3"
        };

        await store.SaveAsync(firstData);

        await store.SaveAsync(secondData);

        var loadResult = await store.LoadAsync();

        loadResult.IsSuccess.Should().BeTrue();

        loadResult.Value.Should().HaveCount(3);

        loadResult.Value.Should().Contain("Event 1");

        loadResult.Value.Should().Contain("Event 2");

        loadResult.Value.Should().Contain("Event 3");

        File.Delete(tempFile);
    }

    [Fact]
    public async Task EmptyFile_ReturnsEmptyCollection()
    {
        var tempFile = Path.GetTempFileName();

        await File.WriteAllTextAsync(tempFile, string.Empty);

        var store = new JsonDataStore<string>(tempFile);

        var result = await store.LoadAsync();

        result.IsSuccess.Should().BeTrue();

        result.Value.Should().NotBeNull();

        result.Value.Should().BeEmpty();

        File.Delete(tempFile);
    }

    [Fact]
    public async Task SaveAsync_CreatesFile()
    {
        var tempFile = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid()}.json");

        var store = new JsonDataStore<string>(tempFile);

        var data = new List<string>
        {
            "Test Event"
        };

        var result = await store.SaveAsync(data);

        result.IsSuccess.Should().BeTrue();

        File.Exists(tempFile).Should().BeTrue();

        File.Delete(tempFile);
    }

    [Fact]
    public async Task SaveAndReload_PreservesExactData()
    {
        var tempFile = Path.GetTempFileName();

        var store = new JsonDataStore<int>(tempFile);

        var originalData = new List<int>
        {
            1,
            2,
            3,
            4,
            5
        };

        await store.SaveAsync(originalData);

        var loadedData = await store.LoadAsync();

        loadedData.IsSuccess.Should().BeTrue();

        loadedData.Value.Should().BeEquivalentTo(originalData);

        File.Delete(tempFile);
    }

    [Fact]
    public async Task MultipleReloads_ReturnSameData()
    {
        var tempFile = Path.GetTempFileName();

        var store = new JsonDataStore<string>(tempFile);

        var data = new List<string>
        {
            "Conference",
            "Meetup"
        };

        await store.SaveAsync(data);

        var firstLoad = await store.LoadAsync();

        var secondLoad = await store.LoadAsync();

        firstLoad.Value.Should().BeEquivalentTo(secondLoad.Value);

        File.Delete(tempFile);
    }
}