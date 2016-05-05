namespace Treesor.Application
{
    public class TreesorContainer : TreesorNodePayload
    {
        public TreesorContainer()
            : base(isContainer: true)
        {
        }

        public override string ToString()
        {
            return "<Container>";
        }
    }
}