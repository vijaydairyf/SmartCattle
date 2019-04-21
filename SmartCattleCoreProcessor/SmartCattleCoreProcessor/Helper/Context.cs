using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCattleCoreProcessor.Helper
{
    class Context
    {
        private static ISessionFactory session;
        public static ISession Open()
        {
            session = FluentNHibernateHelper.Notifications_Session();
            ISession _session = session.OpenSession();
            return _session;
        }

        public static void Close(ISession _session)
        {
            _session.Close();
            session.Close();
            _session.Dispose();
            session.Dispose();
        }
    }
}
