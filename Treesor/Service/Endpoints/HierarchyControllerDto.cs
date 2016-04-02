namespace Treesor.Service.Endpoints
{
    public class HierarchyNodeBody : HierarchyNodeRequestBody
    {
        public string path { get; set; }
    }

    public class HierarchyNodeRequestBody
    { 
        public object value { get; set; }
    }
}