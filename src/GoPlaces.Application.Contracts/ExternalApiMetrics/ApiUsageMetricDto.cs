namespace GoPlaces.ExternalApiMetrics
{
    public class ApiUsageMetricDto
    {
        public string ApiName { get; set; }
        public int TotalCalls { get; set; }
        public int SuccessfulCalls { get; set; }
        public int FailedCalls { get; set; }
        public double AverageDurationMs { get; set; }
    }
}