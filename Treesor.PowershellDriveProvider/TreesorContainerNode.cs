﻿using System;

namespace Treesor.PowershellDriveProvider
{
    public class TreesorContainerNode : TreesorNode
    {
        internal void ClearPropertyValue(TreesorNodeProperty propertyDefinition)
        {  
            throw new NotImplementedException();
        }

        internal void SetPropertyValue(TreesorNodeProperty propertyDefinition, object value)
        {
            throw new NotImplementedException();
        }

        internal bool TryGetPropertyValue<T>(TreesorNodeProperty propertyDefinition, out object value)
        {
            throw new NotImplementedException();
        }
    }
}