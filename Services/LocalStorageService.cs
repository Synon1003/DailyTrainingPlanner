using System.Text.Json;
using DailyTrainingPlanner.Models;
using Microsoft.JSInterop;

namespace DailyTrainingPlanner.Services;

public interface IStorageService
{
    Task<TrainingModel> AddTrainingAsync(TrainingModel training);
    Task UpdateTrainingAsync(TrainingModel training);
    Task DeleteTrainingAsync(Guid taskId);
    Task<IEnumerable<TrainingModel>> GetTrainingsAsync();
    Task<IEnumerable<TrainingModel>> GetTrainingsAsync(DateTime forDate);
}

public class LocalStorageService: IStorageService
{
    private const string TrainingsKey = "trainings";
    private readonly IJSRuntime _jsRuntime;
    private List<TrainingModel> _allTrainings = [];

    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<TrainingModel> AddTrainingAsync(TrainingModel training)
    {
        _allTrainings.Add(training);
        await SaveTrainingsToStorageAsync();
        return training;
    }

    public async Task DeleteTrainingAsync(Guid trainingId)
    {
        var trainingToDelete = _allTrainings.FirstOrDefault(t => t.Id == trainingId);
        if (trainingToDelete is not null)
        {
            _allTrainings.Remove(trainingToDelete);
            await SaveTrainingsToStorageAsync();
        }
    }

    public async Task<IEnumerable<TrainingModel>> GetTrainingsAsync()
    {
        var trainingsString = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", TrainingsKey);

        if (string.IsNullOrWhiteSpace(trainingsString))
            return [];

        try
        {
            _allTrainings = JsonSerializer.Deserialize<List<TrainingModel>?>(trainingsString) ?? [];
            return [.. _allTrainings];
        }
        catch(Exception ex)
        {
            
        }
        return [];
    }

    public async Task<IEnumerable<TrainingModel>> GetTrainingsAsync(DateTime forDate)
    {
        if (_allTrainings.Any())
        {
            forDate = forDate.Date;
            return [.. _allTrainings.Where(t => t.ForDate.Date == forDate)];
        }
        return [];
    }

    public async Task UpdateTrainingAsync(TrainingModel training)
    {
        var trainingIndex = _allTrainings.FindIndex(t => t.Id == training.Id);
        if (trainingIndex > -1)
        {
            _allTrainings[trainingIndex] = training;
            await SaveTrainingsToStorageAsync();
        }
    }

    private static readonly JsonSerializerOptions DefaultSerializerOption = new();

    private ValueTask SaveTrainingsToStorageAsync()
    {
        var trainingsString = JsonSerializer.Serialize(_allTrainings, DefaultSerializerOption);
        return _jsRuntime.InvokeVoidAsync("localStorage.setItem", TrainingsKey, trainingsString);
    }
}