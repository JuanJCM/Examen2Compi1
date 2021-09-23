using DotNetWeb.Core;
using DotNetWeb.Core.Expressions;
using DotNetWeb.Core.Interfaces;
using DotNetWeb.Core.Statements;
using System;

namespace DotNetWeb.Parser
{
    public class Parser : IParser
    {
        private readonly IScanner scanner;
        private Token lookAhead;
        public Parser(IScanner scanner)
        {
            this.scanner = scanner;
            this.Move();
        }
        public void Parse()
        {
            Program();
        }

        private Statement Program()
        {
            var stt1 = Init();
           var stt2 = Template();
            return new SequenceStatement(stt1, stt2);
        }

        private Statement Template()
        {
           var stt1 = Tag();
           var stt2 = InnerTemplate();
            return new SequenceStatement(stt1, stt2);
        }
        
        private Statement InnerTemplate()
        {
            if (this.lookAhead.TokenType == TokenType.LessThan)
            {
                return Template();
            }
            return null;
        }
        private void Tag()
        {
            Match(TokenType.LessThan);
            Match(TokenType.Identifier);
            Match(TokenType.GreaterThan);
            var stt = Stmts();
            Match(TokenType.LessThan);
            Match(TokenType.Slash);
            Match(TokenType.Identifier);
            Match(TokenType.GreaterThan);
            return stt;
        }

        private Statement Stmts()
        {
            if (this.lookAhead.TokenType == TokenType.OpenBrace)
            {
                var stt1 = Stmt();
                var stt2 = Stmts();
                return new SequenceStatement(stt1, stt2);
            }
            return null;
        }

        private Statement Stmt()
        {
            Match(TokenType.OpenBrace);
           
            switch (this.lookAhead.TokenType)
            {
                case TokenType.OpenBrace:
                    Match(TokenType.OpenBrace);
                    var stt = Eq();
                    Match(TokenType.CloseBrace);
                    Match(TokenType.CloseBrace);
                    return stt;
                case TokenType.Percentage:
                    IfStmt();
                    break;
                case TokenType.Hyphen:
                    ForeachStatement();
                    break;
                default:
                    throw new ApplicationException("Unrecognized statement");
                    return null;
            }
        }

        private Statement ForeachStatement()
        {
            Match(TokenType.Hyphen);
            Match(TokenType.Percentage);
            Match(TokenType.ForEeachKeyword);
            Match(TokenType.Identifier);
            Match(TokenType.InKeyword);
            Match(TokenType.Identifier);
            Match(TokenType.Percentage);
            Match(TokenType.CloseBrace);
            var stt= Template();
            Match(TokenType.OpenBrace);
            Match(TokenType.Percentage);
            Match(TokenType.EndForEachKeyword);
            Match(TokenType.Percentage);
            Match(TokenType.CloseBrace);
            return stt;
        }

        private Statement IfStmt()
        {
            Match(TokenType.Percentage);
            Match(TokenType.IfKeyword);
            var stt1 = Eq();
            Match(TokenType.Percentage);
            Match(TokenType.CloseBrace);
            var stt2 = Template();
            Match(TokenType.OpenBrace);
            Match(TokenType.Percentage);
            Match(TokenType.EndIfKeyword);
            Match(TokenType.Percentage);
            Match(TokenType.CloseBrace);
            return new SequenceStatement(stt1, stt2);
        }

        private Expression Eq()
        {
           var expr = Rel();
            while (this.lookAhead.TokenType == TokenType.Equal || this.lookAhead.TokenType == TokenType.NotEqual)
            {
                Move();
                expr = Rel();
                return expr;
            }
            return null;
        }

        private Statement Rel()
        {
            var stt = Expr();
            if (this.lookAhead.TokenType == TokenType.LessThan
                || this.lookAhead.TokenType == TokenType.GreaterThan)
            {
                Move();
                stt = Expr();
                return stt;
            }
            return null;
        }

        private Expression Expr()
        {
            var stt = Term();
            while (this.lookAhead.TokenType == TokenType.Plus || this.lookAhead.TokenType == TokenType.Hyphen)
            {
                Move();
                stt = Term();
                return stt;
            }
            return null;
        }

        private Expression Term()
        {
            var stt = Factor();
            while (this.lookAhead.TokenType == TokenType.Asterisk || this.lookAhead.TokenType == TokenType.Slash)
            {
                Move();
                 stt = Factor();
                return stt;
            }
            return null;
        }

        private Expression Factor()
        {
            switch (this.lookAhead.TokenType)
            {
                case TokenType.LeftParens:
                    {
                        Match(TokenType.LeftParens);
                        var expr = Eq();
                        Match(TokenType.RightParens);
                        return expr;
                    }
                case TokenType.IntConstant:
                    Match(TokenType.IntConstant);
                    break;
                case TokenType.FloatConstant:
                    Match(TokenType.FloatConstant);
                    break;
                case TokenType.StringConstant:
                    Match(TokenType.StringConstant);
                    break;
                case TokenType.OpenBracket:
                    Match(TokenType.OpenBracket);
                    var expr = ExprList();
                    Match(TokenType.CloseBracket);
                    return expr;
                default:
                    Match(TokenType.Identifier);
                    return null;
            }
        }

        private Statement ExprList()
        {
            var expr = Eq();
            if (this.lookAhead.TokenType != TokenType.Comma)
            {
                return null;
            }
            Match(TokenType.Comma);
            var expr2 = ExprList();
            new SequenceStatement(expr, expr2);
        }

        private Statement Init()
        {
            Match(TokenType.OpenBrace);
            Match(TokenType.Percentage);
            Match(TokenType.InitKeyword);
           var stt = Code();
            Match(TokenType.Percentage);
            Match(TokenType.CloseBrace);
            return stt;
        }

        private Statement Code()
        {
            Decls();
            return Assignations();
        }

        private Statement Assignations()
        {
            if (this.lookAhead.TokenType == TokenType.Identifier)
            {
                var stt1 = Assignation();
                var stt2 = Assignations();
                return new SequenceStatement(stt1, stt2);
            }
            return null;
        }

        private Statement Assignation()
        {
            Match(TokenType.Identifier);
            Match(TokenType.Assignation);
            var stt=Eq();
            Match(TokenType.SemiColon);
            return stt;
        }

        private void Decls()
        {
            Decl();
            InnerDecls();
        }

        private void InnerDecls()
        {
            if (this.LookAheadIsType())
            {
                Decls();
            }
        }

        private void Decl()
        {
            switch (this.lookAhead.TokenType)
            {
                case TokenType.FloatKeyword:
                    Match(TokenType.FloatKeyword);
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    break;
                case TokenType.StringKeyword:
                    Match(TokenType.StringKeyword);
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    break;
                case TokenType.IntKeyword:
                    Match(TokenType.IntKeyword);
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    break;
                case TokenType.FloatListKeyword:
                    Match(TokenType.FloatListKeyword);
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    break;
                case TokenType.IntListKeyword:
                    Match(TokenType.IntListKeyword);
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    break;
                case TokenType.StringListKeyword:
                    Match(TokenType.StringListKeyword);
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    break;
                default:
                    throw new ApplicationException($"Unsupported type {this.lookAhead.Lexeme}");
            }
        }

        private void Move()
        {
            this.lookAhead = this.scanner.GetNextToken();
        }

        private void Match(TokenType tokenType)
        {
            if (this.lookAhead.TokenType != tokenType)
            {
                throw new ApplicationException($"Syntax error! expected token {tokenType} but found {this.lookAhead.TokenType}. Line: {this.lookAhead.Line}, Column: {this.lookAhead.Column}");
            }
            this.Move();
        }

        private bool LookAheadIsType()
        {
            return this.lookAhead.TokenType == TokenType.IntKeyword ||
                this.lookAhead.TokenType == TokenType.StringKeyword ||
                this.lookAhead.TokenType == TokenType.FloatKeyword ||
                this.lookAhead.TokenType == TokenType.IntListKeyword ||
                this.lookAhead.TokenType == TokenType.FloatListKeyword ||
                this.lookAhead.TokenType == TokenType.StringListKeyword;

        }
    }
}
