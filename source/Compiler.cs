// Copyright (C) 2008 Jesse Jones
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;

#if OLD
namespace MObjc
{
	[DisableRule("C1026", "NoStaticRemove")]
	internal sealed class Compiler
	{
		public static CompiledExpr Lookup(string expr)
		{
			DBC.Pre(!string.IsNullOrEmpty(expr), "expr is null or empty");

			CompiledExpr compiled;
			
			lock (ms_lock)
			{
				if (!ms_cache.TryGetValue(expr, out compiled))
				{
					Compiler compiler = new Compiler(expr);
					compiled = compiler.DoCompile();
					compiled.Init();
					
					ms_cache[expr] = compiled;
				}
			}
			
			return compiled;
		}
												
		#region Private Methods
		private Compiler(string expr)
		{
			m_expr = expr;
		}
		
		private CompiledExpr DoCompile()
		{
			CompiledExpr expr;
			
			if (DoTryParse('['))
				expr = DoCompileExpr();
			else
				throw new ArgumentException(string.Format("Expected '[', but found \"{0}\".", m_expr.Substring(m_index)));
			
			DoSkipWhiteSpace();
			if (m_index < m_expr.Length)
				throw new ArgumentException(string.Format("Expected end of string, but found \"{0}\".", m_expr.Substring(m_index)));

			return expr;
		}
												
		// Expr := '[' Target (Name | Operand+) ']'
		private CompiledExpr DoCompileExpr()
		{						
			CompiledExpr target = DoParseTarget();
			CompiledExpr result = DoParseExprStem(target);

			DoParseChar(']');
			
			return result;
		}
		
		// Target := Arg | ClassName | Expr					
		private CompiledExpr DoParseTarget()
		{
			CompiledExpr result;
			
			if (DoTryParse('{'))
			{
				int index = DoParseNumber();
				DoParseChar('}');
				
				result = new ArgumentExpr(index);
			}	
			else if (char.IsLetter(DoPeek(0)) || DoPeek(0) == '_')			
			{
				string name = DoParseName();
				Class klass = new Class(name);			
				
				result = new BoundExpr(new Untyped((IntPtr) klass));
			}	
			else if (DoTryParse('['))
			{
				result = DoCompileExpr();
			}	
			else
				throw new ArgumentException(string.Format("Expected a target, but found \"{0}\".", m_expr.Substring(m_index)));
		
			return result;
		}
		
		// Name | Operand+
		// Operand := Name ':' (Arg | Literal | Expr)	name is part of a method name (e.g. "length:")
		private CompiledExpr DoParseExprStem(CompiledExpr target)
		{			
			string name = string.Empty;		
			List<CompiledExpr> operands = new List<CompiledExpr>();

			DoSkipWhiteSpace();
			while (char.IsLetter(DoPeek(0)) || DoPeek(0) == '_')
			{				
				name += DoParseName();				
				
				if (DoTryParse(':'))
				{
					name += ":";
					
					if (DoTryParse('{'))			
					{
						int index = DoParseNumber();
						DoParseChar('}');
					
						operands.Add(new ArgumentExpr(index));
					}	
					else if (DoTryParse('['))
					{
						operands.Add(DoCompileExpr());
					}
					else if (!DoTryParseLiteral(operands))
						throw new ArgumentException(string.Format("Expected an operand, but found \"{0}\".", m_expr.Substring(m_index)));

					DoSkipWhiteSpace();
				}
				else
					break;
			}
			
			if (name.Length == 0)
				throw new ArgumentException(string.Format("Expected a selector, but found \"{0}\".", m_expr.Substring(m_index)));
			
			
			CompiledExpr result;
			if (operands.Count == 0)
			{
				if (name == "alloc")
				{
					result = new CallAllocExpr(target);
				}
				else if (name == "init")
				{
					result = new CallInitExpr(target);
				}
				else
				{
					Selector selector = new Selector(name);
					result = new CallNullaryExpr(target, selector);
				}
			}
			else
			{
				Selector selector = new Selector(name);
				result = new CallExpr(target, selector, operands.ToArray());
			}
			
			return result;
		}
				
		// Literal := nil | NULL | 'YES' | 'NO' | Number | String
		private bool DoTryParseLiteral(List<CompiledExpr> operands)
		{			
			bool found = true;
			
			if (DoTryParse('{'))			
			{
				int index = DoParseNumber();
				DoParseChar('}');
			
				operands.Add(new ArgumentExpr(index));
			}	
			else if (DoTryParse("nil") || DoTryParse("NULL"))
			{
				operands.Add(new BoundExpr(new Untyped(IntPtr.Zero)));
			}
			else if (DoTryParse("YES") || DoTryParse("true"))
			{
				operands.Add(new BoundExpr(new Untyped(true)));
			}
			else if (DoTryParse("NO") || DoTryParse("false"))
			{
				operands.Add(new BoundExpr(new Untyped(false)));
			}
			else if (char.IsDigit(DoPeek(0)))
			{
				int value = DoParseNumber();
				operands.Add(new BoundExpr(new Untyped(value)));
			}
			else if (DoPeek(0) == '"')
			{
				string value = DoParseString();
				operands.Add(new BoundExpr(new Untyped(value)));
			}
			else
				found = false;
				
			return found;
		}
				
		// String := '"' [^"]* '"'
		private string DoParseString()
		{
			DoParseChar('"');

			int count = 0;
			while (true)
			{
				char ch = DoPeek(count);
				if (ch == '"')
					break;
					
				++count;
			}
						
			string str = m_expr.Substring(m_index, count);
			m_index += count;
			
			DoParseChar('"');

			return str;
		}

		// Name := [a-zA-Z_] [a-zA-Z0-9_]*
		private string DoParseName()
		{
			DoSkipWhiteSpace();

			int count = 0;
			if (char.IsLetter(DoPeek(0)) || DoPeek(0) == '_')
			{
				while (true)
				{
					char ch = DoPeek(count);
					if (!char.IsLetterOrDigit(ch) && ch != '_')
						break;
					
					++count;
				}
			}
			
			if (count == 0)
				if (m_index < m_expr.Length)
					throw new ArgumentException(string.Format("Expected a name, but found \"{0}\".", m_expr.Substring(m_index)));
				else
					throw new ArgumentException("Expected a name, not end of string.");
			
			string name = m_expr.Substring(m_index, count);
			m_index += count;
			
			return name;
		}

		// Number := [0-9]+
		private int DoParseNumber()
		{
			DoSkipWhiteSpace();

			int count = 0;
			while (true)
			{
				char ch = DoPeek(count);
				if (!char.IsDigit(ch))
					break;
					
				++count;
			}
			
			if (count == 0)
				if (m_index < m_expr.Length)
					throw new ArgumentException(string.Format("Expected a number, but found \"{0}\".", m_expr.Substring(m_index)));
				else
					throw new ArgumentException("Expected a number, not end of string.");
			
			int num = int.Parse(m_expr.Substring(m_index, count));
			m_index += count;
			
			return num;
		}

		private void DoParseChar(char ch)
		{
			DoSkipWhiteSpace();

			if (m_index < m_expr.Length)
				if (m_expr[m_index] == ch)
					++m_index;
				else
					throw new ArgumentException(string.Format("Expected '{0}', not \"{1}\".", ch, m_expr.Substring(m_index)));
			else
				throw new ArgumentException(string.Format("Expected '{0}', not end of string.", ch));
		}

		private void DoSkipWhiteSpace()
		{
			while (m_index < m_expr.Length && char.IsWhiteSpace(m_expr[m_index]))
				++m_index;
		}

		private bool DoTryParse(char ch)
		{
			DoSkipWhiteSpace();

			if (m_index < m_expr.Length && m_expr[m_index] == ch)
			{
				++m_index;
				return true;
			}
			else
				return false;
		}

		private bool DoTryParse(string text)
		{
			DoSkipWhiteSpace();

			for (int i = 0; i < text.Length; ++i)
			{
				if (m_index + i >= m_expr.Length || m_expr[m_index + i] != text[i])
					return false;
			}
			
			m_index += text.Length;
			
			return true;
		}

		private char DoPeek(int offset)
		{
			if (m_index + offset < m_expr.Length)
				return m_expr[m_index + offset];
			else
				return '\x0';
		}
		#endregion

		#region Fields
		private string m_expr;
		private int m_index;

		private static Dictionary<string, CompiledExpr> ms_cache = new Dictionary<string, CompiledExpr>();
		private static object ms_lock = new object();
		#endregion
	}
}
#endif
