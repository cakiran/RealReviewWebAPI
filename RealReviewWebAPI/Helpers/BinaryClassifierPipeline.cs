using Microsoft.AspNetCore.Http;
using Microsoft.ML;
using Microsoft.ML.Data;
using RealReviewWebAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.ML.DataOperationsCatalog;

namespace RealReviewWebAPI.Helpers
{
    public class BinaryClassifierPipeline
    {
        private readonly string _dataPath = "Data/yelp_labelled.txt";
        private MLContext mLContext;
        private ITransformer model = null;

        public double Accuracy { get; set; }
        public double AreaUnderROCCurve { get; set; }
        public double F1Score { get; set; }

        public int MyProperty { get; set; }
        public BinaryClassifierPipeline()
        {
            mLContext = new MLContext();
            //load data and split train test
            TrainTestData trainTestData = LoadData(mLContext);
            //Build and train or fit the model
            model = BuildAndTrainModel(mLContext, trainTestData.TrainSet);
            //evaluate the model to get accuracy(100%),areaunderroccurve(1) and F1Score(1)
            Evaluate(mLContext, model, trainTestData.TestSet);
        }

        public float GetProbabilityByUsingModelWithSingleItem(string inputReviewText)
        {
            //api to perform prediction on single item
            PredictionEngine<SentimentData, SentimentPrediction> predictionFuction = mLContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(model);
            SentimentData sampleStatement = new SentimentData() { SentimentText = inputReviewText };
            var resultPrediction = predictionFuction.Predict(sampleStatement);
            return resultPrediction.Probability;
        }

        private void Evaluate(MLContext mLContext, ITransformer model, IDataView testSet)
        {
            IDataView predictions = model.Transform(testSet);
            CalibratedBinaryClassificationMetrics metrics = mLContext.BinaryClassification.Evaluate(predictions, "Label");
            Accuracy = metrics.Accuracy;
            AreaUnderROCCurve = metrics.AreaUnderRocCurve;
            F1Score = metrics.F1Score;
        }

        private ITransformer BuildAndTrainModel(MLContext mLContext, IDataView trainSet)
        {
            //featurize the commenttext column to numeric values
            var estimator = mLContext.Transforms.Text.FeaturizeText(outputColumnName: "Features", inputColumnName: nameof(SentimentData.SentimentText))
                .Append(mLContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: "Label", featureColumnName: "Features"));//add the logistic binary traning algorithm to train on the historic data
            var model = estimator.Fit(trainSet);//fit and return the trained model
            return model;
        }

        private TrainTestData LoadData(MLContext mLContext)
        {
            IDataView dataView = mLContext.Data.LoadFromTextFile<SentimentData>(_dataPath, hasHeader: false);
            TrainTestData trainTestData = mLContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
            return trainTestData;
        }
    }
}
