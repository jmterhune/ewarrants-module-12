using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Web.Api;
namespace tjc.Modules.eWarrants.ClerkInterface
{
    public class HttpRoutingMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            try
            {
                mapRouteManager.MapHttpRoute("tjc.Modules/eWarrants/Interface", "Default", "{controller}/{action}/{warrantId}/{status}", new[] { "tjc.Modules.eWarrants.ClerkInterface" });
                mapRouteManager.MapHttpRoute("tjc.Modules/eWarrants/Interface", "InterfaceFileData", "{controller}/{action}/{fileId}", new[] { "tjc.Modules.eWarrants.ClerkInterface" });
                mapRouteManager.MapHttpRoute("tjc.Modules/eWarrants/Interface", "InterfaceWarrantData", "{controller}/{action}", new[] { "tjc.Modules.eWarrants.ClerkInterface" });
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}