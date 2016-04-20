using System;

namespace Treesor.PowershellDriveProvider
{
    internal class ODataRepository
    {
        private Uri endpoint;

        public ODataRepository(Uri endpoint)
        {
            this.endpoint = endpoint;
        }
    }
}