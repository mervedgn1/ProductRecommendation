using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using ProductRecommendation.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductRecommendationHangfire
{
    public class RecommendationModelTrainJob
    {
        [DisableConcurrentExecution(0)]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public void ModelTrainJob(IJobCancellationToken jobCancellationToken, DateTime now)
        {
            using (FakeContext context = new FakeContext())
            {
                context.ServiceProvider.GetServices<IProductService>().FirstOrDefault()?.BuildDatasetAndTrainModel();
            }
        }
    }
}
