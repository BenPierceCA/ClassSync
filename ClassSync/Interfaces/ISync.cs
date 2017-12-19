using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassSync.Models;

namespace ClassSync.Interfaces
{
    public interface ISync
    {
        string URL { get; }
        List<Class> GetAll();
    }
}
