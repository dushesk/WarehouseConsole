using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseConsole
{
    public interface IWarehouseRepository
    {
        void Save(IEnumerable<Pallet> pallets);
        List<Pallet> Load();
    }
}
