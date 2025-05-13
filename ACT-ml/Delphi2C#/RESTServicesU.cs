using System.Collections.Generic;
using WinesBO;
using MVCFramework.RESTAdapter;
using MVCFramework.Serializer.Commons;
using MVCFramework.Commons;

namespace RESTServicesU
{
    [RESTResource(HttpMethod.Get, "/api/wines")]
    [MVCListOf(typeof(TWine))]
    [Mapping(typeof(TWines))]
    public interface IWineResource : IInvokable
    {
        void GetWineList(IAsynchRequest asynchReq);

        [RESTResource(HttpMethod.Post, "/api/wines")]
        void SaveWine([Body] TWine aWine, IAsynchRequest asynchReq);

        [RESTResource(HttpMethod.Put, "/api/wines/{id}")]
        void UpdateWineById([Param("id")] int aId, [Body] TWine aWine, IAsynchRequest asynchReq);
    }
}
