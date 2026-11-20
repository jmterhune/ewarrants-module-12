using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Web.Api;
namespace tjc.Modules.eWarrants.Components
{
    public class HttpRoutingMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            try
            {
                mapRouteManager.MapHttpRoute("tjc.Modules/eWarrants", "Default", "{controller}/{action}/{moduleId}/{countyId}/{judgeTypeId}", new[] { "tjc.Modules.eWarrants.Components" });
                mapRouteManager.MapHttpRoute("tjc.Modules/eWarrants", "NotificationData", "{controller}/{action}/{moduleId}/{judgeId}", new[] { "tjc.Modules.eWarrants.Components" });
                mapRouteManager.MapHttpRoute("tjc.Modules/Interface", "PortalStatusData", "{controller}/{action}/{warrantId}/{status}", new[] { "tjc.Modules.eWarrants.Components" });
                mapRouteManager.MapHttpRoute("tjc.Modules/eWarrants", "ManageAnnotationData", "{controller}/{action}/{parameter}", new[] { "tjc.Modules.eWarrants.Components" });
                mapRouteManager.MapHttpRoute("tjc.Modules/eWarrants", "AnnotationData", "{controller}/{action}", new[] { "tjc.Modules.eWarrants.Components" });
                mapRouteManager.MapHttpRoute("eWarrants", "Default2", "{controller}/{action}/{moduleId}/{countyId}/{judgeTypeId}", new[] { "tjc.Modules.eWarrants.Components" });
                mapRouteManager.MapHttpRoute("eWarrants", "NotificationData2", "{controller}/{action}/{moduleId}/{judgeId}", new[] { "tjc.Modules.eWarrants.Components" });
                mapRouteManager.MapHttpRoute("eWarrants", "ManageAnnotationData2", "{controller}/{action}/{parameter}", new[] { "tjc.Modules.eWarrants.Components" });
                mapRouteManager.MapHttpRoute("eWarrants", "AnnotationData2", "{controller}/{action}", new[] { "tjc.Modules.eWarrants.Components" });

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}