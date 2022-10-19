using System.Net;
using API.classes;
using API.trainer;

namespace API.service;

public class services
{
    private readonly Trainer _trainer;
    private readonly string _path;
    private Guid _imgGuid;
    private byte[] _imgByte;
    private static readonly string[] Labels = new[] { "knallert", "bil", "cykel", "bus", "mc", "lastbil", "person" };

    public services()
    {
        _path = Path.Combine(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../")), "data");
        _trainer = new Trainer(_path);
        InitiateData();
    }

    private void InitiateData()
    {
        var trainData = LoadFromDirectory(Path.Combine(_path, "train"));
        var valData = LoadFromDirectory(Path.Combine(_path, "val"));
        _trainer.LoadData(trainData);
        _trainer.LoadData(valData, false);
        _trainer.TrainData();
    }

    private IEnumerable<ImageData> LoadFromDirectory(string folder)
    {
        var files = Directory.GetFiles(folder, "*", searchOption: SearchOption.AllDirectories);
        foreach (var file in files)
        {
            if ((Path.GetExtension(file) != ".jpg") &&
                (Path.GetExtension(file) != ".png") &&
                (Path.GetExtension(file) != ".jpeg")) continue;
            var label = Directory.GetParent(file).Name;

            yield return new ImageData() { ImagePath = file, Label = label };
        }
    }

    private int FindScoreIndex(string label)
    {
        return label switch
        {
            "moped" => 0,
            "car" => 1,
            "bike" => 2,
            "bus" => 3,
            "motorbike" => 4,
            "truck" => 5,
            "person" => 6,
            _ => 9
        };
    }

    /// <summary>
    /// Deprecated for debug use only
    /// </summary>
    private void SaveModel()
    {
        // _trainer.Save();
    }

    public async Task<List<Tuple<string, float>>> TestImage(byte[]? image)
    {
        var _out = new List<Tuple<string, float>>();
        if (image != null)
        {
            _imgByte = image;
            var result = _trainer.RunImage(image);
            _out.Add(await OutputPrediction(result));
        }
        else
        {
            _out.Add(new Tuple<string, float>("Something wrong happened. No or defect image served.", 404));
        }

        return _out;
    }

    public async Task<HttpStatusCode> ReTrain()
    {
        var number = LoadFromDirectory(Path.Combine(_path, "train")).Count();
        var current = _trainer.ControlData();
        return _trainer.ReTrain(number > current + 10) ? HttpStatusCode.Accepted : HttpStatusCode.FailedDependency;
    }

    public async Task<Tuple<string, float, byte[], string>> Captcha()
    {
        var unknowns = LoadFromDirectory(Path.Combine(_path, "unknown"));
        if (!unknowns.Any()) return new Tuple<string, float, byte[], string>("What did you do?", 404, Array.Empty<byte>(), "");
        var imageInfo = unknowns.First();
        var image = File.ReadAllBytes(imageInfo.ImagePath);
        var result = await TestImage(image);
        return new Tuple<string, float, byte[], string>(result[0].Item1, result[0].Item2, image, imageInfo.ImagePath);
    }

    public async void CaptchaReturn(Tuple<string, string> reply)
    {
        var classification = reply.Item1;
        var imgPath = reply.Item2;

        var image = File.ReadAllBytes(imgPath);
        var result = await TestImage(image);
        if (result[0].Item2 < 33) return;
        File.Move(imgPath, Path.Combine(_path,"train", classification));
    }

    private async Task<Tuple<string, float>> OutputPrediction(ModelOutput prediction)
    {
        int scoreIndex = FindScoreIndex(prediction.PredictedLabel);
        var score = prediction.Score[scoreIndex] * 100;
        var imageName = Path.GetFileName(prediction.ImagePath);
        _imgGuid = Guid.NewGuid();

        if (score < 75) // TODO is 75% too low? Should the limit be higher...?
        {
            if (!string.IsNullOrWhiteSpace(prediction.ImagePath))
            {
                File.Move(prediction.ImagePath, Path.Combine(_path, "unknown"));
            }
            else
            {
                var file = File.Create(string.IsNullOrWhiteSpace(imageName)
                    ? Path.Combine(_path, "unknown", _imgGuid + ".jpg")
                    : Path.Combine(_path, "unknown", imageName + ".jpg"));
                await file.WriteAsync(_imgByte);
                file.Close();
            }
        }

        // This is entirely for debugging purposes 
        switch (score) // TODO Fix fucking return type dammit!
        {
            case > 75:
                Console.WriteLine($"Image: {imageName} " +
                                  $"\t| Actual Value: {prediction.Label} " +
                                  $"\t| Predicted Value: {prediction.PredictedLabel}" +
                                  $"\t| Score: {(score):N2}");
                break;
            case < 35:
            {
                Console.WriteLine(
                    $"I'm sorry Dave, I have absolutely no idea what {imageName} is, but here's my best guess:");

                for (var i = 0; i < prediction.Score.Length; i++)
                {
                    Console.Write($"{(prediction.Score[i] * 100).ToString("N2")}%");
                    Console.WriteLine($"% {Labels[i]}");
                }

                break;
            }
            default:
            {
                Console.WriteLine(
                    $"I beleive that {imageName} is a {prediction.PredictedLabel}, it should be a {prediction.Label} - I'm {score.ToString("N2")}% certain tho.");
                Console.WriteLine($"However, it could also be a walrus ...");

                for (var i = 0; i < prediction.Score.Length; i++)
                {
                    Console.Write($"{(prediction.Score[i] * 100).ToString("N2")}%");
                    Console.WriteLine($"% {Labels[i]}");
                }

                break;
            }
        }

        return score > 74 ? new Tuple<string, float>(prediction.PredictedLabel, score) : new Tuple<string, float>($"Image not classified, closest match was {prediction.PredictedLabel}", score);
    }
}