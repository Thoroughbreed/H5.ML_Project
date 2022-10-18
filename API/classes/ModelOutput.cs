namespace API.classes;

public class ModelOutput : ImageData
{
    public string PredictedLabel { get; set; }
    public float[] Score { get; set; }
}