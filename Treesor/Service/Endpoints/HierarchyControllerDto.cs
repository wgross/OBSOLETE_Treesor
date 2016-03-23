namespace Treesor.Service.Endpoints
{
    public class HierarchyNodeBody : HierarchyNodeRequestBody
    {
        public string Path { get; set; }
    }

    public class HierarchyNodeRequestBody
    { 
        public object Value { get; set; }
    }
}