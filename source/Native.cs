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
using System.Reflection;
using System.Runtime.InteropServices;

namespace MObjc
{
	[DisableRuleAttribute("S1003", "KeepAlive")]
	[DisableRuleAttribute("D1041", "CircularReference")]
	public sealed class Native : IDisposable
	{
		~Native()        
		{                    
			DoDispose(false);
		}

		public Native(IntPtr target, Selector selector) : this(target, selector, DoGetImp(target, selector, IntPtr.Zero))
		{
		}
												
		internal Native(IntPtr target, Selector selector, Class klass) : this(target, selector, DoGetImp(target, selector, (IntPtr) klass))
		{
		}
												
		internal Native(IntPtr target, Selector selector, IntPtr imp)
		{
			DBC.Pre(selector != null, "selector is null");
			DBC.Pre(target == IntPtr.Zero || imp != IntPtr.Zero, "imp is null");
						
			// Save the target, the method we need to call, and the method's signature.
			m_target = target;

			if (m_target != IntPtr.Zero)
			{
				m_selector = selector;
				m_imp = imp;
				m_sig = new MethodSignature(target, (IntPtr) selector);
				
				// Get ffi_type*'s and allocate buffers for the return and argument values.
				m_returnEncoding = m_sig.GetReturnEncoding();
				IntPtr resultType = Ffi.GetFfiType(m_returnEncoding);
				m_resultBuffer = Ffi.CreateBuffer(m_returnEncoding);
							
				int numArgs = m_sig.GetNumArgs();
				m_argTypes = new PtrArray(numArgs + 1);
				m_argBuffers = new PtrArray(numArgs);
				
				for (int i = 0; i < numArgs; ++i)
				{
					string encoding = m_sig.GetArgEncoding(i);
					m_argTypes[i] = Ffi.GetFfiType(encoding);
					m_argBuffers[i] = Ffi.CreateBuffer(encoding);
				}
				
				m_argTypes[numArgs] = IntPtr.Zero;
				
				Ignore.Value = Ffi.FillBuffer(m_argBuffers[0], target, "@", m_handles);
				Ignore.Value = Ffi.FillBuffer(m_argBuffers[1], m_selector, ":", m_handles);
	
				// Allocate the ffi_cif*.
				m_cif = Ffi.AllocCif(resultType, m_argTypes);
			}
		}
		
		public void SetArgs(params object[] args)
		{	
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
					IntPtr buffer = Ffi.FillBuffer(m_argBuffers[i + 2], args[i], encoding, m_handles);
					if (buffer != IntPtr.Zero)
						m_buffers.Add(buffer);
				}
			}
		}
								
		public Untyped Invoke()	
		{			
			if (m_disposed)        
            	throw new ObjectDisposedException(GetType().Name);

			Untyped result = new Untyped(IntPtr.Zero);
			
			if (m_target != IntPtr.Zero)
			{
				Ffi.Call(m_cif, m_imp, m_resultBuffer, m_argBuffers);

				object o = Ffi.DrainReturnBuffer(m_resultBuffer, m_returnEncoding);
			
				result = new Untyped(o);
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
			return m_sig.ToString();
		}

		#region Private Methods -----------------------------------------------
		private void DoDispose(bool disposing)
		{
			Unused.Arg(disposing);
			
			if (!m_disposed)
			{
				if (m_target != IntPtr.Zero)
				{
					m_argBuffers.Free();
					m_argTypes.FreeBuffer();
					DoFreeBuffers();
	
					if (m_resultBuffer != IntPtr.Zero)
						Marshal.FreeHGlobal(m_resultBuffer);
				
					Ffi.FreeCif(m_cif);
				}
	
				m_disposed = true;
			}
		}

		private void DoFreeBuffers()
		{
			foreach (GCHandle handle in m_handles)	
			{
				handle.Free();
			}
			m_handles.Clear();
			
			foreach (IntPtr buffer in m_buffers)
			{
				Marshal.FreeHGlobal(buffer);
			}
			m_buffers.Clear();
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
			{
				method = class_getClassMethod(klass, (IntPtr) selector);
			}
			if (method == IntPtr.Zero)
				throw new InvalidCallException(string.Format("Couldn't get the method for {0} {1}", new NSObject(target).Class, selector));

			IntPtr imp = method_getImplementation(method);

			if (imp == IntPtr.Zero)
				throw new InvalidCallException(string.Format("Couldn't get the method for {0} {1}", new NSObject(target).Class, selector));

			return imp;
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
		private IntPtr m_cif;
		private bool m_disposed = false;
	
		private string m_returnEncoding;
		private IntPtr m_resultBuffer;
		private PtrArray m_argTypes;
		private PtrArray m_argBuffers;
		private List<GCHandle> m_handles = new List<GCHandle>();
		private List<IntPtr> m_buffers = new List<IntPtr>();
		#endregion
	}
}
