using Microsoft.ML;
using Microsoft.ML.Vision;
using API.classes;
using API.service;

namespace API.trainer
{
    public class Trainer
    {
        private IDataView _imageData { get; set; }
        private readonly string _projectDir;
        private string _dataPath { get; set; }
        private MLContext _context { get; set; }
        private IEnumerable<ImageData> _imageDatas { get; set; }
        private services _service { get; set; } = new services();
        private IDataView _trainSet { get; set; }
        private IDataView _validationSet { get; set; }
        public ITransformer TrainedModel { get; set; }
        private int _setAmount { get; set; }

        private static readonly string[] _labels = new[] { "knallert", "bil", "cykel", "bus", "mc", "lastbil", "person" };

        public Trainer()
        {
            _projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
            _dataPath = Path.Combine(_projectDir, "data");
            _context = new MLContext();
            _trainSet = LoadData("train");
            _validationSet = LoadData("val");

        }

        private IDataView LoadData(string type)
        {
            _imageDatas = _service.LoadFromDirectory(Path.Combine(_dataPath, type));
            if (type == "train") _setAmount = _imageDatas.Count();
            _imageData = _context.Data.LoadFromEnumerable(_imageDatas);

            var preprocessingPipeline = _context.Transforms.Conversion.MapValueToKey("LabelAsKey", "Label")
                .Append(_context.Transforms.LoadRawImageBytes("Image", _dataPath, "ImagePath"));
            IDataView shuffledData = _context.Data.ShuffleRows(_imageData);
            return preprocessingPipeline.Fit(shuffledData).Transform(shuffledData);
        }

        public void TrainData()
        {
            if (!File.Exists(_dataPath + "model.mod"))
            {
                var cOpt = new ImageClassificationTrainer.Options()
                {
                    FeatureColumnName = "Image",
                    LabelColumnName = "LabelAsKey",
                    ValidationSet = _validationSet,
                    // Arch = ImageClassificationTrainer.Architecture.ResnetV2101,
                    Arch = ImageClassificationTrainer.Architecture.ResnetV250,
                    MetricsCallback = (metrics) => Console.WriteLine(metrics),
                    TestOnTrainSet = false,
                    ReuseTrainSetBottleneckCachedValues = true,
                    ReuseValidationSetBottleneckCachedValues = true
                };

                var trainingPipeline = _context.MulticlassClassification.Trainers
                    .ImageClassification(cOpt)
                    .Append(_context.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

                TrainedModel = trainingPipeline.Fit(_trainSet); // Start the damn training!
            }

            TrainedModel = _context.Model.Load($"{_dataPath}model.mod", out var _);
        }

        private void SaveModel()
        {
            _context.Model.Save(TrainedModel, _trainSet.Schema, _dataPath + "model.mod");
        }

        public void RunSingleImage()
        {
            PredictionEngine<ModelInput, ModelOutput> pEngine =
                _context.Model.CreatePredictionEngine<ModelInput, ModelOutput>(TrainedModel);
            var predictions = _context.Data.CreateEnumerable<ModelInput>(_validationSet, reuseRowObject: true).Take(10);

            foreach (var item in predictions)
            {
                var debug = item.LabelAsKey;
                ModelOutput prediction = pEngine.Predict(item);
                OutputPrediction(prediction); 
            }
        }

        public ModelOutput RunImage(byte[] img)
        {
            var image = new ModelInput() { Image = img };
            PredictionEngine<ModelInput, ModelOutput> pEngine =
                _context.Model.CreatePredictionEngine<ModelInput, ModelOutput>(TrainedModel);
            return pEngine.Predict(image);
        }

        private void OutputPrediction(ModelOutput prediction)
        {
            int scoreIndex = prediction.PredictedLabel switch
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
            var score = prediction.Score[scoreIndex]*100;
            var imageName = Path.GetFileName(prediction.ImagePath);
            
            switch (score)
            {
                case > 75:
                    Console.WriteLine($"Image: {imageName} " +
                                      $"\t| Actual Value: {prediction.Label} " +
                                      $"\t| Predicted Value: {prediction.PredictedLabel}" +
                                      $"\t| Score: {(score):N2}");
                    break;
                case < 20:
                {
                    Console.WriteLine($"I'm sorry Dave, I have absolutely no idea what {imageName} is, but here's my best guess:");
                
                    for (var i = 0; i < prediction.Score.Length; i++)
                    {
                        Console.Write($"{(prediction.Score[i]*100).ToString("N2")}%");
                        Console.WriteLine($"% {_labels[i]}");
                    }
                    break;
                }
                default:
                {
                    Console.WriteLine($"I beleive that {imageName} is a {prediction.PredictedLabel}, it should be a {prediction.Label} - I'm {score.ToString("N2")}% certain tho.");
                    Console.WriteLine($"However, it could also be a walrus ...");
                
                    for (var i = 0; i < prediction.Score.Length; i++)
                    {
                        Console.Write($"{(prediction.Score[i]*100).ToString("N2")}%");
                        Console.WriteLine($"% {_labels[i]}");
                    }
                    break;
                }
            }
            
        }
    }
}