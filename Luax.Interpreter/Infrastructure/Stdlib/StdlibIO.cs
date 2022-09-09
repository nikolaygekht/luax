using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using Luax.Parser.Ast;

#pragma warning disable S4136 // Method overloads should be grouped together
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
            mFileClass.LuaType.Properties.Add(new LuaXProperty() { Name = "__file", LuaType = LuaXTypeDefinition.Void, Visibility = LuaXVisibility.Private });
            mFileClass.LuaType.Properties.Add(new LuaXProperty() { Name = "__reader", LuaType = LuaXTypeDefinition.Void, Visibility = LuaXVisibility.Private });
            mFileClass.LuaType.Properties.Add(new LuaXProperty() { Name = "__writer", LuaType = LuaXTypeDefinition.Void, Visibility = LuaXVisibility.Private });
        }

        //public static extern exists(filename : string) : boolean;
        [LuaXExternMethod("io", "exists")]
        public static object Exists(string name) => File.Exists(name) || Directory.Exists(name);

        //public static extern size(filename : string) : real;
        [LuaXExternMethod("io", "size")]
        public static object Size(string name)
        {
            if (File.Exists(name))
                return (int)new FileInfo(name).Length;
            return 0;
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
            return array;
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
            return array;
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

        private static Encoding GetEncoding(int codePage)
        {
#pragma warning disable SYSLIB0001 // Obsolete
            if (codePage == 65000)
                return Encoding.UTF7;
            else if (codePage == 65001)
                return new UTF8Encoding(false);
            else if (codePage == 437)
                return Encoding.ASCII;
            else
                return Encoding.GetEncoding(codePage);
#pragma warning restore SYSLIB0001 // Obsolete
        }

        //public static extern writeTextToFile(path : string, text : string, codepage : int) : void;
        [LuaXExternMethod("io", "writeTextToFile")]
        public static object WriteTextToFile(string path, string text, int codePage)
        {
            File.WriteAllText(path, text, GetEncoding(codePage));
            return null;
        }

        //public static extern readTextFromFile(path : string, codepage : int) : string;
        [LuaXExternMethod("io", "readTextFromFile")]
        public static object ReadTextFromFile(string path, int codePage)
        {
            if (!File.Exists(path))
                return null;

            return File.ReadAllText(path, GetEncoding(codePage));
        }

        //public static extern tempFolder() : string;
        [LuaXExternMethod("io", "tempFolder")]
        public static object TempFolder() => Path.GetTempPath();        //NOSONAR -- accessing to temp is by design

        //public static extern combinePath(p1 : string, p2 : string) : string;
        [LuaXExternMethod("io", "combinePath")]
        public static object CombinePath(string path1, string path2) => Path.Combine(path1, path2);

        //public static extern fullPath(p1 : string) : string;
        [LuaXExternMethod("io", "fullPath")]
        public static object FullPath(string name) => Path.GetFullPath(name);

        //public static extern currentFolder() : string;
        [LuaXExternMethod("io", "currentFolder")]
        public static object CurrentDirectory() => Directory.GetCurrentDirectory();

        //public static extern createFolder(name : string) : void;
        [LuaXExternMethod("io", "createFolder")]
        public static object CreateFolder(string name) => Directory.CreateDirectory(name);

        //public static extern open(filename : string, mode : int, codePage : int) : file;
        [LuaXExternMethod("io", "open")]
        public static object Open(string file, int mode, int codePage)
        {
            FileMode fileMode = FileMode.Open;
            FileAccess access = FileAccess.Read;
            FileShare share = FileShare.None;

            if ((mode & 0x7) == 1)
                fileMode = FileMode.Open;
            else if ((mode & 0x7) == 2)
                fileMode = FileMode.Create;
            else if ((mode & 0x7) == 3)
                fileMode = FileMode.OpenOrCreate;

            if ((mode & 16) == 16 && (mode & 32) == 32)
                access = FileAccess.ReadWrite;
            else if ((mode & 16) == 16)
                access = FileAccess.Read;
            else if ((mode & 32) == 32)
                access = FileAccess.Write;

            if ((mode & 64) == 64 && (mode & 128) == 128)
                share = FileShare.ReadWrite;
            else if ((mode & 64) == 64)
                share = FileShare.Read;
            else if ((mode & 128) == 128)
                share = FileShare.Write;

            var stream = new FileStream(file, fileMode, access, share);
            var fileObject = mFileClass.New(mTypeLibrary);
            fileObject.Properties["__file"].Value = stream;
            var enc = GetEncoding(codePage);
            if ((mode & 16) == 16)
                fileObject.Properties["__reader"].Value = new StreamReader(stream, enc, leaveOpen: true);
            if ((mode & 32) == 32)
                fileObject.Properties["__writer"].Value = new StreamWriter(stream, enc, leaveOpen: true);

            return fileObject;
        }

        //public extern file.size() : int;
        [LuaXExternMethod("file", "size")]
        public static object Size(LuaXObjectInstance @this)
        {
            if (@this.Properties["__file"]?.Value is not FileStream fs)
                throw new ArgumentException("The file object isn't properly initialized", nameof(@this));

            return (int)fs.Length;
        }

        //public extern file.position() : int;
        [LuaXExternMethod("file", "position")]
        public static object Position(LuaXObjectInstance @this)
        {
            if (@this.Properties["__file"]?.Value is not FileStream fs)
                throw new ArgumentException("The file object isn't properly initialized", nameof(@this));

            return (int)fs.Position;
        }

        //public extern file.seek(position : int) : void;
        [LuaXExternMethod("file", "seek")]
        public static object Seek(LuaXObjectInstance @this, int position)
        {
            if (@this.Properties["__file"]?.Value is not FileStream fs)
                throw new ArgumentException("The file object isn't properly initialized", nameof(@this));

            fs.Position = position;
            return null;
        }

        //public extern file.readLine() : string;
        [LuaXExternMethod("file", "readLine")]
        public static object ReadLine(LuaXObjectInstance @this)
        {
            if (@this.Properties["__reader"]?.Value is not StreamReader sr)
                throw new ArgumentException("The file object isn't properly initialized", nameof(@this));

            return sr.ReadLine();
        }

        //public extern file.readByte() : int;
        [LuaXExternMethod("file", "readByte")]
        public static object ReadByte(LuaXObjectInstance @this)
        {
            if (@this.Properties["__file"]?.Value is not FileStream fs)
                throw new ArgumentException("The file object isn't properly initialized", nameof(@this));

            return fs.ReadByte();
        }

        //public extern file.readBuffer(length : int) : buffer;
        [LuaXExternMethod("file", "readBuffer")]
        public static object ReadBuffer(LuaXObjectInstance @this, int length)
        {
            if (@this.Properties["__file"]?.Value is not FileStream fs)
                throw new ArgumentException("The file object isn't properly initialized", nameof(@this));

            var b = new byte[length];
            fs.Read(b);

            var buffer = mBufferClass.New(mTypeLibrary);
            buffer.Properties["__array"].Value = b;
            return buffer;
        }

        //public extern file.writeLine(v : string) : void;
        [LuaXExternMethod("file", "writeLine")]
        public static object WriteLine(LuaXObjectInstance @this, string v)
        {
            if (@this.Properties["__writer"]?.Value is not StreamWriter fs)
                throw new ArgumentException("The file object isn't properly initialized", nameof(@this));
            fs.WriteLine(v);
            return null;
        }

        //public extern file.writeByte(v : int) : void;
        [LuaXExternMethod("file", "writeByte")]
        public static object WriteByte(LuaXObjectInstance @this, int v)
        {
            if (@this.Properties["__file"]?.Value is not FileStream fs)
                throw new ArgumentException("The file object isn't properly initialized", nameof(@this));

            Span<byte> b = stackalloc byte[1];
            b[0] = (byte)(v & 0xff);
            fs.Write(b);
            return null;
        }

        //public extern file.writeBuffer(v : buffer, int offset, int length) : void;
        [LuaXExternMethod("file", "writeBuffer")]
        public static object WriteBuffer(LuaXObjectInstance @this, LuaXObjectInstance buffer, int offset, int length)
        {
            if (@this.Properties["__file"]?.Value is not FileStream fs)
                throw new ArgumentException("The file object isn't properly initialized", nameof(@this));

            if (buffer.Properties["__array"]?.Value is not byte[] arr)
                throw new ArgumentException("The buffer object isn't properly initialized", nameof(buffer));

            fs.Write(arr, offset, length);
            return null;
        }

        //public extern lock(offset : int, length : int) : void;
        [LuaXExternMethod("file", "lock")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("macos")]
        public static object Lock(LuaXObjectInstance @this, int offset, int length)
        {
            if (@this.Properties["__file"]?.Value is not FileStream fs)
                throw new ArgumentException("The file object isn't properly initialized", nameof(@this));
            if (OperatingSystem.IsMacOS())
                throw new NotSupportedException("Locking is not supported on macOS");
            fs.Lock(offset, length);
            return null;
        }
        //public extern unlock(offset : int, length : int) : void;
        [LuaXExternMethod("file", "unlock")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("macos")]
        public static object Unlock(LuaXObjectInstance @this, int offset, int length)
        {
            if (@this.Properties["__file"]?.Value is not FileStream fs)
                throw new ArgumentException("The file object isn't properly initialized", nameof(@this));
            if (OperatingSystem.IsMacOS())
                throw new NotSupportedException("Locking is not supported on macOS");
            fs.Unlock(offset, length);
            return null;
        }

        //public extern file.close() : void;
        [LuaXExternMethod("file", "close")]
        public static object Close(LuaXObjectInstance @this)
        {
            if (@this.Properties["__reader"]?.Value is StreamReader r)
                r.Dispose();

            if (@this.Properties["__writer"]?.Value is StreamWriter w)
                w.Dispose();

            if (@this.Properties["__file"]?.Value is not FileStream fs)
                throw new ArgumentException("The file object isn't properly initialized", nameof(@this));
            fs.Dispose();

            return null;
        }
    }
}

