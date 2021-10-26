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
        /// <summary>
        /// Reads the resource from the assembly specified
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="resourceName">The name of the resource. The method reads the first resource that has the name which ends with the name specified.</param>
        /// <returns></returns>
        public static string Read(Assembly assembly, string resourceName)
        {
            var names = assembly.GetManifestResourceNames();
            string fullName = null;
            foreach (var name in names)
            {
                if (name.EndsWith(resourceName))
                {
                    fullName = name;
                    break;
                }
            }
            
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
