using System.IO;
using System.Text;
using Luax.Parser.Ast;

#pragma warning disable S125 // Sections of code should not be commented out

namespace Luax.Interpreter.Infrastructure.Stdlib
{
    internal static class StdlibIO
    {
        private static LuaXClassInstance mFileClass, mBufferClass;

        private static LuaXTypesLibrary mTypeLibrary;

        public static void Initialize(LuaXTypesLibrary typeLibrary)
        {
            mTypeLibrary = typeLibrary;
            typeLibrary.SearchClass("buffer", out mBufferClass);
            typeLibrary.SearchClass("file", out mFileClass);
        }

        //public static extern exists(filename : string) : boolean;
        [LuaXExternMethod("io", "exists")]
        public static object Exists(string name) => File.Exists(name) || Directory.Exists(name);
        
        //public static extern size(filename : string) : real;
        [LuaXExternMethod("io", "size")]
        public static object Size(string name)
        {
            if (File.Exists(name))
                return (double)((new FileInfo(name)).Length);
            return (double)0;

        }

        //public static extern isFolder(filename : string) : boolean;
        [LuaXExternMethod("io", "isFolder")]
        public static object IsFolder(string name) => Directory.Exists(name);
        
        //public static extern isFile(filename : string) : boolean;
        [LuaXExternMethod("io", "isFile")]
        public static object IsFile(string name) => File.Exists(name);

        //public static extern files(path : string) : string[];
        [LuaXExternMethod("io", "files")]
        public static object Files(string name)
        {
            if (!Directory.Exists(name))
                return null;
            string[] files = Directory.GetFiles(name);
            LuaXVariableInstanceArray array = new LuaXVariableInstanceArray(LuaXTypeDefinition.String.ArrayOf(), files.Length);
            for (int i = 0; i < files.Length; i++)
                array[i].Value = files[i];
            return files;
        }

        //public static extern files(path : string) : string[];
        [LuaXExternMethod("io", "folders")]
        public static object Folders(string name)
        {
            if (!Directory.Exists(name))
                return null;
            string[] directories = Directory.GetDirectories(name);
            LuaXVariableInstanceArray array = new LuaXVariableInstanceArray(LuaXTypeDefinition.String.ArrayOf(), directories.Length);
            for (int i = 0; i < directories.Length; i++)
                array[i].Value = directories[i];
            return directories;
        }
        
        //public static extern delete(path : string) : void;
        [LuaXExternMethod("io", "delete")]
        public static object Delete(string path)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            else if (File.Exists(path))
                File.Delete(path);
            return null;
        }

#pragma warning disable SYSLIB0001 // Type or member is obsolete        
        //public static extern writeTextToFile(path : string, text : string, codepage : int) : void;
        [LuaXExternMethod("io", "writeTextToFile")]
        public static object WriteTextToFile(string path, string text, int codePage)
        {
            Encoding encoding;
            if (codePage == 65000)
                encoding = Encoding.UTF7;
            else if (codePage == 65001)
                encoding = new UTF8Encoding(false);
            else if (codePage == 437)
                encoding = Encoding.ASCII;
            else
                encoding = Encoding.GetEncoding(codePage);

            File.WriteAllText(path, text, encoding);
            return null;
        }
        
        //public static extern readTextFromFile(path : string, codepage : int) : string;
        [LuaXExternMethod("io", "readTextFromFile")]
        public static object ReadTextFromFile(string path, int codePage)
        {
            if (!File.Exists(path))
                return null;

            Encoding encoding;
            if (codePage == 65000)
                encoding = Encoding.UTF7;
            else if (codePage == 65001)
                encoding = new UTF8Encoding(false);
            else if (codePage == 437)
                encoding = Encoding.ASCII;
            else
                encoding = Encoding.GetEncoding(codePage);

            return File.ReadAllText(path, encoding);
        }
#pragma warning restore SYSLIB0001 // Type or member is obsolete

        //public static extern tempFolder() : string;
        [LuaXExternMethod("io", "tempFolder")]
        public static object TempFolder() => Path.GetTempPath();

        //public static extern combinePath(p1 : string, p2 : string) : string;
        [LuaXExternMethod("io", "combinePath")]
        public static object CombinePath(string path1, string path2) => Path.Combine(path1, path2);

        //public static extern fullPath(p1 : string) : string;
        [LuaXExternMethod("io", "fullPath")]
        public static object FullPath(string name) => Path.GetFullPath(name);

        //public static extern currentDirectory() : string;
        [LuaXExternMethod("io", "currentDirectory")]
        public static object CurrentDirectory() => Directory.GetCurrentDirectory();

        //public static extern createFolder(name : string) : void;
        [LuaXExternMethod("io", "createFolder")]
        public static object CreateFolder(string name) => Directory.CreateDirectory(name);
    }

}
