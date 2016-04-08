namespace Treesor.PowershellDriveProvider
{
    using Client;
    using NLog;
    using NLog.Fluent;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Management.Automation;
    using System.Threading.Tasks;
    public class TreesorDriveInfo : PSDriveInfo
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static TreesorDriveInfo CreateDefault(ProviderInfo provider)
        {
            log.Debug()
                .Message("Creating default drive provider")
                .Property(nameof(provider.Name), provider.Name)
                .Write();

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
            this.treesorService = TreesorService.Factory(new RemoteHierarchy(driveInfo.Name));
        }

        public TreesorDriveInfo(string name, ProviderInfo provider, string root, string description, PSCredential credential) : base(name, provider, root, description, credential)
        {
        }

        private readonly TreesorService treesorService;
        
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
            log.Trace().Property(nameof(path), path).Write();

            TreesorContainerNode jObjectAtPath;
            return this.treesorService.TryGetContainer(path, out jObjectAtPath);
        }

        internal TreesorNode GetItem(TreesorNodePath path)
        {
            log.Trace().Property(nameof(path), path).Write();

            return this.treesorService.GetContainer(path);
        }

        internal void SetItem(TreesorNodePath path, object value)
        {
            log.Trace().Property(nameof(path), path).Property(nameof(value.GetHashCode), value?.GetHashCode()).Write();

            this.treesorService.SetValue(path, value);
        }

        #endregion Implement ItemCmdletProvider

        #region Implement ContainerCmdletProvider

        internal IEnumerable<TreesorNode> GetChildItem(TreesorNodePath treesorNodePath, bool recursive)
        {
            if (recursive)
                return this.treesorService.GetContainerDescendants(treesorNodePath);
            else
                return this.treesorService.GetContainerChildren(treesorNodePath);
        }

        internal void ClearItem(TreesorNodePath treesorNodePath)
        {
            log.Trace().Property(nameof(treesorNodePath),treesorNodePath).Write();

            this.treesorService.RemoveValue(treesorNodePath);
        }

        internal TreesorNode NewItem(TreesorNodePath treesorNodePath, string itemTypeName, object newItemValue, out bool? isContainer)
        {
            isContainer = null;

            if ("Directory".Equals(itemTypeName, StringComparison.InvariantCultureIgnoreCase))
            {
                isContainer = true;
                return this.treesorService.CreateContainer(treesorNodePath);
            }
            else if ("File".Equals(itemTypeName, StringComparison.InvariantCultureIgnoreCase))
            {
                isContainer = false;

                // create a odata endpoint at the specified place path from the latest parameter set depending
                // on te given creation parameters

                return this.treesorService.SetValue(treesorNodePath, newItemValue);
            }
            else throw new InvalidOperationException($"Item type '{itemTypeName}' isn't supported");
        }

        internal object NewItemDynamicParameters(TreesorNodePath treesorNodePath, string itemTypeName, object newItemValue)
        {
            if ("odata".Equals(itemTypeName, StringComparison.InvariantCultureIgnoreCase))
                return TreesorNewODataDynamicParameters.Get(treesorNodePath, itemTypeName);
            return null;
        }

        internal void RemoveItem(TreesorNodePath path, bool recurse)
        {
            this.treesorService.RemoveContainer(path);
        }

        internal bool HasChildItems(TreesorNodePath path)
        {
            return this.treesorService.GetContainerChildren(path).Any();
        }

        internal IEnumerable<string> GetChildNames(TreesorNodePath path, ReturnContainers returnContainers)
        {
            //if (returnContainers == ReturnContainers.ReturnAllContainers)
            return this.treesorService.GetContainerChildren(path).Select(c => c.Name);
            //else
            //    throw new PSNotImplementedException(string.Format("GetChildNames(path={0},returnContainers={1})", path, returnContainers));
        }

        internal TreesorContainerNode RenameItem(TreesorNodePath path, string newName)
        {
            return this.treesorService.RenameContainer(path, newName);
        }

        internal void CopyItem(TreesorNodePath sourcePath, TreesorNodePath destinationPath, bool recurse)
        {
            if (!recurse)
                this.treesorService.CopyContainer(sourcePath, destinationPath);
            else throw new PSNotImplementedException("recurse");
        }

        #endregion Implement ContainerCmdletProvider

        #region Implement NavigationCmdletProvider

        internal void MoveItem(TreesorNodePath path, TreesorNodePath destination)
        {
            this.treesorService.MoveContainer(path, destination);
        }

        #endregion Implement NavigationCmdletProvider

        #region IPropertyCmdletProvider Members

        internal void ClearProperty(TreesorNodePath treesorNodePath, Collection<string> propertyToClear)
        {
            TreesorContainerNode treesorNode;
            if (this.treesorService.TryGetContainer(treesorNodePath, out treesorNode))
            {
                foreach (var propertyName in propertyToClear)
                {
                    TreesorNodeProperty propertyDefinition;
                    if (this.treesorService.TryGetNodeProperty(propertyName, out propertyDefinition))
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
            if (this.treesorService.TryGetContainer(path, out treesorNode))
                foreach (var property in propertyValue.Properties)
                {
                    TreesorNodeProperty propertyDefinition;
                    if (this.treesorService.TryGetNodeProperty(property.Name, out propertyDefinition))
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
            if (this.treesorService.TryGetContainer(treesorNodePath, out treesorNode))
            {
                foreach (var propertyName in providerSpecificPickList)
                {
                    TreesorNodeProperty propertyDefinition;
                    if (this.treesorService.TryGetNodeProperty(propertyName, out propertyDefinition))
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
            if (this.treesorService.TryGetContainer(path, out node))
            {
                if (string.IsNullOrEmpty(propertyTypeName))
                    if (value != null)
                        propertyTypeName = value.GetType().FullName;

                if (!this.treesorService.TryGetNodeProperty(propertyName, out propertyDefinition))
                    this.treesorService.CreateNodeProperty(propertyDefinition = new TreesorNodeProperty(propertyName, Type.GetType(propertyTypeName)));

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
            if (this.treesorService.TryGetContainer(sourceNodePath, out sourceNode))
                if (this.treesorService.TryGetContainer(destinationNodePath, out destinationNode))
                    if (this.treesorService.TryGetNodeProperty(sourcePropertyName, out sourceProperty))
                        if (this.treesorService.TryGetNodeProperty(destinationPropertyName ?? sourcePropertyName, out destinationProperty))
                            this.treesorService.CopyPropertyValue(fromNode: sourceNode, fromProperty: sourceProperty, toNode: destinationNode, toProperty: destinationProperty);
        }

        internal object CopyPropertyDynamicParameters(TreesorNodePath treesorNodePath1, string sourceProperty, TreesorNodePath treesorNodePath2, string destinationProperty)
        {
            return new TreesorCopyPropertyDynamicParameters();
        }

        internal void MoveProperty(TreesorNodePath sourceNodePath, string sourcePropertyName, TreesorNodePath destinationNodePath, string destinationPropertyName)
        {
            TreesorContainerNode sourceNode, destinationNode;
            TreesorNodeProperty sourceProperty, destinationProperty;
            if (this.treesorService.TryGetContainer(sourceNodePath, out sourceNode))
                if (this.treesorService.TryGetContainer(destinationNodePath, out destinationNode))
                    if (this.treesorService.TryGetNodeProperty(sourcePropertyName, out sourceProperty))
                        if (this.treesorService.TryGetNodeProperty(destinationPropertyName ?? sourcePropertyName, out destinationProperty))
                            this.treesorService.MovePropertyValue(fromNode: sourceNode, fromProperty: sourceProperty, toNode: destinationNode, toProperty: destinationProperty);
        }

        internal object MovePropertyDynamicParameters(TreesorNodePath treesorNodePath1, string sourceProperty, TreesorNodePath treesorNodePath2, string destinationProperty)
        {
            return new TreesorMovePropertyDynamicParameters();
        }

        #endregion IDynamicPropertyCmdletProvider Members
    }
}