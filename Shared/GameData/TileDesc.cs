using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Shared.GameData;
public class TileDesc : ObjectDesc
{
    public readonly bool NoWalk = false;
    public TileDesc(XElement e, uint id, string name) : base(e, id, name)
    {
        NoWalk = e.ParseBool("NoWalk", false);
    }
}
