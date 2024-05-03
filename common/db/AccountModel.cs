using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace common.db;
public class AccountModel {
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string Name { get; set; } = "";
    public int NextCharId { get; set; } = 0;
    public int Rank { get; set; } = 0; //0 = normal, 50 = staff, 100 = owner
}
