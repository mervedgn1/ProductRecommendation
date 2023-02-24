using Hangfire;
using ProductRecommendationService.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductRecommendationHangfire
{
    public static class RecurringJobs
    {
        public static void SchedulerJob()
        {

            RecurringJob.RemoveIfExists("BuildDatasetAndTrainModel");
            RecurringJob.AddOrUpdate<RecommendationModelTrainJob>("BuildDatasetAndTrainModel", job => job.ModelTrainJob(JobCancellationToken.Null, DateTime.Now), "*/1 * * * *", TimeZoneInfo.Local);
        }

    }
}
