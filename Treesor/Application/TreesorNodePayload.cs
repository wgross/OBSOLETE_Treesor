namespace Treesor.Application
{
    public abstract class TreesorNodePayload
    {
        public TreesorNodePayload(bool isContainer)
        {
            this.IsContainer = isContainer;
        }

        public bool IsContainer { get; private set; }
    }
}