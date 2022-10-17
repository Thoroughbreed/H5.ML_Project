namespace API.classes;

public class ModelInput : ImageData
{
    public byte[] Image { get; set; }
    public UInt16 LabelAsKey { get; set; }
}