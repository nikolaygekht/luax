using System;
using FluentAssertions;
using Luax.Parser.Ast;
using Luax.Parser.Ast.Builder;
using Luax.Parser.Test.Tools;
using Xunit;

namespace Luax.Parser.Test
{
    public class TreeBuilderTest_Body
    {
        [Fact]
        public void VariableDeclaration_Success()
        {
            const string tree = "[STATEMENTS[STATEMENT[DECLARATION[VAR[var(var)]][DECL_LIST[DECL[IDENTIFIER(i)][COLON[:(:)]][TYPE_DECL[TYPE_NAME[TYPE_INT[int(int)]]]]]][EOS[;(;)]]]]]";
            var node = AstNodeExtensions.Parse(tree);
            var @class = new LuaXClass("a");
            var @method = new LuaXMethod(@class) { Name = "b" };
            var processor = new LuaXAstTreeCreator("");
            processor.ProcessBody(node, @class, method);
            method.Variables.Should().HaveCount(1);
            var v = method.Variables[0];
            v.Name.Should().Be("i");
            v.LuaType.IsInteger().Should().BeTrue();
        }

        [Fact]
        public void VariableDeclaration_Fail_VariableExists()
        {
            const string tree = "[STATEMENTS[STATEMENT[DECLARATION[VAR[var(var)]][DECL_LIST[DECL[IDENTIFIER(i)][COLON[:(:)]][TYPE_DECL[TYPE_NAME[TYPE_INT[int(int)]]]]]][EOS[;(;)]]]]]";
            var node = AstNodeExtensions.Parse(tree);
            var @class = new LuaXClass("a");
            var method = new LuaXMethod(@class) { Name = "b" };
            method.Variables.Add(new LuaXVariable() { Name = "i" });
            var processor = new LuaXAstTreeCreator("");
            ((Action)(() => processor.ProcessBody(node, @class, method))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void VariableDeclaration_Fail_ArgumentExists()
        {
            const string tree = "[STATEMENTS[STATEMENT[DECLARATION[VAR[var(var)]][DECL_LIST[DECL[IDENTIFIER(i)][COLON[:(:)]][TYPE_DECL[TYPE_NAME[TYPE_INT[int(int)]]]]]][EOS[;(;)]]]]]";
            var node = AstNodeExtensions.Parse(tree);
            var @class = new LuaXClass("a");
            var method = new LuaXMethod(@class) { Name = "b" };
            method.Arguments.Add(new LuaXVariable() { Name = "i" });
            var processor = new LuaXAstTreeCreator("");
            ((Action)(() => processor.ProcessBody(node, @class, method))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void VariableDeclaration_Fail_ConstantExists()
        {
            const string tree = "[STATEMENTS[STATEMENT[DECLARATION[VAR[var(var)]][DECL_LIST[DECL[IDENTIFIER(i)][COLON[:(:)]][TYPE_DECL[TYPE_NAME[TYPE_INT[int(int)]]]]]][EOS[;(;)]]]]]";
            var node = AstNodeExtensions.Parse(tree);
            var @class = new LuaXClass("a");
            var method = new LuaXMethod(@class) { Name = "b" };
            method.Constants.Add(new LuaXConstantVariable() { Name = "i" });
            var processor = new LuaXAstTreeCreator("");
            ((Action)(() => processor.ProcessBody(node, @class, method))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void ConstantDeclaration_Success()
        {
            const string tree = "[STATEMENTS[STATEMENT[CONST_DECLARATION[CONST_KW[const(const)]][IDENTIFIER(i)][ASSIGN[=(=)]][CONSTANT[STRING[STRINGDQ(\"a\")]]][EOS[;(;)]]]]]";
            var node = AstNodeExtensions.Parse(tree);
            var @class = new LuaXClass("a");
            var @method = new LuaXMethod(@class) { Name = "b" };
            var processor = new LuaXAstTreeCreator("");
            processor.ProcessBody(node, @class, method);
            method.Constants.Should().HaveCount(1);
            var v = method.Constants[0];
            v.Name.Should().Be("i");
            v.Value.ConstantTypeFull.IsString().Should().BeTrue();
            v.Value.Value.Should().Be("a");
        }

        [Fact]
        public void ConstantDeclaration_Fail_AlreadyExists()
        {
            const string tree = "[STATEMENTS[STATEMENT[CONST_DECLARATION[CONST_KW[const(const)]][IDENTIFIER(i)][ASSIGN[=(=)]][CONSTANT[STRING[STRINGDQ(\"a\")]]][EOS[;(;)]]]]]";
            var node = AstNodeExtensions.Parse(tree);
            var @class = new LuaXClass("a");
            var @method = new LuaXMethod(@class) { Name = "b" };
            @method.Constants.Add(new LuaXConstantVariable() { Name = "i" });
            var processor = new LuaXAstTreeCreator("");
            ((Action)(() => processor.ProcessBody(node, @class, method))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void ConstantDeclaration_Fail_VariableExists()
        {
            const string tree = "[STATEMENTS[STATEMENT[CONST_DECLARATION[CONST_KW[const(const)]][IDENTIFIER(i)][ASSIGN[=(=)]][CONSTANT[STRING[STRINGDQ(\"a\")]]][EOS[;(;)]]]]]";
            var node = AstNodeExtensions.Parse(tree);
            var @class = new LuaXClass("a");
            var @method = new LuaXMethod(@class) { Name = "b" };
            @method.Variables.Add(new LuaXVariable() { Name = "i" });
            var processor = new LuaXAstTreeCreator("");
            ((Action)(() => processor.ProcessBody(node, @class, method))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void ConstantDeclaration_Fail_ArgumentExists()
        {
            const string tree = "[STATEMENTS[STATEMENT[CONST_DECLARATION[CONST_KW[const(const)]][IDENTIFIER(i)][ASSIGN[=(=)]][CONSTANT[STRING[STRINGDQ(\"a\")]]][EOS[;(;)]]]]]";
            var node = AstNodeExtensions.Parse(tree);
            var @class = new LuaXClass("a");
            var @method = new LuaXMethod(@class) { Name = "b" };
            @method.Arguments.Add(new LuaXVariable() { Name = "i" });
            var processor = new LuaXAstTreeCreator("");
            ((Action)(() => processor.ProcessBody(node, @class, method))).Should().Throw<LuaXAstGeneratorException>();
        }
    }
}
