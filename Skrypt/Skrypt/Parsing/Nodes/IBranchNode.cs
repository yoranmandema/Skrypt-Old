using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skrypt.Parsing {
    public interface IBranchNode {
        Node Condition { get; set; }
        Node Block { get; set; }
    }
}
