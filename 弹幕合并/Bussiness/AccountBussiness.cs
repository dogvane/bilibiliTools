using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServiceStack.OrmLite;
using 弹幕合并.Bussiness.Database;
using 弹幕合并.Bussiness.Entity;

namespace 弹幕合并.Bussiness
{
    public class AccountBussiness
    {
        static AccountBussiness()
        {
            using (var db = DbSet.GetDb())
            {
                db.CreateTableIfNotExists<Account>();
            }
        }

        public Account GetAccount(string userName)
        {
            using (var db = DbSet.GetDb())
            {
                return db.Single<Account>(o => o.UserName == userName);
            }
        }

        /// <summary>
        /// 注册账号
        /// </summary>
        /// <param name="username"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public Account Register(string username, string pwd)
        {
            using (var db = DbSet.GetDb())
            {
                Account account = new Account()
                {
                    UserName = username,
                    Pwd = pwd,
                };

                db.Insert(account);
                return account;
            }
        }
    }
}
