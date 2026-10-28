namespace DailyTrainingPlanner.Models;

public class TrainingModel
{
    public Guid Id {get; set;}
    public string Text {get;set;}
    public DateTime ForDate {get; set;}
    public bool IsDone {get;set;}

    public TrainingModel Clone() => (MemberwiseClone() as TrainingModel)!;
}