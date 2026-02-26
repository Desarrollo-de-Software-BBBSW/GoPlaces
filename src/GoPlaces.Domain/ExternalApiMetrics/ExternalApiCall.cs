using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace GoPlaces.ExternalApiMetrics
{
    // Solo auditamos la creación, porque los logs no se editan ni se borran
    public class ExternalApiCall : CreationAuditedEntity<Guid>
    {
        public string ApiName { get; private set; }
        public string Endpoint { get; private set; }
        public int DurationMs { get; private set; } // Cuánto tardó en responder
        public bool IsSuccess { get; private set; } // Si la API externa falló o no

        private ExternalApiCall() { }

        public ExternalApiCall(Guid id, string apiName, string endpoint, int durationMs, bool isSuccess)
            : base(id)
        {
            ApiName = apiName;
            Endpoint = endpoint;
            DurationMs = durationMs;
            IsSuccess = isSuccess;
        }
    }
}