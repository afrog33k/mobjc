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

using MObjc.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace MObjc
{
	/// <summary>Lightweight object used to ensure that a reference to the delegate
	/// used by an Objective-C block does not disappear too early.</summary>
	[ThreadModel(ThreadModel.Concurrent)]
	public sealed class BlockCookie
	{
		/// <remarks>Name is used for debugging.</remarks>
		public BlockCookie(string name, Delegate callback)
		{
			Contract.Requires(!string.IsNullOrEmpty(name), "name is null or empty");
			
			Name = name;
			Block = new ExtendedBlock(callback);
			
			lock (ms_lock)
			{
				ms_cookies.Add(this);
			}
		}
		
		public string Name {get; private set;}
		
		public ExtendedBlock Block {get; private set;}
		
		/// <summary>Call this after the native code is done with the block.</summary>
		/// <remarks>For example, when using a sheet this would be called in the
		/// completion handler.</remarks>
		public void Free()
		{
			// Users may not bother getting rid of the reference to the cookie, but it's
			// important that we do get rid of the reference to the delegate so that the
			// object it is bound to can be GCed.
			Block = null;
			
			lock (ms_lock)
			{
				ms_cookies.Remove(this);
			}
		}
		
		/// <summary>mcocoa calls this on application exit to verify that BlockCookies are all
		/// properly freed.</summary>
		/// <remarks>Returns null if all cookies have been freed.</remarks>
		public static string[] InUse()
		{
			string[] names = null;
			
			lock (ms_lock)
			{
				if (ms_cookies.Count > 0)
					names = (from cookie in ms_cookies select cookie.Name).ToArray();
			}
			
			return names;
		}
		
		#region Fields
		private static object ms_lock = new object();
			private static HashSet<BlockCookie> ms_cookies = new HashSet<BlockCookie>();
		#endregion
	}
	
	/// <summary>Wrapper for an Objective-C block.</summary>
	/// <remarks>The block is added to the autorelease pool, but its lifetime can be
	/// extended by calling retain.</remarks>
	[ThreadModel(ThreadModel.Concurrent)]
	public sealed class ExtendedBlock
	{
		/// <remarks>The first argument of the delegate must be an IntPtr for the
		/// (normally implicit) block argument.</remarks>
		public ExtendedBlock(Delegate callback)
		{
			this.callback = callback;
			
			IntPtr fp = Marshal.GetFunctionPointerForDelegate(callback);
			this.block = NSObject.Lookup(CreateBlock(fp));
		}
		
		/// <summary>Returns true if the library was compiled against Snow Leopard
		/// or later and the application is running on Snow Leopard or later.</summary>
		public static bool HasBlocks()
		{
			return BlocksAreAvailable() != 0;
		}
		
		public readonly Delegate callback;	// need to keep this around so that it isn't GCed
		public readonly NSObject block;
		
		#region Private Methods
		[DllImport("mobjc-glue.dylib")]
		private extern static IntPtr CreateBlock(IntPtr callback);
		
		[DllImport("mobjc-glue.dylib")]
		private extern static byte BlocksAreAvailable();
		#endregion
	}
}
