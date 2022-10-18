using Microsoft.ML;
using Microsoft.ML.Vision;
using API.classes;

namespace API.trainer
{
    public class Trainer
    {
        private IDataView? _imageData { get; set; }
        private string _dataPath { get; set; }
        private MLContext _context { get; set; }
        private IDataView? _trainSet { get; set; }
        private IDataView? _validationSet { get; set; }
        private ITransformer? TrainedModel { get; set; }
        private int _setAmount { get; set; }
        private int _generation { get; set; }


        public Trainer(string dataPath)
        {
            _dataPath = dataPath;
            _context = new MLContext();
            foreach (var file in Directory.GetFiles(_dataPath, "*.i", searchOption: SearchOption.TopDirectoryOnly))
            {
                var ex = Path.GetExtension(file);
                int i = 0;
                if (ex != ".i") continue;
                int.TryParse(new FileInfo(file).Name.Split('.')[0], out i);
                _generation = i;
                break;
            }
        }

        public void ControlData(int newNum)
        {
            bool force = newNum > _setAmount + 10;
            if (!force) return;
            TrainData(force);
        }

        public void LoadData(IEnumerable<ImageData> data, bool train = true)
        {
            _imageData = _context.Data.LoadFromEnumerable(data);

            var preprocessingPipeline = _context.Transforms.Conversion
                .MapValueToKey("LabelAsKey", "Label")
                .Append(_context.Transforms.LoadRawImageBytes("Image", _dataPath, "ImagePath"));
            IDataView shuffledData = _context.Data.ShuffleRows(_imageData);

            switch (train)
            {
                case true:
                {
                    _setAmount = data.Count();
                    _trainSet = preprocessingPipeline.Fit(shuffledData).Transform(shuffledData);
                    break;
                }
                case false:
                {
                    _validationSet = preprocessingPipeline.Fit(shuffledData).Transform(shuffledData);
                    break;
                }
            }
        }

        public void TrainData(bool forceTrain = false)
        {
            if (!File.Exists(_dataPath + "model.mod") || forceTrain)
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
                Save();
            }
            else
            {
                TrainedModel = _context.Model.Load($"{_dataPath}model.mod", out var _);
            }
        }

        private void Save()
        {
            _generation++;
            var i = _generation;
            File.Create(_dataPath + $"/{i}.i");
            File.Copy(_dataPath + "model.mod", _dataPath + $"gen{i}.mod");
            _context.Model.Save(TrainedModel, _trainSet.Schema, _dataPath + "model.mod");
        }

        public ModelOutput RunImage(byte[] img)
        {
            var image = new ModelInput() { Image = img };
            PredictionEngine<ModelInput, ModelOutput> pEngine =
                _context.Model.CreatePredictionEngine<ModelInput, ModelOutput>(TrainedModel);
            return pEngine.Predict(image);
        }
        /// <summary>
        /// Deprecated - was used for debugging purposes
        /// Took 10 random pictures and tried to classify them.
        /// </summary>
        /// <returns>List containing the predictions</returns>
        private List<ModelOutput> RunMultipleImage()
        {
            PredictionEngine<ModelInput, ModelOutput> pEngine =
                _context.Model.CreatePredictionEngine<ModelInput, ModelOutput>(TrainedModel);
            var predictions = _context.Data.CreateEnumerable<ModelInput>(_validationSet, reuseRowObject: true).Take(10);

            var output = new List<ModelOutput>();
            foreach (var item in predictions)
            {
                var debug = item.LabelAsKey;
                ModelOutput prediction = pEngine.Predict(item);
                //TODO OutputPrediction(prediction); 
                output.Add(prediction);
            }

            return output;
        }

    }
}