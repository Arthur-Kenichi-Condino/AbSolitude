#if COM_UNITY_NETCODE_FOR_GAMEOBJECTS_V2_4_X
using Unity.Multiplayer.Tools.MetricTypes;
using Unity.Multiplayer.Tools.NetStats;

namespace Unity.Multiplayer.Tools.GameObjects.Tests
{
    internal class WaitForGaugeMetricValues : WaitForMetricValues<Gauge>
    {
        private double m_Value;

        public delegate bool GaugeFilter(double metric);
        private GaugeFilter m_GaugeFilterDelegate;

        public WaitForGaugeMetricValues(IMetricDispatcher dispatcher, DirectionalMetricInfo directionalMetricName)
            : base(dispatcher, directionalMetricName)
        {
        }

        public WaitForGaugeMetricValues(IMetricDispatcher dispatcher, DirectionalMetricInfo directionalMetricName, GaugeFilter counterFilter)
            : this(dispatcher, directionalMetricName)
        {
            m_GaugeFilterDelegate = counterFilter;
        }

        public bool MetricFound()
        {
            return m_Found;
        }

        public double AssertMetricValueHaveBeenFound()
        {
            AssertHasError();
            AssertIsFound();

            return m_Value;
        }

        public override void Observe(MetricCollection collection)
        {
            if (FindMetric(collection, out var metric))
            {
                var typedMetric = metric as Gauge;
                if (typedMetric == default)
                {
                    SetError(metric);
                    return;
                }

                m_Value = typedMetric.Value;
                m_Found = m_GaugeFilterDelegate != null ? m_GaugeFilterDelegate(m_Value) : true;
            }
        }
    }
}
#endif
