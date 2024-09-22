using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Shared.GameData;
public sealed class ProjectileDesc : ObjectDesc
{
    public ProjectileDesc(XElement e, uint type, string id) : base(e, type, id)
    {

    }
}
