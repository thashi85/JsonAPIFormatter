using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonAPIFormatSerializer
{

    /// <summary>
    /// Options used when logging a method
    /// </summary>
    [Flags]
    public enum JS_LogOptions
    {
        /// <summary>
        /// Log entry into the method
        /// </summary>
        Entry = 0x01,
        /// <summary>
        /// Log exit from the method
        /// </summary>
        Exit = 0x02,
        /// <summary>
        /// Log the execution time of the method
        /// </summary>
        ExecutionTime = 0x04,
        /// <summary> 
        /// Log all data 
        /// </summary> 
        All = 0xFF
    }

    public class JS_MethodLogger : IDisposable
    {
        private string _methodName;
        private JS_LogOptions _options;
        private Stopwatch _sw;
      
        /// <summary>
        /// Log method entry 
        /// </summary>
        /// <param name="methodName">The name of the method being logged</param>
        /// <param name="options">The log options</param>
        /// <returns>A disposable object or none if logging is disabled</returns>
        public static IDisposable Log(string methodName, JS_LogOptions options)
        {

            // If logging off then return null, else
            return new JS_MethodLogger(methodName, options);
        }

        /// <summary>
        /// Ctor now private - just called from the static Log method
        /// </summary>
        /// <param name="methodName">The name of the method being logged</param>
        /// <param name="options">The log options</param>
        private JS_MethodLogger(string methodName, JS_LogOptions options)
        {
            _methodName = methodName;
            _options = options;
            _sw = new Stopwatch();
            _sw.Start();
            SaveLogToFile(_methodName);

            //if ((_options & LogOptions.ExecutionTime) == LogOptions.ExecutionTime)
            //{
            //    _sw = new Stopwatch();
            //    _sw.Start();
            //}

            //if ((_options & LogOptions.Entry) == LogOptions.Entry)
            //{
            //    // Log method entry here
            //    SaveLogToFile(_methodName);
            //}
        }

        /// <summary>
        /// Tidy up
        /// </summary>
        public void Dispose()
        {
            _sw.Stop();
            SaveLogToFile(_methodName + " - Miliseconds: (" + _sw.ElapsedMilliseconds + ")");
            //if ((_options & LogOptions.ExecutionTime) == LogOptions.ExecutionTime)
            //{
            //    _sw.Stop();
            //    SaveLogToFile(_methodName);
            //    // Log _sw.ElapsedMilliseconds
            //}

            //if ((_options & LogOptions.Exit) == LogOptions.Exit)
            //{
            //    // Log method exit here
            //}
        }
        public static bool SaveLogToFile(string strData)
        {
            // NLogLogger.Trace(strData);
            // return true;

            strData = DateTime.Now.ToString("dd/MMM/yyyy hh:mm:ss") + " - " + strData;

            bool bAns = false;
            StreamWriter objReader = null;
            try
            {
                //RegistryRetriever retriever = new RegistryRetriever();

                string sDirectoryPath = @"D:\LogFile"; // @"C:\TFS_Source\OptimoV4\Main\Optimo.WebAPI\Optimo.WebAPI\Optimo.WebAPI\LogFile"; //retriever.GetLogPath();
                string sFilePath = sDirectoryPath + "\\" + "JS_log.txt";

                if (!Directory.Exists(sDirectoryPath))
                {
                    Directory.CreateDirectory(sDirectoryPath);
                }

                using (objReader = new StreamWriter(sFilePath, true))
                {
                    objReader.WriteLine(strData);
                    objReader.Close();
                }
                bAns = true;
            }
            catch (Exception Ex)
            {
                if ((objReader != null))
                {
                    objReader.Close();
                }
                //throw Ex;
            }
            return bAns;
        }

    }
}
