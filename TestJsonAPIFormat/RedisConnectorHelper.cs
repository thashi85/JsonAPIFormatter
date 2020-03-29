using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestJsonAPIFormat
{
    public class RedisConnectorHelper
    {
        static RedisConnectorHelper()
        {
            try
            {
                string _ip = "127.0.0.1";// GetValue("Redis_IP");
                string _password = "";// GetValue("Redis_Credential");
                string _timeout = "6000";// GetValue("Redis_SyncTimeout");
                if (!string.IsNullOrEmpty(_ip))
                {
                    RedisConnectorHelper.lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
                    {

                        var configOptions = new ConfigurationOptions();
                        configOptions.EndPoints.Add(_ip);
                        configOptions.Password = _password;
                        configOptions.ResponseTimeout = Int32.Parse(_timeout);
                        return ConnectionMultiplexer.Connect(configOptions);

                        // return ConnectionMultiplexer.Connect(_ip + ", password=" + _password + ",syncTimeout=" + _timeout);
                        //return ConnectionMultiplexer.Connect("localhost");
                    });
                }
            }
            catch (Exception ex)
            {

            }
        }
        //private static string GetValue(string Key)
        //{
        //    return System.Configuration.ConfigurationManager.AppSettings[Key];
        //}
        private static Lazy<ConnectionMultiplexer> lazyConnection;

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }
    }

    public class CacheManager
    {
        public string GetResource(string key)
        {
            try
            {
               
                //using (MethodLogger.Log("GetFromRedisCache :" + name, LogOptions.All))
                {
                    ConnectionMultiplexer redis = RedisConnectorHelper.Connection;
                    {
                        IDatabase db = redis.GetDatabase();
                        string res = db.StringGet(key);
                        return res;
                    }
                }
            }
            catch (Exception ex)
            {
               // NLogLogger.LogError("AppCacheManager: GetFromRedisCache", ex);
            }
            return "";
        }
    }
}
