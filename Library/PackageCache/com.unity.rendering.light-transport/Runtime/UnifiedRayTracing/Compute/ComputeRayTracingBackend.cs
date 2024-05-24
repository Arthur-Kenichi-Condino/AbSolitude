namespace UnityEngine.Rendering.UnifiedRayTracing
{
    internal class ComputeRayTracingBackend : IRayTracingBackend
    {
        public ComputeRayTracingBackend(RayTracingResources resources)
        {
            m_Resources = resources;
        }

        public IRayTracingShader CreateRayTracingShader(Object shader, string dispatchFuncName)
        {
            Debug.Assert(shader is ComputeShader);
            return new ComputeRayTracingShader((ComputeShader)shader, dispatchFuncName);
        }

        public IRayTracingAccelStruct CreateAccelerationStructure(AccelerationStructureOptions options, GeometryPool geometryPool, ReferenceCounter counter)
        {
            return new ComputeRayTracingAccelStruct(options, geometryPool, m_Resources, counter);
        }

        RayTracingResources m_Resources;
    }
}
