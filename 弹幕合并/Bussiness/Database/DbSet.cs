using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ServiceStack.OrmLite;

namespace 弹幕合并.Bussiness.Database
{
    public class DbSet
    {
        public static IDbConnection GetDb()
        {
            var dbFactory = new OrmLiteConnectionFactory(
                "dmhb.sqlite",
                SqliteDialect.Provider);
            return dbFactory.Open();
        }
    }
}
