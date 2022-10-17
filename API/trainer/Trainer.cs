using Microsoft.ML;
using static Microsoft.ML.DataOperationsCatalog;
using Microsoft.ML.Vision;
using System;
using System.Collections.Generic;
using API.classes;
using API.service;
using Microsoft.OpenApi.Expressions;


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
            var cOpt = new ImageClassificationTrainer.Options()
            {
                FeatureColumnName = "Image",
                LabelColumnName = "LabelAsKey",
                ValidationSet = _validationSet,
                Arch = ImageClassificationTrainer.Architecture.ResnetV2101,
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

        public void RunSingleImage()
        {
            PredictionEngine<ModelInput, ModelOutput> pEngine =
                _context.Model.CreatePredictionEngine<ModelInput, ModelOutput>(TrainedModel);
            ModelOutput prediction = pEngine.Predict(_context.Data.CreateEnumerable<ModelInput>(_validationSet, reuseRowObject: true).First());
            
            OutputPrediction(prediction);
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
            var imageName = Path.GetFileName(prediction.ImagePath);
            Console.WriteLine($"Image: {imageName} \t| Actual Value: {prediction.Label} \t| Predicted Value: {prediction.PredictedLabel}");
        }
    }
}