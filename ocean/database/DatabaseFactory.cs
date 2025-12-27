using ocean.Interfaces;
using SomeNameSpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ocean.database
{
    public static class DatabaseFactory
    {
        /// <summary>
        /// 创建指定类型的数据库操作实例
        /// </summary>
        /// <param name="dbType">数据库类型（如Sqlite/Access）</param>
        /// <param name="connectionString">连接字符串</param>
        /// <returns>统一的IDatabaseOperation实例</returns>
        public static IDatabaseOperation CreateDatabase(string dbType, string connectionString)
        {
            return dbType.ToLower() switch
            {
                "sqlite" => new DB_SQLlite(),
                "access" => new DB_Access(),
                _ => throw new NotSupportedException($"不支持的数据库类型：{dbType}")
            };
        }
    }
}
