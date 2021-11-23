using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Luax.Parser.Ast;

#pragma warning disable S125 // Sections of code should not be commented out

namespace Luax.Interpreter.Infrastructure.Stdlib
{
    internal static class StdlibString
    {
        private static LuaXClassInstance mMatchClass, mRegexpClass;
        private static LuaXTypesLibrary mTypeLibrary;

        public static void Initialize(LuaXTypesLibrary typeLibrary)
        {
            mTypeLibrary = typeLibrary;
            typeLibrary.SearchClass("match", out mMatchClass);
            mMatchClass.LuaType.Properties.Add(new LuaXProperty() { Name = "__match", Visibility = LuaXVisibility.Private, LuaType = LuaXTypeDefinition.Void });
            mMatchClass.LuaType.Properties.Add(new LuaXProperty() { Name = "__text", Visibility = LuaXVisibility.Private, LuaType = LuaXTypeDefinition.String });

            typeLibrary.SearchClass("regexp", out mRegexpClass);
            mRegexpClass.LuaType.Properties.Add(new LuaXProperty() { Name = "__regexp", Visibility = LuaXVisibility.Private, LuaType = LuaXTypeDefinition.Void });
        }

        //public static extern len(s : string) : int;
        [LuaXExternMethod("stdlib", "len")]
        public static object Extern_len(string s)
        {
            return s.Length;
        }

        //public static extern indexOf(s : string, sub:string) : int;
        [LuaXExternMethod("stdlib", "indexOf")]
        public static object Extern_indexOf(string s, string sub, bool caseInsensitive)
        {
            return s.IndexOf(sub, caseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        }

        //public static extern lastIndexOf(s : string, sub:string) : int;
        [LuaXExternMethod("stdlib", "lastIndexOf")]
        public static object Extern_lastIndexOf(string s, string sub, bool caseInsensitive)
        {
            return s.LastIndexOf(sub, caseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        }

        //public static extern left(s : string, length : int) : string;
        [LuaXExternMethod("stdlib", "left")]
        public static object Extern_left(string s, int length)
        {
            return s[..length];
        }

        //public static extern upper(s : string) : string;
        [LuaXExternMethod("stdlib", "upper")]
        public static object Extern_upper(string s)
        {
            return s.ToUpper(CultureInfo.InvariantCulture);
        }

        //public static extern upper(s : string) : string;
        [LuaXExternMethod("stdlib", "lower")]
        public static object Extern_lower(string s)
        {
            return s.ToLower(CultureInfo.InvariantCulture);
        }

        //public static extern trim(s : string) : string;
        [LuaXExternMethod("stdlib", "trim")]
        public static object Extern_trim(string s)
        {
            return s.Trim();
        }

        //public static extern right(s : string, length : int) : string;
        [LuaXExternMethod("stdlib", "right")]
        public static object Extern_right(string s, int length)
        {
            return s[^length..];
        }

        //public static extern substring(s : string, from : int, length : int) : string;
        [LuaXExternMethod("stdlib", "substring")]
        public static object Extern_substring(string s, int from, int length)
        {
            return s.Substring(from, length);
        }

        //public static extern match(s : string, re : string) : boolean;
        [LuaXExternMethod("stdlib", "match")]
        public static object Extern_match(string s, string re)
        {
            var r = CreateRegex(re);
            return r.Match(s).Success;
        }

        //public static extern unicode(s : string) : int;
        [LuaXExternMethod("stdlib", "unicode")]
        public static object Extern_unicode(string s, int index)
        {
            return ((int)s[index]) & 0xffff;
        }

        //public static extern char(unicode : int) : string;
        [LuaXExternMethod("stdlib", "char")]
        public static object Extern_char(int unicode)
        {
            return new string((char)(unicode & 0xffff), 1);
        }

        private static LuaXObjectInstance CreateMatch(string text, Match m)
        {
            var @object = mMatchClass.New(mTypeLibrary);

            @object.Properties["__match"].Value = m;
            @object.Properties["__text"].Value = text;
            return @object;
        }

        internal static Regex CreateRegex(string regex)
        {
            RegexOptions options = RegexOptions.None;
            if (regex[0] == '/')
            {
                var ix = regex.LastIndexOf('/');
                for (int i = ix + 1; i < regex.Length; i++)
                {
                    if (regex[i] == 'i')
                        options |= RegexOptions.IgnoreCase;
                    else if (regex[i] == 's')
                        options |= RegexOptions.Singleline;
                    else if (regex[i] == 'm')
                        options |= RegexOptions.Multiline;
                    else if (regex[i] == 'x')
                        options |= RegexOptions.IgnorePatternWhitespace;
                }
                regex = regex[1..ix];
            }

            return new Regex(regex, options);
        }

        //public static extern create(expression : string) : regexp;
        [LuaXExternMethod("regexp", "create")]
        public static object Regexp_Create(string regex)
        {
            var @object = mRegexpClass.New(mTypeLibrary);
            @object.Properties["__regexp"].Value = CreateRegex(regex);
            return @object;
        }

        //public extern match(text : string) : match;
        [LuaXExternMethod("regexp", "match")]
        public static object Regexp_Match(LuaXObjectInstance @this, string text)
        {
            if (@this.Properties["__regexp"].Value is not Regex re)
                throw new ArgumentException("The regular expression is not properly initialized. Use create method to create a new one", nameof(@this));

            var m = re.Match(text);
            return CreateMatch(text, m);
        }

        //public extern successful() : boolean;
        [LuaXExternMethod("match", "successful")]
        public static object Match_Successful(LuaXObjectInstance @this)
        {
            if (@this.Properties["__match"].Value is not Match m)
                throw new ArgumentException("The match is not properly initialized. Use regex class to create a new one", nameof(@this));

            return m.Success;
        }
        //public extern groupsCount() : int;
        [LuaXExternMethod("match", "groupsCount")]
        public static object Match_groupsCount(LuaXObjectInstance @this)
        {
            if (@this.Properties["__match"].Value is not Match m)
                throw new ArgumentException("The match is not properly initialized. Use regex class to create a new one", nameof(@this));

            return m.Groups.Count;
        }
        //public extern groupValue(index : int) : string;
        [LuaXExternMethod("match", "groupValue")]
        public static object Match_groupValue(LuaXObjectInstance @this, int index)
        {
            if (@this.Properties["__match"].Value is not Match m)
                throw new ArgumentException("The match is not properly initialized. Use regex class to create a new one", nameof(@this));

            return m.Groups[index].Value;
        }
        //public extern groupPosition(index : int) : string;
        [LuaXExternMethod("match", "groupPosition")]
        public static object Match_groupPosition(LuaXObjectInstance @this, int index)
        {
            if (@this.Properties["__match"].Value is not Match m)
                throw new ArgumentException("The match is not properly initialized. Use regex class to create a new one", nameof(@this));

            return m.Groups[index].Index;
        }
        //public extern groupLength(index : int) : string;
        [LuaXExternMethod("match", "groupLength")]
        public static object Match_groupLength(LuaXObjectInstance @this, int index)
        {
            if (@this.Properties["__match"].Value is not Match m)
                throw new ArgumentException("The match is not properly initialized. Use regex class to create a new one", nameof(@this));

            return m.Groups[index].Length;
        }
        //public extern next() : match;
        [LuaXExternMethod("match", "next")]
        public static object Match_next(LuaXObjectInstance @this)
        {
            if (@this.Properties["__match"].Value is not Match m)
                throw new ArgumentException("The match is not properly initialized. Use regex class to create a new one", nameof(@this));

            return CreateMatch(@this.Properties["__text"].Value as string, m.NextMatch());
        }

        //public static extern compareStrings(a : string, b : string, caseSenstitive : boolean) : int;
        [LuaXExternMethod("stdlib", "compareStrings")]
        public static object CompareStrings(string a, string b, bool caseSensitive)
        {
            int rc = string.Compare(a, b, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
            if (rc < 0)
                return -1;
            else if (rc > 0)
                return 1;
            else
                return 0;
        }

        //public static extern charIndex(s : string, char : int, startOffset : int, caseSenstitive : boolean) : int;
        [LuaXExternMethod("stdlib", "charIndex")]
        public static object CharIndex(string s, int c, int offset, bool caseSensitive)
        {
            if (s == null)
                return -1;

            char sc;
            if (caseSensitive)
                sc = (char)c;
            else
                sc = char.ToUpper((char)c);

            for (int i = offset; i < s.Length; i++)
            {
                char cc = caseSensitive ? s[i] : char.ToUpper(s[i]);
                if (cc == sc)
                    return i;
            }
            return -1;
        }

        //public static extern lastCharIndex(s : string, char : int, startOffset : int, caseSenstitive : boolean) : int;
        [LuaXExternMethod("stdlib", "lastCharIndex")]
        public static object LastCharIndex(string s, int c, int offset, bool caseSensitive)
        {
            if (s == null)
                return -1;

            char sc;
            if (caseSensitive)
                sc = (char)c;
            else
                sc = char.ToUpper((char)c);

            for (int i = s.Length - offset - 1; i >= 0; i--)
            {
                char cc = caseSensitive ? s[i] : char.ToUpper(s[i]);
                if (cc == sc)
                    return i;
            }
            return -1;
        }
    }
}