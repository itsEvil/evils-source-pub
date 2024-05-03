using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace common.db;
public class CharacterModel {
    public int Id { get; set; } = 0;
    public int ObjectType { get; set; } = 0;
    public ItemModel[] Inventory { get; set; } = [];
}
