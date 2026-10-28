using DailyTrainingPlanner.Models;

namespace DailyTrainingPlanner.Services;

public class TrainingService
{
    private List<TrainingModel> _trainings = [];
    private readonly IStorageService _storageService;

    public DateTime SelectedDate {get; private set;} = DateTime.Now.Date;
    public event Action? SelectedDateChanged;
    public event Action<TrainingModel>? TrainingAdded;

    public TrainingService(IStorageService storageService)
    {
        _storageService = storageService;
    }

    public async Task InitializeAsync()
    {
        await _storageService.GetTrainingsAsync();
        _trainings = [.. await _storageService.GetTrainingsAsync(SelectedDate)];
    }

    public async Task SelectDateAsync(DateTime newDate)
    {
        SelectedDate = newDate;
        _trainings = [.. await _storageService.GetTrainingsAsync(SelectedDate)];
        SelectedDateChanged?.Invoke();
    }

    public async Task AddTrainingAsync(TrainingModel training)
    {
        var newTraining = training.Clone();
        newTraining.Id = Guid.NewGuid();
        newTraining.ForDate = newTraining.ForDate.Date;

        _trainings.Add(newTraining);
        TrainingAdded?.Invoke(newTraining);

        await _storageService.AddTrainingAsync(newTraining);
    }

    public List<TrainingModel> GetTrainings()
    {
        return [.. _trainings];
    }

    public async Task UpdateTrainingAsync(TrainingModel training)
    {
        var existingTraining = _trainings.FirstOrDefault(t => t.Id == training.Id);
        if (existingTraining is not null)
        {
            existingTraining.Text = training.Text;
            existingTraining.IsDone = training.IsDone;

            await _storageService.UpdateTrainingAsync(existingTraining);
        }
    }

    public async Task DeleteTrainingAsync(Guid trainingId)
    {
        var existingTraining = _trainings.FirstOrDefault(t => t.Id == trainingId);
        if (existingTraining is not null)
        {
            _trainings.Remove(existingTraining);

            await _storageService.DeleteTrainingAsync(trainingId);
        }
    }
}
