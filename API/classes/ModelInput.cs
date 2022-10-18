namespace API.classes;

public class ModelInput : ImageData
{
    public byte[] Image { get; set; }
    public UInt32 LabelAsKey { get; set; }
}