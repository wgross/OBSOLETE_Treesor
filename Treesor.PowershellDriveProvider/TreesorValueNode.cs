using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Treesor.PowershellDriveProvider
{
    public class TreesorValueNode : TreesorNode
    {
        public TreesorValueNode(TreesorNodePath path)
            : base(path)
        {}

        public object Value { get; set; }
    }
}
