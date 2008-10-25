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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace MObjc
{	
	public sealed class Managed
	{
		internal Managed(MethodInfo info, string encoding)
		{
			DBC.Pre(info != null, "info is null");
			DBC.Pre(!string.IsNullOrEmpty(encoding), "encoding is null or empty");
			
			m_info = info;
			m_signature = new MethodSignature(encoding);
		}
		
		// We default to logging exceptions thrown by managed code to stderr before
		// we convert them into a native exception. If you want to do something else
		// you can set this.
		public static Action<Exception> LogException {get; set;}
		
		internal IntPtr Call(IntPtr dummy, IntPtr resultBuffer, IntPtr argBuffers)	// thread safe
		{
			IntPtr exception = IntPtr.Zero;
	
			try
			{			
				// Get the this pointer for the method we're calling.
				PtrArray argArray = new PtrArray(argBuffers, m_signature.GetNumArgs() + 2);
				IntPtr target = (IntPtr) Ffi.DrainBuffer(argArray[0], "@");
				
				NSObject instance = NSObject.Lookup(target);
				if (instance == null)
					throw new ArgumentException("Couldn't find the C# object for " + target);	// shouldn't happen
				
				// Get the selector for the method that was called.
				// Get the method arguments.
				string encoding;
				object[] args = new object[m_signature.GetNumArgs() - 2];
				for (int i = 0; i < args.Length; ++i)
				{
					encoding = m_signature.GetArgEncoding(i + 2);
					if (encoding == "@")
						args[i] = new NSObject((IntPtr) Ffi.DrainBuffer(argArray[i + 2], encoding));
					else
						args[i] = Ffi.DrainBuffer(argArray[i + 2], encoding);
						
					// Native code doesn't distinguish between unsigned short and unichar
					// so we need to fix things up here.
					Type ptype = m_info.GetParameters()[i].ParameterType;
					if (ptype == typeof(char))
						args[i] = (char) (UInt16) args[i];
						
					else if (ptype != typeof(NSObject) && typeof(NSObject).IsAssignableFrom(ptype))
					{
						IntPtr ip;
						if (args[i].GetType() == typeof(NSObject))
							ip = (IntPtr) (NSObject) args[i];
						else
							ip = (IntPtr) args[i];

						args[i] = NSObject.Lookup(ip);
						if (args[i] == null && ip != IntPtr.Zero)
							throw new InvalidCallException(string.Format("Couldn't create a {0} when calling {1}. Is {0} registered or exported?", ptype, m_info.Name));
					}
						
					// Provide a better error message than the uber lame: "parameters".
					if (args[i] != null && !ptype.IsAssignableFrom(args[i].GetType()))
						throw new InvalidCallException(string.Format("Expected a {0} when calling {1} but have a {2}.", ptype, m_info.Name, args[i].GetType()));
				}
			
				// Call the method,
				object value = m_info.Invoke(instance, args);
				
				// and marshal the result.
				encoding = m_signature.GetReturnEncoding();
				if (encoding != "v" && encoding != "Vv")
				{
					if (ms_buffer != IntPtr.Zero)
						Marshal.FreeHGlobal(ms_buffer);
					
					ms_buffer = Ffi.FillBuffer(resultBuffer, value, encoding, null);
				}
			}
			catch (TargetInvocationException ie)
			{
				if (LogException != null)
					LogException(ie);
				else
					DoLogException(ie);
				exception = DoCreateNativeException(ie.InnerException);
			}
			catch (Exception e)
			{
				if (LogException != null)
					LogException(e);
				else
					DoLogException(e);
				exception = DoCreateNativeException(e);
			}
			
			return exception;
		}
		
		// Cocoa has a tendency to eat exceptions so in debug, at least, we'll write any 
		// exceptions thrown from managed code to stderr.
		[Conditional("DEBUG")]
		private static void DoLogException(Exception e)
		{
			Console.Error.WriteLine("Exception was thrown from managed code:");
			
			Exception ee = e;
			while (ee != null)
			{
				if (e.InnerException != null)
					Console.Error.WriteLine("-------- {0} Exception --------{1}", ee == e ? "Outer" : "Inner", Environment.NewLine);
				Console.Error.WriteLine("{0}", ee.Message + Environment.NewLine);
				Console.Error.WriteLine("{0}", ee.StackTrace + Environment.NewLine);

				ee = ee.InnerException;
			}
		}
		
		private static NSObject DoCreateNativeException(Exception e)
		{
			// Create the name, reason, and userInfo objects.
			NSObject name = (NSObject) Native.Call("[[NSString alloc] initWithUTF8String:{0}]", e.GetType().ToString());
			NSObject reason = (NSObject) Native.Call("[[NSString alloc] initWithUTF8String:{0}]", e.Message);			
			NSObject userInfo = (NSObject) Native.Call("[[NSMutableDictionary alloc] init]");
			
			// Add the original System.Exception to userInfo.
			NSObject key = (NSObject) Native.Call("[[NSString alloc] initWithUTF8String:\".NET exception\"]");
			try
			{
				MemoryStream stream = new MemoryStream();
				BinaryFormatter formatter = new BinaryFormatter();
				formatter.Serialize(stream, e);
			
				NSObject buffer = (NSObject) Native.Call("[NSData dataWithBytes:{0} length:{1}]", stream.ToArray(), (int) stream.Length);
				Ignore.Value = Native.Call("[{0} setObject:{1} forKey:{2}]", userInfo, buffer, key);
			}
			catch (Exception ee)
			{
				// Typically this will happen if e is not serializable.
				Console.Error.WriteLine(ee.Message);
				Console.Error.WriteLine(ee.StackTrace);
				Console.Error.Flush();
			}

			// Create the NSException.
			NSObject native = (NSObject) Native.Call("[NSException exceptionWithName:{0} reason:{1} userInfo:{2}]", name, reason, userInfo);
				
			return native;
		}
		
		#region Fields			
		private MethodInfo m_info;
		private MethodSignature m_signature;
		
		[ThreadStatic]
		private static IntPtr ms_buffer;
		#endregion
	}
}
