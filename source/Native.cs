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
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MObjc
{
	[DisableRuleAttribute("S1003", "KeepAlive")]
	[DisableRuleAttribute("D1041", "CircularReference")]
	[DisableRuleAttribute("D1067", "PreferSafeHandle")]
	public sealed class Native : IDisposable
	{
		~Native()        
		{                    
			DoDispose(false);
		}

		public Native(IntPtr target, Selector selector) : this(target, selector, DoGetImp(target, selector, IntPtr.Zero), null)
		{
		}
												
		internal Native(IntPtr target, Selector selector, MethodSignature sig) : this(target, selector, DoGetImp(target, selector, IntPtr.Zero), sig)
		{
		}
												
		internal Native(IntPtr target, Selector selector, Class klass) : this(target, selector, DoGetImp(target, selector, (IntPtr) klass), null)
		{
		}
		
		internal Native(IntPtr target, Selector selector, IntPtr imp, MethodSignature sig)
		{
			Trace.Assert(selector != null, "selector is null");
			Trace.Assert(target == IntPtr.Zero || imp != IntPtr.Zero, "imp is null");
						
			m_target = target;

			if (m_target != IntPtr.Zero)
			{
				if (ms_stackFrames == null)
					ms_stackFrames = new Dictionary<MethodSignature, StackFrame>();	// note that we have to do this here so each thread gets its own dictionary
			
				m_selector = selector;
				m_imp = imp;
				m_sig = sig ?? new MethodSignature(target, (IntPtr) selector);
				
				if (!ms_stackFrames.TryGetValue(m_sig, out m_stackFrame))
				{
					m_stackFrame = new StackFrame(m_sig);
					ms_stackFrames.Add(m_sig, m_stackFrame);
				}
																			
				Unused.Value = Ffi.FillBuffer(m_stackFrame.ArgBuffers[0], target, "@", m_handles);
				Unused.Value = Ffi.FillBuffer(m_stackFrame.ArgBuffers[1], m_selector, ":", m_handles);
			}
		}
				
		public void SetArgs(params object[] args)
		{	
			Trace.Assert(args != null, "args is null");

			if (m_disposed)        
            	throw new ObjectDisposedException(GetType().Name);

			if (m_target != IntPtr.Zero)
			{	
				if (m_sig.GetNumArgs() != 2 + args.Length)
					throw new InvalidCallException(string.Format("{0} takes {1} arguments but was called with {2} arguments", m_selector, m_sig.GetNumArgs() - 2, args.Length));

				DoFreeBuffers();
				
				for (int i = 0; i < args.Length; ++i)
				{
					string encoding = m_sig.GetArgEncoding(i + 2);
					IntPtr buffer = Ffi.FillBuffer(m_stackFrame.ArgBuffers[i + 2], args[i], encoding, m_handles);
					if (buffer != IntPtr.Zero)
						m_buffers.Add(buffer);
				}
			}
		}
								
		public object Invoke()	
		{			
			if (m_disposed)        
            	throw new ObjectDisposedException(GetType().Name);

			object result = new NSObject(IntPtr.Zero);
			
			if (m_target != IntPtr.Zero)
			{
				Ffi.Call(m_stackFrame.Cif, m_imp, m_stackFrame.ResultBuffer, m_stackFrame.ArgBuffers);
				result = Ffi.DrainReturnBuffer(m_stackFrame.ResultBuffer, m_stackFrame.ReturnEncoding);
			}
			
			return result;
		}
		
		internal static object Call(IntPtr instance, string name, object[] args)	// thread safe
		{
			Trace.Assert(instance != IntPtr.Zero, "instance is zero");
			
			object result;
			if (name == "alloc" && args.Length == 0)	// need this so we can create an auto release pool without leaking NSMethodSignature
			{
				IntPtr exception = IntPtr.Zero;
				IntPtr ip = DirectCalls.Callp(instance, Selector.Alloc, ref exception);
				if (exception != IntPtr.Zero)
					CocoaException.Raise(exception);

				result = NSObject.Lookup(ip);
			}
			else
			{					
				Selector selector = new Selector(name);
				MethodSignature sig = new MethodSignature(instance, (IntPtr) selector);
			
				using (Native native = new Native(instance, selector, sig))
				{
					native.SetArgs(args);			
					result = native.Invoke();
				}
			}
			
			return result;
		}
																										
		public void Dispose()
		{
			DoDispose(true);
	
			GC.SuppressFinalize(this);
		}
		
		public override string ToString()
		{
			if (m_disposed)        
            	throw new ObjectDisposedException(GetType().Name);

			return m_sig.ToString();
		}

		#region Private Methods -----------------------------------------------
		private void DoDispose(bool disposing)
		{
			Unused.Value = disposing;
			
			if (!m_disposed)
			{
				if (m_target != IntPtr.Zero)
				{
					DoFreeBuffers();
				}
	
				m_disposed = true;
			}
		}
		
		private void DoFreeBuffers()
		{
			if (m_handles != null)
			{
				foreach (GCHandle handle in m_handles)	
				{
					handle.Free();
				}
				m_handles.Clear();
			}
			
			if (m_buffers != null)
			{
				foreach (IntPtr buffer in m_buffers)
				{
					Marshal.FreeHGlobal(buffer);
				}
				m_buffers.Clear();
			}
		}
		
		private static IntPtr DoGetImp(IntPtr target, Selector selector, IntPtr klass)
		{
			if (target == IntPtr.Zero)
				return IntPtr.Zero;

			if (selector == null)
				throw new ArgumentException("selector is null");
			
			if (klass == IntPtr.Zero)
				klass = object_getClass(target);		// have to use object_getClass instead of Class method so proxies work
			IntPtr method = class_getInstanceMethod(klass, (IntPtr) selector);

			if (method == IntPtr.Zero)
				method = class_getClassMethod(klass, (IntPtr) selector);
				
			if (method == IntPtr.Zero)
				throw new InvalidCallException(string.Format("Couldn't get the method for {0} {1}", new NSObject(target).Class, selector));

			IntPtr imp = method_getImplementation(method);

			if (imp == IntPtr.Zero)
				throw new InvalidCallException(string.Format("Couldn't get the method for {0} {1}", new NSObject(target).Class, selector));

			return imp;
		}
		#endregion
				
		#region Private Types -------------------------------------------------
		private struct StackFrame
		{
			public StackFrame(MethodSignature sig)
			{
				m_returnEncoding = sig.GetReturnEncoding();

				IntPtr resultType = Ffi.GetFfiType(m_returnEncoding);
				m_resultBuffer = Ffi.CreateBuffer(m_returnEncoding);

				int numArgs = sig.GetNumArgs();
				m_argBuffers = new PtrArray(numArgs);
				m_argTypes = new PtrArray(numArgs + 1);
				
				for (int i = 0; i < numArgs; ++i)
				{
					string encoding = sig.GetArgEncoding(i);
					m_argBuffers[i] = Ffi.CreateBuffer(encoding);
					m_argTypes[i] = Ffi.GetFfiType(encoding);
				}

				m_argTypes[numArgs] = IntPtr.Zero;

				m_cif = Ffi.AllocCif(resultType, m_argTypes);
			}
			
			public IntPtr Cif
			{
				get {return m_cif;}
			}
			
			public string ReturnEncoding
			{
				get {return m_returnEncoding;}
			}
			
			public IntPtr ResultBuffer
			{
				get {return m_resultBuffer;}
			}
			
			public PtrArray ArgBuffers
			{
				get {return m_argBuffers;}
			}
						
			private string m_returnEncoding;
			private IntPtr m_resultBuffer;
			private PtrArray m_argBuffers;
			private PtrArray m_argTypes;
			private IntPtr m_cif;
		}
		#endregion

		#region P/Invokes -----------------------------------------------------
		[DllImport("/usr/lib/libobjc.dylib")]
		private extern static IntPtr class_getInstanceMethod(IntPtr klass, IntPtr selector);

		[DllImport("/usr/lib/libobjc.dylib")]
		private extern static IntPtr class_getClassMethod(IntPtr klass, IntPtr selector);

		[DllImport("/usr/lib/libobjc.dylib")]
		private extern static IntPtr method_getImplementation(IntPtr method);

		[DllImport("/usr/lib/libobjc.dylib")]
		private extern static IntPtr object_getClass(IntPtr obj);
		#endregion

		#region Fields --------------------------------------------------------
		private IntPtr m_target;
		private Selector m_selector;
		private MethodSignature m_sig;
		private IntPtr m_imp;
//		private IntPtr m_cif;
		private bool m_disposed;
	
//		private string m_returnEncoding;
//		private IntPtr m_resultBuffer;
//		private PtrArray m_argTypes;
//		private PtrArray m_argBuffers;
		private StackFrame m_stackFrame;
		private List<GCHandle> m_handles = new List<GCHandle>();
		private List<IntPtr> m_buffers = new List<IntPtr>();

		[ThreadStatic]
		private static Dictionary<MethodSignature, StackFrame> ms_stackFrames;
		#endregion
	}
}
