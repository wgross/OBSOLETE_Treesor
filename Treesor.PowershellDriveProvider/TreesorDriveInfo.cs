namespace Treesor.PowershellDriveProvider
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Management.Automation;
    public class TreesorDriveInfo : PSDriveInfo
    {
        public static TreesorDriveInfo CreateDefault(ProviderInfo provider)
        {
            return new TreesorDriveInfo(new PSDriveInfo(
               name: "treesor",
               provider: provider,
               root: string.Empty,
               description: "Treesor data store provider",
               credential: null
           ));
        }

        #region Creation and initialization of this instance

        public TreesorDriveInfo(PSDriveInfo driveInfo)
            : base(driveInfo)
        {
            this.treeService = new TreesorNodeService(driveInfo.Name);
        }

        public TreesorDriveInfo(string name, ProviderInfo provider, string root, string description, PSCredential credential) : base(name, provider, root, description, credential)
        {
        }

        private readonly TreesorNodeService treeService;
        
        #endregion Creation and initialization of this instance

        #region Get notified of end of life

        public void RemovingDrive()
        {
            return;
        }

        #endregion Get notified of end of life

        #region Implement ItemCmdletProvider

        internal bool ItemExists(TreesorNodePath path)
        {
            TreesorContainerNode jObjectAtPath;
            return this.treeService.TryGetContainer(path, out jObjectAtPath);
        }

        internal TreesorNode GetItem(TreesorNodePath TreesorNodePath)
        {
            return this.treeService.GetContainer(TreesorNodePath);
        }

        #endregion Implement ItemCmdletProvider

        #region Implement ContainerCmdletProvider

        internal IEnumerable<TreesorNode> GetChildItem(TreesorNodePath treesorNodePath, bool recursive)
        {
            if (recursive)
                return this.treeService.GetContainerDescendants(treesorNodePath);
            else
                return this.treeService.GetContainerChildren(treesorNodePath);
        }

        internal TreesorNode NewItem(TreesorNodePath treesorNodePath, string itemTypeName, object newItemValue, out bool? isContainer)
        {
            isContainer = null;

            if ("Directory".Equals(itemTypeName, StringComparison.InvariantCultureIgnoreCase))
            {
                isContainer = true;
                return this.treeService.CreateContainer(treesorNodePath);
            }
            else if ("odata".Equals(itemTypeName, StringComparison.InvariantCultureIgnoreCase))
            {
                isContainer = false;

                // create a odata endpoint at the specified place path from the latest parameter set depending
                // on te given creation parameters

                return this.treeService.CreateDataNode(treesorNodePath,
                    new ODataRepository(TreesorNewODataDynamicParameters.Get(treesorNodePath, itemTypeName).Endpoint),
                    newItemValue);
            }
            else throw new InvalidOperationException(string.Format("Item type '{0}' is unknown", itemTypeName));
        }

        internal object NewItemDynamicParameters(TreesorNodePath treesorNodePath, string itemTypeName, object newItemValue)
        {
            if ("odata".Equals(itemTypeName, StringComparison.InvariantCultureIgnoreCase))
                return TreesorNewODataDynamicParameters.Get(treesorNodePath, itemTypeName);
            return null;
        }

        internal void RemoveItem(TreesorNodePath path, bool recurse)
        {
            this.treeService.RemoveContainer(path);
        }

        internal bool HasChildItems(TreesorNodePath path)
        {
            return this.treeService.GetContainerChildren(path).Any();
        }

        internal IEnumerable<string> GetChildNames(TreesorNodePath path, ReturnContainers returnContainers)
        {
            //if (returnContainers == ReturnContainers.ReturnAllContainers)
            return this.treeService.GetContainerChildren(path).Select(c => c.Name);
            //else
            //    throw new PSNotImplementedException(string.Format("GetChildNames(path={0},returnContainers={1})", path, returnContainers));
        }

        internal TreesorContainerNode RenameItem(TreesorNodePath path, string newName)
        {
            return this.treeService.RenameContainer(path, newName);
        }

        internal void CopyItem(TreesorNodePath sourcePath, TreesorNodePath destinationPath, bool recurse)
        {
            if (!recurse)
                this.treeService.CopyContainer(sourcePath, destinationPath);
            else throw new PSNotImplementedException("recurse");
        }

        #endregion Implement ContainerCmdletProvider

        #region Implement NavigationCmdletProvider

        internal void MoveItem(TreesorNodePath path, TreesorNodePath destination)
        {
            this.treeService.MoveContainer(path, destination);
        }

        #endregion Implement NavigationCmdletProvider

        #region IPropertyCmdletProvider Members

        internal void ClearProperty(TreesorNodePath treesorNodePath, Collection<string> propertyToClear)
        {
            TreesorContainerNode treesorNode;
            if (this.treeService.TryGetContainer(treesorNodePath, out treesorNode))
            {
                foreach (var propertyName in propertyToClear)
                {
                    TreesorNodeProperty propertyDefinition;
                    if (this.treeService.TryGetNodeProperty(propertyName, out propertyDefinition))
                        treesorNode.ClearPropertyValue(propertyDefinition);
                    else throw new PSInvalidOperationException(string.Format("Property {0} doesn't exist", propertyName));
                }
            };
        }

        internal object ClearPropertyDynamicParameters(TreesorNodePath treesorNodePath, Collection<string> propertyToClear)
        {
            return new TreesorClearPropertyDynamicParameters();
        }

        internal void SetProperty(TreesorNodePath path, PSObject propertyValue)
        {
            TreesorContainerNode treesorNode;
            if (this.treeService.TryGetContainer(path, out treesorNode))
                foreach (var property in propertyValue.Properties)
                {
                    TreesorNodeProperty propertyDefinition;
                    if (this.treeService.TryGetNodeProperty(property.Name, out propertyDefinition))
                        treesorNode.SetPropertyValue(propertyDefinition, property.Value);
                    else throw new PSInvalidOperationException(string.Format("Property {0} doesn't exist", property.Name));
                }
        }

        internal object SetPropertyDynamicParameters(TreesorNodePath treesorNodePath, PSObject propertyValue)
        {
            return new TreesorSetPropertyDynamicParameters();
        }

        internal object GetProperty(TreesorNodePath treesorNodePath, Collection<string> providerSpecificPickList)
        {
            var psObject = new PSObject();

            TreesorContainerNode treesorNode;
            if (this.treeService.TryGetContainer(treesorNodePath, out treesorNode))
            {
                foreach (var propertyName in providerSpecificPickList)
                {
                    TreesorNodeProperty propertyDefinition;
                    if (this.treeService.TryGetNodeProperty(propertyName, out propertyDefinition))
                    {
                        object value;
                        if (treesorNode.TryGetPropertyValue<object>(propertyDefinition, out value))
                            psObject.Properties.Add(new PSNoteProperty(propertyName, value));
                    }
                    else throw new PSInvalidOperationException(string.Format("Property {0} doesn't exist", propertyName));
                }
            }
            return psObject;
        }

        internal object GetPropertyDynamicParameters(TreesorNodePath treesorNodePath, System.Collections.ObjectModel.Collection<string> providerSpecificPickList)
        {
            return new TreesorGetPropertyDynamicParameters();
        }

        #endregion IPropertyCmdletProvider Members

        #region IDynamicPropertyCmdletProvider Members

        internal void NewProperty(TreesorNodePath path, string propertyName, string propertyTypeName, object value)
        {
            TreesorContainerNode node;
            TreesorNodeProperty propertyDefinition;
            if (this.treeService.TryGetContainer(path, out node))
            {
                if (string.IsNullOrEmpty(propertyTypeName))
                    if (value != null)
                        propertyTypeName = value.GetType().FullName;

                if (!this.treeService.TryGetNodeProperty(propertyName, out propertyDefinition))
                    this.treeService.CreateNodeProperty(propertyDefinition = new TreesorNodeProperty(propertyName, Type.GetType(propertyTypeName)));

                node.SetPropertyValue(propertyDefinition, value);
            }
        }

        internal object NewPropertyDynamicParameters(TreesorNodePath treesorNodePath, string propertyName, string propertyTypeName, object value)
        {
            return TreesorNewPropertyDynamicParameterProvider.Instance.Get(treesorNodePath, propertyName, propertyTypeName, value);
        }

        internal void CopyProperty(TreesorNodePath sourceNodePath, string sourcePropertyName, TreesorNodePath destinationNodePath, string destinationPropertyName)
        {
            TreesorContainerNode sourceNode, destinationNode;
            TreesorNodeProperty sourceProperty, destinationProperty;
            if (this.treeService.TryGetContainer(sourceNodePath, out sourceNode))
                if (this.treeService.TryGetContainer(destinationNodePath, out destinationNode))
                    if (this.treeService.TryGetNodeProperty(sourcePropertyName, out sourceProperty))
                        if (this.treeService.TryGetNodeProperty(destinationPropertyName ?? sourcePropertyName, out destinationProperty))
                            this.treeService.CopyPropertyValue(fromNode: sourceNode, fromProperty: sourceProperty, toNode: destinationNode, toProperty: destinationProperty);
        }

        internal object CopyPropertyDynamicParameters(TreesorNodePath treesorNodePath1, string sourceProperty, TreesorNodePath treesorNodePath2, string destinationProperty)
        {
            return new TreesorCopyPropertyDynamicParameters();
        }

        internal void MoveProperty(TreesorNodePath sourceNodePath, string sourcePropertyName, TreesorNodePath destinationNodePath, string destinationPropertyName)
        {
            TreesorContainerNode sourceNode, destinationNode;
            TreesorNodeProperty sourceProperty, destinationProperty;
            if (this.treeService.TryGetContainer(sourceNodePath, out sourceNode))
                if (this.treeService.TryGetContainer(destinationNodePath, out destinationNode))
                    if (this.treeService.TryGetNodeProperty(sourcePropertyName, out sourceProperty))
                        if (this.treeService.TryGetNodeProperty(destinationPropertyName ?? sourcePropertyName, out destinationProperty))
                            this.treeService.MovePropertyValue(fromNode: sourceNode, fromProperty: sourceProperty, toNode: destinationNode, toProperty: destinationProperty);
        }

        internal object MovePropertyDynamicParameters(TreesorNodePath treesorNodePath1, string sourceProperty, TreesorNodePath treesorNodePath2, string destinationProperty)
        {
            return new TreesorMovePropertyDynamicParameters();
        }

        #endregion IDynamicPropertyCmdletProvider Members
    }
}