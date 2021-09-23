﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetWeb.Core
{
    public class Type : IEquatable<Type>
    {
        public string Lexeme { get; private set; }

        public TokenType TokenType { get; private set; }
        public Type(string lexeme, TokenType tokenType)
        {
            Lexeme = lexeme;
            TokenType = tokenType;
        }
        public bool Equals(Type other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Lexeme == other.Lexeme && TokenType == other.TokenType;
        }
        public static Type Int => new Type("int", TokenType.IntKeyword);
        public static Type Float => new Type("float", TokenType.FloatKeyword);
        public static Type String => new Type("string", TokenType.StringKeyword);
        public static Type Stringlist => new Type("stringlist", TokenType.StringListKeyword);
        public static Type Floatlist => new Type("floatlist", TokenType.FloatKeyword);
        public static Type Intlist => new Type("intlist", TokenType.IntKeyword);
        public static Type Bool => new Type("bool", TokenType.BoolKeyword);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((Type)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Lexeme, (int)TokenType);
        }

        public static bool operator ==(Type a, Type b) => a.Equals(b);

        public static bool operator !=(Type a, Type b) => !a.Equals(b);

        public override string ToString()
        {
            return Lexeme;
        }
    }
}
