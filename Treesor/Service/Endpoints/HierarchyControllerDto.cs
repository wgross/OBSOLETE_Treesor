namespace Treesor.Service.Endpoints
{
    public class HierarchyNodeCollectionBody
    {
        public HierarchyNodeBody[] nodes { get; set; }
    }

    public class HierarchyNodeBody 
    {
        public string path { get; set; }
    }

    public class HierarchyValueBody : HierarchyValueRequestBody
    {
        public string path { get; set; }
    }

    public class HierarchyValueRequestBody
    { 
        public object value { get; set; }
    }
}