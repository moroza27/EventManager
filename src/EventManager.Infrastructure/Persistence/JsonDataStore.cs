using System.Text.Json;
using EventManager.Application.Common;

namespace EventManager.Infrastructure.Persistence;

public class JsonDataStore<T>
{
    private readonly string _filePath;

    public JsonDataStore(string filePath)
    {
        _filePath = filePath;
    }

    public async Task<Result<List<T>>> LoadAsync()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                return Result<List<T>>.Success(new List<T>());
            }

            var json = await File.ReadAllTextAsync(_filePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                return Result<List<T>>.Success(new List<T>());
            }

            var data = JsonSerializer.Deserialize<List<T>>(json);

            return Result<List<T>>.Success(data ?? new List<T>());
        }
        catch (JsonException ex)
        {
            return Result<List<T>>.Failure($"JSON parsing error: {ex.Message}");
        }
        catch (IOException ex)
        {
            return Result<List<T>>.Failure($"I/O error: {ex.Message}");
        }
    }

    public async Task<Result> SaveAsync(List<T> data)
    {
        const int maxRetries = 3;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var json = JsonSerializer.Serialize(
                    data,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                await File.WriteAllTextAsync(_filePath, json);

                return Result.Success();
            }
            catch (IOException)
            {
                if (attempt == maxRetries)
                {
                    return Result.Failure("Unable to save file after retries.");
                }

                await Task.Delay(200);
            }
            catch (UnauthorizedAccessException)
            {
                if (attempt == maxRetries)
                {
                    return Result.Failure("Unable to save file after retries.");
                }

                await Task.Delay(200);
            }
        }

        return Result.Failure("Unknown save error.");
    }
}