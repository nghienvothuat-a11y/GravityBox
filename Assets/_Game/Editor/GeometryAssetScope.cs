using System;
using System.IO;

namespace GravityBox.Editor
{
    // Isolates generated campaign meshes from the permanent Physics Lab assets.
    internal sealed class GeometryAssetScope : IDisposable
    {
        private static string folder, prefix;
        private readonly string previousFolder, previousPrefix;
        public GeometryAssetScope(string destination, string namePrefix)
        {
            previousFolder = folder; previousPrefix = prefix;
            folder = destination; prefix = namePrefix; Directory.CreateDirectory(folder);
        }
        public static string MeshPath(string name) => (folder ?? PhysicsLabBuilder.Folder + "/Meshes") + "/" +
            (prefix ?? "") + name.Replace('/', '-').Replace('\\', '-') + ".asset";
        public void Dispose() { folder = previousFolder; prefix = previousPrefix; }
    }
}
