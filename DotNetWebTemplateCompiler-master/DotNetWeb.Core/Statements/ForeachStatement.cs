using DateTimeCompiler.Core;
using DotNetWeb.Core.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetWeb.Core.Statements
{
    public class ForeachStatement : Statement
    {
        public Id element { get; }
        public Id array { get; }
        public Statement block { get; }
        public ForeachStatement(Id element, Id array, Statement block)
        {
            this.element = element;
            this.array = array;
            this.block = block;
        }
        public override string Generate(int tabs)
        {
            throw new NotImplementedException();
        }

        public override void Interpret()
        {
            var method = EnvironmentManager.GetSymbolForEvaluation(Id.Token.Lexeme);
            if (method.Id.Token.Lexeme == "print")
            {
                InnerEvaluate(block);
            }
        }

        private void InnerEvaluate(Statement block)
        {
            throw new NotImplementedException();
        }

        public override void ValidateSemantic()
        {
            ValidateArguments(element, array, block);
        }

        private void ValidateArguments(Id element, Id array, Statement block)
        {
            if (element == null || array == null || block == null)
                return;

        }
    }
}
