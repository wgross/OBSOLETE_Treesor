namespace Treesor.PowershellDriveProvider
{
    using Elementary.Hierarchy;
    using System.Collections.Generic;
    using System.Linq;

    public sealed class TreesorNodePath
    {
        public static readonly TreesorNodePath RootPath = new TreesorNodePath(itemPath: HierarchyPath.Create<string>());

        public static TreesorNodePath Parse(string drivePath)
        {
            TreesorNodePath result;
            TryParse(drivePath, out result);
            return result;
        }

        public static bool TryParse(string drivePath, out TreesorNodePath parsedPath)
        {
            if (string.IsNullOrEmpty(drivePath))
                parsedPath = RootPath;

            parsedPath = new TreesorNodePath(HierarchyPath.Parse(drivePath, "/"));
            return true; // currently no error cases are implemented
        }

        // introdices ambiguity
        //public static TreesorNodePath Create(HierarchyPath<string> treeKey)
        //{
        //    return new TreesorNodePath(treeKey);
        //}

        public static TreesorNodePath Create(params string[] pathItems)
        {
            return new TreesorNodePath(HierarchyPath.Create(pathItems));
        }

        #region Construction and initialization of this instance

        private TreesorNodePath(HierarchyPath<string> itemPath)
        {
            this.itemPath = itemPath;
        }

        public HierarchyPath<string> NodePath
        {
            get
            {
                return this.itemPath;
            }
        }

        private readonly HierarchyPath<string> itemPath;

        #endregion Construction and initialization of this instance

        public bool IsDrive
        {
            get
            {
                return !(this.itemPath.Items.Any());
            }
        }

        #region Override object behaviour

        public override bool Equals(object other)
        {
            TreesorNodePath otherAsNodePath = other as TreesorNodePath;

            if (otherAsNodePath == null)
                return false; // wrong type

            if (object.ReferenceEquals(this, other))
                return true; // instances are same

            return this.NodePath.Equals(otherAsNodePath.NodePath);
        }

        public override int GetHashCode()
        {
            return this.NodePath.GetHashCode();
        }

        #endregion Override object behaviour
    }
}