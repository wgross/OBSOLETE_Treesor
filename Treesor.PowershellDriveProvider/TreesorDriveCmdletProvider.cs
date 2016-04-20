using System.Linq;

namespace Treesor.PowershellDriveProvider
{
    using Elementary.Hierarchy;
    using NLog;
    using NLog.Fluent;
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Provider;

    [CmdletProvider("Treesor", ProviderCapabilities.None)]
    public class TreesorDriveCmdletProvider : NavigationCmdletProvider, IPropertyCmdletProvider, IDynamicPropertyCmdletProvider
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private TreesorDriveInfo GetTreesorDriveInfo()
        {
            return (TreesorDriveInfo)this.PSDriveInfo;
        }

        #region Override DriveCmdletProvider methods

        protected override PSDriveInfo NewDrive(PSDriveInfo drive)
        {
            // Check if the drive object is null.
            if (drive == null)
            {
                this.WriteError(new ErrorRecord(new ArgumentNullException("drive"), "NullDrive", ErrorCategory.InvalidArgument, targetObject: null));

                return null;
            }

            // Check if the drive root is not null or empty
            // and if it is an existing file.
            //if (String.IsNullOrEmpty(drive.Root) || (File.Exists(drive.Root) == false))
            //{
            //    WriteError(new ErrorRecord(
            //               new ArgumentException("drive.Root"),
            //               "Nof",
            //               ErrorCategory.InvalidArgument,
            //               drive));

            //    return null;
            //}

            TreesorDriveInfo treesorDriveInfo = drive as TreesorDriveInfo;

            if (treesorDriveInfo == null)
            {
                return null;
            }

            return treesorDriveInfo;
        }

        protected override Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            log.Info().Message($"Created default drive for provider '{this.ProviderInfo.Description}'").Write();

            return new Collection<PSDriveInfo> { TreesorDriveInfo.CreateDefault(this.ProviderInfo) };
        }

        protected override PSDriveInfo RemoveDrive(PSDriveInfo drive)
        {
            TreesorDriveInfo treesorDriveInfo = drive as TreesorDriveInfo;

            if (treesorDriveInfo == null)
            {
                this.WriteError(new ErrorRecord(new ArgumentNullException("drive"), "NullDrive", ErrorCategory.InvalidArgument, null));
                return null;
            }

            treesorDriveInfo.RemovingDrive();

            return treesorDriveInfo;
        }

        #endregion Override DriveCmdletProvider methods

        #region Override ItemCmdletProvider methods

        protected override void ClearItem(string path)
        {
            log.Trace().Property(nameof(path),path).Write();

            this.GetTreesorDriveInfo().ClearItem(TreesorNodePath.Parse(path));
        }

        protected override string[] ExpandPath(string path)
        {
            log.Debug().Message("Processing ExpandPath({0})", path ?? "null").Write();

            throw new NotImplementedException("ExpandPath");
        }

        protected override void GetItem(string path)
        {
            log.Debug().Message("Processing GetItem({0})", path ?? "null").Write();

            var treesorNodePath = TreesorNodePath.Parse(path);

            // Check to see if the path represents a valid drive.
            if (treesorNodePath.IsDrive)
            {
                this.WriteItemObject(this.PSDriveInfo, path, isContainer: true);
                return;
            }
            else
            {
                var treeItem = this.GetTreesorDriveInfo().GetItem(treesorNodePath);

                this.WriteItemObject(treeItem, path, isContainer: true);
            }
        }

        protected override bool IsValidPath(string path)
        {
            log.Debug("Processing IsValidPath({0})", path ?? "null");

            TreesorNodePath parsedPath;
            return TreesorNodePath.TryParse(path, out parsedPath);
        }

        protected override bool ItemExists(string path)
        {
            log.Trace().Property(nameof(path), path).Write();

            // is called by Test-Path
            return this.GetTreesorDriveInfo().ItemExists(TreesorNodePath.Parse(path));
        }

        protected override void SetItem(string path, object value)
        {
            log.Trace().Property(nameof(path), path).Property(nameof(value.GetHashCode), value?.GetHashCode()).Write();

            this.GetTreesorDriveInfo().SetItem(TreesorNodePath.Parse(path), value);
        }

        
        #endregion Override ItemCmdletProvider methods

        #region Override ContainerCmdletProvider methods

        protected override bool ConvertPath(string path, string filter, ref string updatedPath, ref string updatedFilter)
        {
            log.Debug().Message("Processing ConvertPath({0}, {1},..)", path ?? "null", filter ?? "null").Write();

            updatedPath = path; // TreesorNodePath.Parse(path).ToString();
            updatedFilter = filter;

            return true;
        }

        protected override void CopyItem(string path, string copyPath, bool recurse)
        {
            log.Debug().Message("Processing CopyItem({0}, {1}, {2})", path ?? "null", copyPath ?? "null", recurse).Write();

            this.GetTreesorDriveInfo().CopyItem(sourcePath: TreesorNodePath.Parse(path), destinationPath: TreesorNodePath.Parse(copyPath), recurse: recurse);
        }

        protected override void GetChildItems(string path, bool recurse)
        {
            log.Debug().Message("Processing GetChildItems({0}, {1})", path ?? "Null", recurse).Write();

            foreach (var childITem in this.GetTreesorDriveInfo().GetChildItem(TreesorNodePath.Parse(path), recurse))
            {
                this.WriteItemObject(childITem, childITem.Name.ToString(), true);
            }
        }

        protected override void GetChildNames(string path, ReturnContainers returnContainers)
        {
            log.Debug().Message("Processing GetChildNames({0})", path ?? "null").Write();

            foreach (var childName in this.GetTreesorDriveInfo().GetChildNames(TreesorNodePath.Parse(path), returnContainers))
            {
                this.WriteItemObject(childName, path, true);
            }
        }

        protected override bool HasChildItems(string path)
        {
            log.Debug("Processing HasChildItems({0})", path ?? "null");

            // Verifies if their are children under the specified node
            return this.GetTreesorDriveInfo().HasChildItems(TreesorNodePath.Parse(path));
        }

        protected override void NewItem(string path, string itemTypeName, object newItemValue)
        {
            log.Trace()
                .Property(nameof(path), path)
                .Property(nameof(itemTypeName), itemTypeName)
                .Property(nameof(newItemValue), newItemValue?.GetHashCode())
                .Write();

            bool? isContainer;
            var newItem = this.GetTreesorDriveInfo().NewItem(TreesorNodePath.Parse(path), itemTypeName, newItemValue, out isContainer);

            this.WriteItemObject(newItem, path, isContainer.GetValueOrDefault(false));
        }

        protected override object NewItemDynamicParameters(string path, string itemTypeName, object newItemValue)
        {
            return this.GetTreesorDriveInfo().NewItemDynamicParameters(TreesorNodePath.Parse(path), itemTypeName, newItemValue);
        }

        protected override void RemoveItem(string path, bool recurse)
        {
            log.Debug("Processing RemoveItem({0}, {1})", path ?? "null", recurse);

            this.GetTreesorDriveInfo().RemoveItem(TreesorNodePath.Parse(path), recurse);
        }

        protected override void RenameItem(string path, string newName)
        {
            log.Debug("Processing RenameItem({0}, {1})", path ?? "null", newName ?? "null");

            var renamedItem = this.GetTreesorDriveInfo().RenameItem(TreesorNodePath.Parse(path), newName);
        }

        #endregion Override ContainerCmdletProvider methods

        #region Override NavigationCmdletProvider methods

        protected override void MoveItem(string path, string destination)
        {
            log.Debug("Processing MoveItem({0}, {1})", path ?? "null", destination ?? "null");

            this.GetTreesorDriveInfo().MoveItem(path: TreesorNodePath.Parse(path), destination: TreesorNodePath.Parse(destination));
        }

        protected override string GetChildName(string path)
        {
            log.Debug("Processing GetChildName({0})", path ?? "null");

            return TreesorNodePath.Parse(path).HierarchyPath.Leaf().ToString();
        }

        protected override string MakePath(string parent, string child)
        {
            log.Debug("Processing MakePath({0}, {1})", parent ?? "null", child ?? "null");

            return TreesorNodePath.Create(TreesorNodePath.Parse(parent).HierarchyPath.Join(TreesorNodePath.Parse(child).HierarchyPath).Items.ToArray()).HierarchyPath.ToString();
        }

        protected override string GetParentPath(string path, string root)
        {
            log.Debug("Processing GetParentPath({0}, {1})", path ?? "null", root ?? "null");
            var parsedPath = TreesorNodePath.Parse(path).HierarchyPath;
            if (parsedPath.HasParentNode)
                return parsedPath.Parent().ToString();
            else return root;
            //return TreesorNodePath.Parse(path).NodePath.Parent().ToString();
        }

        protected override bool IsItemContainer(string path)
        {
            return (this.GetTreesorDriveInfo().GetItem(TreesorNodePath.Parse(path)) != null);
        }

        #endregion Override NavigationCmdletProvider methods

        #region IPropertyCmdletProvider Members

        public void ClearProperty(string path, Collection<string> propertyToClear)
        {
            this.GetTreesorDriveInfo().ClearProperty(TreesorNodePath.Parse(path), propertyToClear);
        }

        public object ClearPropertyDynamicParameters(string path, Collection<string> propertyToClear)
        {
            log.Debug("Processing ClearPropertyDynamicParameters({0}, {1})", path ?? "null", string.Join(",", propertyToClear));

            return this.GetTreesorDriveInfo().ClearPropertyDynamicParameters(TreesorNodePath.Parse(path), propertyToClear);
        }

        public void GetProperty(string path, Collection<string> providerSpecificPickList)
        {
            log.Debug("Processing: GetProperty({0},{1})", path ?? "null", string.Join(",", providerSpecificPickList));

            var value = this.GetTreesorDriveInfo().GetProperty(TreesorNodePath.Parse(path), providerSpecificPickList);
            this.WritePropertyObject(value, path);
        }

        public object GetPropertyDynamicParameters(string path, Collection<string> providerSpecificPickList)
        {
            log.Debug("Processing GetPropertyDynamicParameters({0}, {1})", path ?? "null", string.Join(",", providerSpecificPickList));

            return this.GetTreesorDriveInfo().GetPropertyDynamicParameters(TreesorNodePath.Parse(path), providerSpecificPickList);
        }

        public void SetProperty(string path, PSObject propertyValue)
        {
            log.Debug("Processing SetProperty({0}, {1})", path ?? "null", propertyValue.ToString());

            this.GetTreesorDriveInfo().SetProperty(TreesorNodePath.Parse(path), propertyValue);
        }

        public object SetPropertyDynamicParameters(string path, PSObject propertyValue)
        {
            log.Debug("Processing SetPropertyDynamicParameters({0}, {1})", path ?? "null", propertyValue);

            return this.GetTreesorDriveInfo().SetPropertyDynamicParameters(TreesorNodePath.Parse(path), propertyValue);
        }

        #endregion IPropertyCmdletProvider Members

        #region IDynamicPropertyCmdletProvider Members

        public void CopyProperty(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
        {
            log.Debug("Processing: CopyProperty({0}, {1}, {2}, {3}", sourcePath ?? "null", sourceProperty ?? "null", destinationPath ?? "null", destinationProperty ?? "null");

            this.GetTreesorDriveInfo().CopyProperty(TreesorNodePath.Parse(sourcePath), sourceProperty, TreesorNodePath.Parse(destinationPath), destinationProperty);
        }

        public object CopyPropertyDynamicParameters(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
        {
            log.Debug("Processing: CopyPropertyDynamicParameters({0}, {1}, {2}, {3}", sourcePath ?? "null", sourceProperty ?? "null", destinationPath ?? "null", destinationProperty ?? "null");

            return this.GetTreesorDriveInfo().CopyPropertyDynamicParameters(TreesorNodePath.Parse(sourcePath), sourceProperty, TreesorNodePath.Parse(destinationPath), destinationProperty);
        }

        public void MoveProperty(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
        {
            log.Debug("Processing: MoveProperty({0}, {1}, {2}, {3}", sourcePath ?? "null", sourceProperty ?? "null", destinationPath ?? "null", destinationProperty ?? "null");

            this.GetTreesorDriveInfo().MoveProperty(TreesorNodePath.Parse(sourcePath), sourceProperty, TreesorNodePath.Parse(destinationPath), destinationProperty);
        }

        public object MovePropertyDynamicParameters(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
        {
            log.Debug("Processing: MovePropertyDynamicParameters({0}, {1}, {2}, {3}", sourcePath ?? "null", sourceProperty ?? "null", destinationPath ?? "null", destinationProperty ?? "null");

            return this.GetTreesorDriveInfo().MovePropertyDynamicParameters(TreesorNodePath.Parse(sourcePath), sourceProperty, TreesorNodePath.Parse(destinationPath), destinationProperty);
        }

        public void NewProperty(string path, string propertyName, string propertyTypeName, object value)
        {
            log.Debug("Processing: NewProperty({0}, {1}, {2}, {3}", path ?? "null", propertyTypeName ?? "null", propertyTypeName ?? "null", value ?? (object)"null");

            this.GetTreesorDriveInfo().NewProperty(TreesorNodePath.Parse(path), propertyName, propertyTypeName, value);
        }

        public object NewPropertyDynamicParameters(string path, string propertyName, string propertyTypeName, object value)
        {
            log.Debug("Processing: NewPropertyDynamicParameters({0}, {1}, {2}, {3}", path ?? "null", propertyTypeName ?? "null", propertyTypeName ?? "null", value ?? (object)"null");

            return this.GetTreesorDriveInfo().NewPropertyDynamicParameters(TreesorNodePath.Parse(path), propertyName, propertyTypeName, value);
        }

        public void RemoveProperty(string path, string propertyName)
        {
            throw new NotImplementedException();
        }

        public object RemovePropertyDynamicParameters(string path, string propertyName)
        {
            throw new NotImplementedException();
        }

        public void RenameProperty(string path, string sourceProperty, string destinationProperty)
        {
            throw new NotImplementedException();
        }

        public object RenamePropertyDynamicParameters(string path, string sourceProperty, string destinationProperty)
        {
            throw new NotImplementedException();
        }

        #endregion IDynamicPropertyCmdletProvider Members
    }
}