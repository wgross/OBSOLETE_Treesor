using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Treesor.Application
{
    public class TreesorValue : TreesorNodePayload
    {
        public TreesorValue(object value)
            : base(isContainer: false)
        {
            this.Value = value;
        }

        public object Value { get; private set; }

        public override bool Equals(object obj)
        {
            var objAsTreesorNodeValue = obj as TreesorValue;
            if (objAsTreesorNodeValue == null)
                return false;

            if (this.Value == null && objAsTreesorNodeValue.Value == null)
                return true;
            else if (this.Value == null && objAsTreesorNodeValue.Value != null)
                return false;

            return this.Value.Equals(objAsTreesorNodeValue.Value);
            // return EqualityComparer<object>.Default.Equals(this.Value, objAsTreesorNodeValue.Value);
        }

        public override int GetHashCode()
        {
            return this.Value?.GetHashCode() ?? 0;
        }
    }
}
