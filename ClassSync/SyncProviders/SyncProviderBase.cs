using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassSync.Interfaces;
using ClassSync.Models;

namespace ClassSync.SyncProviders
{
    public abstract class SyncProviderBase : ISync
    {
        public SyncProviderBase(string _url)
        {
            URL = _url;
        }

        public string URL { get; }
        public abstract List<Class> GetAll();       // Pull all classes   
    }
}
