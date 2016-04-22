namespace Treesor.PowershellDriveProvider
{
    public class TreesorNode
    {
        protected TreesorNode(TreesorNodePath path)
        {
            this.Path = path;
            this.Name = path.HierarchyPath.Leaf().ToString();
        }

        public TreesorNodePath Path { get; private set; }

        public string Name { get; private set; }
    }
}