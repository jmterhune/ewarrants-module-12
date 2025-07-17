using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Data;
using tjc.Modules.eWarrants.Components;
namespace tjc.Modules.eWarrants.ClerkInterface
{
    class PortalWarrantController
    {

        public IEnumerable<PortalWarrant> GetPortalWarrants()
        {
            IEnumerable<PortalWarrant> t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<PortalWarrant>();
                t = rep.Get();
            }
            return t;
        }
        public WarrantFile GetWarrantFile(int fileId)
        {
            WarrantFile file = new WarrantFile();
            using (IDataContext ctx = DataContext.Instance())
            {
                file.Bytes = ctx.ExecuteScalar<byte[]>(System.Data.CommandType.StoredProcedure, "tjc_warrant_get_file_bytes", fileId);
            }
            return file;
        }
        public IEnumerable<SignedWarrant> GetSignedWarrants()
        {
            IEnumerable<SignedWarrant> t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<SignedWarrant>();
                t = rep.Get();
            }
            return t;
        }
        public void UpdatePortalStatus(int warrantId, int portalStatus)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Warrant>();
                if (rep.GetById(warrantId) != null)
                    ctx.Execute(System.Data.CommandType.StoredProcedure, "tjc_warrant_update_eportal_status", warrantId, portalStatus);
                else
                    throw new HttpException(404, "Warrant Not Found");

            }
        }
    }
}
