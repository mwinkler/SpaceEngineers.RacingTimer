using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mod.Data.Scripts.RacingTimer
{
    public class Logger
    {
        private TextWriter _writer;
        private StringBuilder _cache = new StringBuilder();

        public Logger(string logFile, bool append = false)
        {
            if (append)
            {
                using (var reader = MyAPIGateway.Utilities.ReadFileInGlobalStorage(logFile))
                {
                    _cache.Append(reader.ReadToEnd());
                }
            }

            _writer = MyAPIGateway.Utilities.WriteFileInGlobalStorage(logFile);
        }

        public void WriteLine(string component, string text)
        {
            if (_cache.Length > 0)
                _writer.WriteLine(_cache);

            _cache.Clear();
            
            _cache.Append(DateTime.Now.ToString("[HH:mm:ss] "));
            _writer.WriteLine(_cache.Append("[" + component + "] " + text));
            
            _writer.Flush();
            _cache.Clear();
        }

        public void Write(string text)
        {
            _cache.Append(text);
        }

        internal void Close()
        {
            if (_cache.Length > 0)
                _writer.WriteLine(_cache);

            _writer.Flush();
            _writer.Close();
        }
    }
}
