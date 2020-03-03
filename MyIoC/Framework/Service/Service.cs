using MyIoC.Framework.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyIoC.Framework.Service
{
    [ImportConstructor]
    public class Service
    {
        public readonly IRepository repository;
        public Service(IRepository repository)
        {
            this.repository = repository;
        }
    }
}
