using System;
using System.Collections.Generic;
using System.Linq;
using EXOE.CsharpHelper;

namespace WpfWcfNamedPipeBinding
{
    public class DataPool
    {
        private static readonly object _syncRoot = new object();
        private volatile List<DataUnit> _datas = new List<DataUnit>();

        private DataPool()
        {
        }

        public List<DataUnit> GetNewData(DateTime oldDt)
        {
            lock (_syncRoot)
            {
                IEnumerable<DataUnit> newDu = _datas.Where(p => p.DateTime > oldDt);
                if (newDu != null && newDu.Count() > 0)
                    return newDu.ToList();
                return null;
            }
        }

        public List<DataUnit> GetAllDatas()
        {
            lock (_syncRoot)
            {
                var history = new List<DataUnit>();
                foreach (DataUnit item in _datas)
                {
                    history.Add(new DataUnit
                    {
                        DateTime = item.DateTime,
                        Message = item.Message,
                        Sender = item.Sender
                    });
                }
                return history;
            }
        }

        public void AddData(string message, string sender)
        {
            lock (_syncRoot)
            {
                _datas.Add(new DataUnit
                {
                    DateTime = DateTime.Now,
                    Message = message,
                    Sender = sender
                });
            }
        }

        public static DataPool Instance()
        {
            return Singleton<DataPool>.Instance;
        }
    }
}