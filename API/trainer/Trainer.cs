using Microsoft.ML;
using Microsoft.ML.Vision;
using API.classes;

namespace API.trainer
{
    public class Trainer
    {
        private readonly string _dataPath;
        private readonly MLContext _context;
        private bool _training;
        private IDataView? ImageData { get; set; }
        private IDataView? TrainSet { get; set; }
        private IDataView? ValidationSet { get; set; }
        private ITransformer? TrainedModel { get; set; }
        private int SetAmount { get; set; }
        private int Generation { get; set; }


        protected internal Trainer(string dataPath)
        {
            _dataPath = dataPath;
            _context = new MLContext();
            foreach (var file in Directory.GetFiles(_dataPath, "*.i", searchOption: SearchOption.TopDirectoryOnly))
            {
                var ex = Path.GetExtension(file);
                if (ex != ".i") continue;
                int.TryParse(new FileInfo(file).Name.Split('.')[0], out int i);
                Generation = i;
                break;
            }
        }

        protected internal int ControlData()
        {
            return SetAmount;
        }

        protected internal bool ReTrain(bool force)
        {
            TrainData(force);
            return force;
        }

        protected internal void LoadData(IList<ImageData> data, bool train = true)
        {
            ImageData = _context.Data.LoadFromEnumerable(data);

            var preprocessingPipeline = _context.Transforms.Conversion
                .MapValueToKey("LabelAsKey", "Label")
                .Append(_context.Transforms.LoadRawImageBytes("Image", _dataPath, "ImagePath"));
            IDataView shuffledData = _context.Data.ShuffleRows(ImageData);

            switch (train)
            {
                case true:
                {
                    SetAmount = data.Count;
                    TrainSet = preprocessingPipeline.Fit(shuffledData).Transform(shuffledData);
                    break;
                }
                case false:
                {
                    ValidationSet = preprocessingPipeline.Fit(shuffledData).Transform(shuffledData);
                    break;
                }
            }
        }

        protected internal void TrainData(bool forceTrain = false)
        {
            if (!File.Exists(_dataPath + "model.mod") || forceTrain)
            {
                Task.Run(TrainNow);
            }
            else
            {
                TrainedModel = _context.Model.Load($"{_dataPath}model.mod", out var _);
            }
        }

        protected internal ModelOutput RunImage(byte[] img)
        {
            var image = new ModelInput() { Image = img };
            PredictionEngine<ModelInput, ModelOutput> pEngine =
                _context.Model.CreatePredictionEngine<ModelInput, ModelOutput>(TrainedModel);
            return pEngine.Predict(image);
        }

        private void TrainNow()
        {
            if (_training) return;
            _training = true;
            Console.WriteLine("Starting goddamn training bro!");
            var cOpt = new ImageClassificationTrainer.Options()
            {
                FeatureColumnName = "Image",
                LabelColumnName = "LabelAsKey",
                ValidationSet = ValidationSet,
                Arch = ImageClassificationTrainer.Architecture.ResnetV2101,
                // Arch = ImageClassificationTrainer.Architecture.ResnetV250,
                MetricsCallback = (metrics) => Console.WriteLine(metrics),
                TestOnTrainSet = false,
                ReuseTrainSetBottleneckCachedValues = true,
                ReuseValidationSetBottleneckCachedValues = true
            };

            var trainingPipeline = _context.MulticlassClassification.Trainers
                .ImageClassification(cOpt)
                .Append(_context.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            TrainedModel = trainingPipeline.Fit(TrainSet); // Start the damn training!
            Console.WriteLine("Training completed ... saving new generation");
            Save();
            _training = false;
        }

        private void Save()
        {
            Generation++;
            var i = Generation;
            File.Create(_dataPath + $"/{i}.i");
            File.Copy(_dataPath + "model.mod", _dataPath + $"gen{i}.mod");
            _context.Model.Save(TrainedModel, TrainSet?.Schema, _dataPath + "model.mod");
        }
    }
}