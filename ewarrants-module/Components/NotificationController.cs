using System;
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Data;

namespace tjc.Modules.eWarrants.Components
{
    public class NotificationController
    {
        public void CreateNotification(Notification t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Notification>();
                rep.Insert(t);
            }
        }

        public void DeleteNotification(int notificationId)
        {
            var t = GetNotification(notificationId);
            DeleteNotification(t);
        }

        public void DeleteNotification(Notification t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Notification>();
                rep.Delete(t);
            }
        }


        public IEnumerable<Notification> GetNotifications()
        {
            IEnumerable<Notification> t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Notification>();
                t = rep.Get();
            }
            return t;
        }

        public IEnumerable<Notification> GetConflictingNotifications(int judgeId, DateTime? startDate, DateTime? endDate)
        {
            IEnumerable<Notification> t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Notification>();
                t = rep.Find("Where RequestingJudgeID = @0 AND (@1 < EndDateTime AND StartDateTime < @2)", judgeId,startDate,endDate);
            }
            return t;
        }

        public IEnumerable<Notification> GetNotificationsByJudge(int judgeId, bool isCoverJudge, bool outNow, bool currentOnly)
        {
            IEnumerable<Notification> t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Notification>();
                if (isCoverJudge)
                {
                    t = rep.Find("Where CoveringJudgeID = @0",judgeId);
                }
                else
                {
                    t = rep.Find("Where RequestingJudgeID = @0", judgeId);
                }
                if (outNow)
                {
                    t.Where(n => n.EndDateTime >= DateTime.Now && n.StartDateTime <= DateTime.Now);
                }
                if (currentOnly)
                {
                    t.Where(n => n.EndDateTime >= DateTime.Now);
                }
            }
            return t;
        }
        public IEnumerable<Notification> GetCurrentNotificationsByJudge(int judgeId)
        {
            IEnumerable<Notification> t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Notification>();
                t = rep.Find("Where (GetDate() between [StartDateTime] and [EndDateTime]) and [RequestingJudgeId] = @0",judgeId);
            }
            return t;
        }

        public Notification GetNotification(int notificationId)
        {
            Notification t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Notification>();
                t = rep.GetById(notificationId);
            }
            return t;
        }

        public void UpdateNotification(Notification t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Notification>();
                rep.Update(t);
            }
        }

        public IEnumerable<Notification> GetCurrentNotifications()
        {
            IEnumerable<Notification> t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Notification>();
                t = rep.Find("Where GetDate() <= EndDateTime");
            }
            return t;
        }
    }
}
