namespace Treesor.PowershellDriveProvider
{
    public class TreesorValueItem : TreesorNode
    {
        public TreesorValueItem(TreesorNodePath path)
            : base(path)
        { }

        public object Value { get; set; }
    }
}