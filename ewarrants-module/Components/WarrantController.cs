using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Data;

namespace tjc.Modules.eWarrants.Components
{
    class WarrantController
    {
        public void CreateWarrant(Warrant t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Warrant>();
                rep.Insert(t);
            }
        }

        public void DeleteWarrant(int warrantId)
        {
            var t = GetWarrant(warrantId);
            DeleteWarrant(t);
        }

        public void DeleteWarrant(Warrant t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Warrant>();
                rep.Delete(t);
            }
        }
        public Warrant GetWarrant(int warrantId)
        {
            Warrant t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Warrant>();
                t = rep.GetById(warrantId);
            }
            return t;
        }
        public Warrant GetWarrantByFileId(int fileId)
        {
            Warrant t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Warrant>();
                t = rep.Find("Where FileID = @0", fileId).FirstOrDefault();
            }
            return t;
        }

        public IEnumerable<WarrantListItem> GetWarrantListItems(int moduleId, DateTime startDate, DateTime endDate)
        {
            IEnumerable<WarrantListItem> t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<WarrantListItem>();
                t = rep.Find("Where CreatedDate Between @0 And @1", startDate, endDate);
            }
            return t;
        }
        public IEnumerable<WarrantListItem> GetWarrantListItemsBySergeant(int moduleId, int userId, DateTime startDate, DateTime endDate)
        {
            IEnumerable<WarrantListItem> t;
            using (IDataContext ctx = DataContext.Instance())
            {
                t = ctx.ExecuteQuery<WarrantListItem>(System.Data.CommandType.StoredProcedure, "tjc_warrant_get_warrants_by_sergeant", moduleId, userId, startDate, endDate);
            }
            return t;
        }
        public IEnumerable<WarrantListItem> GetWarrantListItemsByUser(int moduleId, int userId, DateTime startDate, DateTime endDate)
        {
            IEnumerable<WarrantListItem> t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<WarrantListItem>();
                t = rep.Find("Where (CreatedDate Between @0 And @1) And CreatedByUserID = @2", startDate, endDate,userId);
            }
            return t;
        }
        public IEnumerable<WarrantListItem> GetWarrantListItemsNotificationNotSent()
        {
            IEnumerable<WarrantListItem> t;
            using (IDataContext ctx = DataContext.Instance())
            {
                t = ctx.ExecuteQuery<WarrantListItem>(System.Data.CommandType.StoredProcedure, "tjc_warrant_get_warrants_notification_not_sent");
            }
            return t;
        }

        public IEnumerable<WarrantListItem> GetWarrantListItemsByAgency(int moduleId, int agencyId, DateTime startDate, DateTime endDate)
        {
            IEnumerable<WarrantListItem> t;
            using (IDataContext ctx = DataContext.Instance())
            {
                t = ctx.ExecuteQuery<WarrantListItem>(System.Data.CommandType.StoredProcedure, "tjc_warrant_get_warrants_by_agency", moduleId, agencyId, startDate, endDate);
            }
            return t;
        }

        public IEnumerable<WarrantListItem> GetUnsignedWarrants( DateTime cutOffDate)
        {
            IEnumerable<WarrantListItem> t;
            using (IDataContext ctx = DataContext.Instance())
            {
                t = ctx.ExecuteQuery<WarrantListItem>(System.Data.CommandType.StoredProcedure, "tjc_warrant_get_unsigned_warrants", cutOffDate);
            }
            return t;
        }
        public IEnumerable<WarrantListItem> GetNotificationsNotSent()
        {
            IEnumerable<WarrantListItem> t;
            using (IDataContext ctx = DataContext.Instance())
            {
                t = ctx.ExecuteQuery<WarrantListItem>(System.Data.CommandType.StoredProcedure, "tjc_warrant_get_notification_not_sent");
            }
            return t;
        }
        public IEnumerable<WarrantListItem> GetUnclaimedWarrants(DateTime cutOffDate)
        {
            IEnumerable<WarrantListItem> t;
            using (IDataContext ctx = DataContext.Instance())
            {
                t = ctx.ExecuteQuery<WarrantListItem>(System.Data.CommandType.StoredProcedure, "tjc_warrant_get_unclaimed_warrants", cutOffDate);
            }
            return t;
        }

        public void DeleteOldWarrants(int moduleId,DateTime cutOffDate)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                 ctx.Execute(System.Data.CommandType.StoredProcedure, "tjc_warrant_delete_old_warrants", moduleId,cutOffDate);
            }
        }
        public void UpdatePortalStatus(int warrantId,int portalStatus)
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
        public void UpdateNotificationSent(int warrantId)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                ctx.Execute(System.Data.CommandType.StoredProcedure, "tjc_warrant_update_notification_sent", warrantId);
            }
        }
        public WarrantListItem GetWarrantListItem(int warrantId)
        {
            WarrantListItem t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<WarrantListItem>();
                t = rep.GetById(warrantId);
            }
            return t;
        }

        public void UpdateWarrant(Warrant t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Warrant>();
                rep.Update(t);
            }
        }
        public PortalWarrant GetPortalWarrant(int warrantId)
        {
            PortalWarrant t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<PortalWarrant>();
                t = rep.GetById(warrantId);
            }
            return t;
        }
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
    }
}
