using System;
using System.Threading.Tasks;

namespace Bones.Flow.Core
{
    public static class Consts
    {
        public const string LOGGED = "traced";
        public const string BONES_FLOW_INSTRUMENTATION = "Bones.Flow";

        #region metrics
        public const string BONES_FLOW_METER = "Bones.Flow.Meter";
        public const string BONES_FLOW_PIPELINE_HISTOGRAM = "bones.flow.pipeline";
        public const string BONES_FLOW_MIDDLEWARE_HISTOGRAM = "bones.flow.middleware";
        #endregion
    }
}