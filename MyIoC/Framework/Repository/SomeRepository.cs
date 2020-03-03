using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyIoC.Framework.Repository
{
    [Export(typeof(IRepository))]
    public class SomeRepository : AbstractRepository, IRepository
    {
        [Import]
        public Connection connection { get; set; }

        public void Create()
        {
            throw new NotImplementedException();
        }
    }
}
