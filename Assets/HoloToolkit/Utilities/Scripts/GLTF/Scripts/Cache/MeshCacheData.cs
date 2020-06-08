using GLTF;
using System.Collections.Generic;

namespace UnityGLTF.Cache
{
    public class MeshCacheData
    {
        public UnityEngine.Mesh LoadedMesh { get; set; }
        public Dictionary<string, AttributeAccessor> MeshAttributes { get; set; }

        public MeshCacheData()
        {
            MeshAttributes = new Dictionary<string, AttributeAccessor>();
        }
    }
}
