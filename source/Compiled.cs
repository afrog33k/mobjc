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
	internal abstract class CompiledExpr
	{
		// This is called (once) when the expression is first compiled.
		public void Init()
		{
			List<int> used = new List<int>();
			OnUsed(used);
			for (int i = 0; i < used.Count; ++i)
				if (used.IndexOf(i) < 0)
					throw new ArgumentException("Formatted call has gaps in the argument indices.");
		
			m_numArgs = used.Count;
		}
		
		// This will usually be called multiple times and may happen on
		// multiple threads.
		public Untyped Eval(params object[] args)
		{
			if (m_numArgs != args.Length)
				throw new InvalidCallException(string.Format("Expression takes {0} arguments but was called with {1} arguments", m_numArgs, args.Length));

			return OnEval(args);
		}
		
		public abstract Untyped OnEval(object[] args);

		public abstract void OnUsed(List<int> used);
				
		private int m_numArgs;
	}

	internal sealed class BoundExpr : CompiledExpr
	{
		public BoundExpr(Untyped value)
		{
			m_value = value;
		}
		
		public override Untyped OnEval(object[] args)
		{
			Unused.Arg(args);

			return m_value;
		}

		public override void OnUsed(List<int> used)
		{
		}
		
		private readonly Untyped m_value;
	}

	internal sealed class ArgumentExpr : CompiledExpr
	{
		public ArgumentExpr(int argIndex)
		{
			m_argIndex = argIndex;
		}
		
		public override Untyped OnEval(object[] args)
		{
			object o = args[m_argIndex];
			
			if (o != null && o.GetType() == typeof(Untyped))
				return (Untyped) o;
			else
				return new Untyped(o);
		}

		public override void OnUsed(List<int> used)
		{
			if (used.IndexOf(m_argIndex) < 0)
				used.Add(m_argIndex);
		}
		
		private readonly int m_argIndex;
	}

	internal sealed class CallNullaryExpr : CompiledExpr
	{
		public CallNullaryExpr(CompiledExpr target, Selector selector)
		{
			m_target = target;
			m_selector = selector;
		}
		
		public override Untyped OnEval(object[] inArgs)
		{
			Untyped target = m_target.OnEval(inArgs);
			
			Untyped result;
			using (Native native = new Native((IntPtr) target, m_selector))
			{
				result = native.Invoke();
			}
			
			return result;
		}

		public override void OnUsed(List<int> used)
		{
			m_target.OnUsed(used);
		}
		
		private readonly CompiledExpr m_target;
		private readonly Selector m_selector;
	}

	internal sealed class CallExpr : CompiledExpr
	{
		public CallExpr(CompiledExpr target, Selector selector, CompiledExpr[] operands)
		{
			m_target = target;
			m_selector = selector;
			m_operands = operands;
		}
		
		public override Untyped OnEval(object[] inArgs)
		{
			Untyped target = m_target.OnEval(inArgs);
			
			object[] args = new object[m_operands.Length];		// note that we can't use an m_args because of thread safety
			for (int i = 0; i < args.Length; ++i)
				args[i] = m_operands[i].OnEval(inArgs).Value;
				
			Untyped result;
			using (Native native = new Native((IntPtr) target, m_selector))
			{
				native.SetArgs(args);			
				result = native.Invoke();
			}
						
			return result;
		}

		public override void OnUsed(List<int> used)
		{
			m_target.OnUsed(used);
			
			foreach (CompiledExpr operand in m_operands)
				operand.OnUsed(used);
		}
		
		private readonly CompiledExpr m_target;
		private readonly Selector m_selector;
		private readonly CompiledExpr[] m_operands;
	}
	
	// It's kind of lame to provide alloc and init special cases but we need to do
	// something like this to ensure we don't make use of any auto-released objects
	// when executing "[[NSAutoreleasePool alloc] init]".
	internal sealed class CallAllocExpr : CompiledExpr
	{
		public CallAllocExpr(CompiledExpr target)
		{
			m_target = target;
		}
		
		public override Untyped OnEval(object[] inArgs)
		{
			Untyped target = m_target.OnEval(inArgs);
							
			IntPtr exception = IntPtr.Zero;
			IntPtr instance = DirectCalls.Callp((IntPtr) target, alloc, ref exception);
			if (exception != IntPtr.Zero)
				CocoaException.Raise(exception);
				
			return new Untyped(instance);
		}

		public override void OnUsed(List<int> used)
		{
			m_target.OnUsed(used);
		}
		
		private readonly CompiledExpr m_target;
		private static readonly Selector alloc = new Selector("alloc");
	}

	internal sealed class CallInitExpr : CompiledExpr
	{
		public CallInitExpr(CompiledExpr target)
		{
			m_target = target;
		}
		
		public override Untyped OnEval(object[] inArgs)
		{
			Untyped target = m_target.OnEval(inArgs);
							
			IntPtr exception = IntPtr.Zero;
			IntPtr instance = DirectCalls.Callp((IntPtr) target, init, ref exception);
			if (exception != IntPtr.Zero)
				CocoaException.Raise(exception);
				
			return new Untyped(instance);
		}

		public override void OnUsed(List<int> used)
		{
			m_target.OnUsed(used);
		}
		
		private readonly CompiledExpr m_target;
		private static readonly Selector init = new Selector("init");
	}
}
#endif
