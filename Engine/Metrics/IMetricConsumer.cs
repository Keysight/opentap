using System.Collections.Generic;

namespace OpenTap.Metrics
{
    /// <summary> Defines that a class can consume metrics. </summary>
    public interface IMetricConsumer
    {
        /// <summary>  Event occuring when a metric producer generates out-of-band metrics. </summary>
        void OnPushMetric(ResultTable table);

        /// <summary> Defines which in a list of metrics are the ones that have interest. </summary>
        IEnumerable<MetricInfo> GetInterest(IEnumerable<MetricInfo> allMetrics);
    }
}