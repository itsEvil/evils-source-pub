using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace common.db;
public class ItemModel {
    public int Id { get; set; } = 0;
    public string Name { get; set; } = "";
    public int[] Stats { get; set; } = [];
    public bool Tradeable { get; set; } = true;
}
