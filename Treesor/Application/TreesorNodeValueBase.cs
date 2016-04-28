namespace Treesor.Application
{
    public abstract class TreesorNodeValueBase
    {
        public TreesorNodeValueBase(bool isContainer)
        {
            this.IsContainer = isContainer;
        }

        public bool IsContainer { get; private set; }
    }
}