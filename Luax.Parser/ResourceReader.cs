using System;
using System.Reflection;
using System.Text;

namespace Luax.Parser
{
    /// <summary>
    /// Resource reader
    /// </summary>
    public static class ResourceReader
    {
        private static Assembly FindAssembly(string resourceName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic)
                    continue;
                var names1 = assembly.GetManifestResourceNames();
                if (Array.Exists(names1, s => s.EndsWith(resourceName)))
                    return assembly;
            }
            return null;
        }

        /// <summary>
        /// Reads the resource from the assembly specified
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="resourceName">The name of the resource. The method reads the first resource that has the name which ends with the name specified.</param>
        /// <returns></returns>
        public static string Read(Assembly assembly, string resourceName)
        {
            if (assembly == null)
                assembly = FindAssembly(resourceName);

            if (assembly == null)
                throw new ArgumentException("Assembly containing the resource specified is not found", nameof(assembly));

            var names = assembly.GetManifestResourceNames();
            var fullName = Array.Find(names, name => name.EndsWith(resourceName));

            if (fullName == null)
                throw new ArgumentException($"Resource with the name that ends with {resourceName} is not found in the assembly specified", nameof(resourceName));

            using (var stream = assembly.GetManifestResourceStream(fullName))
            {
                var l = (int)stream.Length;
                var t = new byte[l];
                stream.Read(t, 0, l);
                if (l > 3 && t[0] == 0xef && t[1] == 0xbb && t[2] == 0xbf)
                    return Encoding.UTF8.GetString(t, 3, l - 3);
                else
                    return Encoding.UTF8.GetString(t);
            }
        }
    }
}
