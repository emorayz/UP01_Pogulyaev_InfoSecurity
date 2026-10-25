using System;

namespace InfoSecurity
{
    public static class AuditService
    {
        public static void Log(string action, string details)
        {
            if (Core.CurrentUser == null)
                return;

            using (var db = Core.CreateContext())
            {
                db.AuditEvents.Add(new AuditEvent
                {
                    EventDate = DateTime.Now,
                    UserID = Core.CurrentUser.ID,
                    Action = action,
                    Details = details,
                    IpAddress = "127.0.0.1"
                });
                db.SaveChanges();
            }
        }
    }
}
