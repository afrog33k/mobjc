// Copyright (C) 2009 Jesse Jones
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

namespace MObjc.Helpers
{
	/// <summary>Used with <see cref = "ThreadModelAttribute">ThreadModelAttribute</see> to specify the thread
	/// safety of a type or member.</summary>
	[Serializable]
	public enum ThreadModel
	{
		/// <summary>The code may run safely only under the main thread (this is the default for
		/// code in the assemblies being checked).</summary>
		MainThread = 0x0000,
		
		/// <summary>The code may run under a single arbitrary thread.</summary>
		SingleThread = 0x0001,
		
		/// <summary>The code may run under multiple threads, but only if the execution is 
		/// serialized (e.g. by user level locking).</summary>
		Serializable = 0x0002,
		
		/// <summary>The code may run under multiple threads concurrently without user locking 
		/// (this is the default for code in the System/Mono namespaces).</summary>
		Concurrent = 0x0003,
		
		/// <summary>Or this with one of the other values for the rare cases where the code cannot be shown to 
		/// be correct using a static analysis.</summary>
		AllowEveryCaller = 0x0008,
	}
	
	/// <summary>Used with <see cref = "ThreadModel">ThreadModel</see> to specify the thread
	/// safety of a type or member.</summary>
	/// <remarks>The mono 2.5 version of <a href = "http://www.mono-project.com/Gendarme">Gendarme</a>
	/// will include a rule which will catch specification violations (such as concurrent code
	/// calling main thread code).</remarks>
	[Serializable]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct |
	AttributeTargets.Interface | AttributeTargets.Delegate |
	AttributeTargets.Method | AttributeTargets.Event | AttributeTargets.Property,
	AllowMultiple = false, Inherited = false)]
	public sealed class ThreadModelAttribute : Attribute
	{
		public ThreadModelAttribute(ThreadModel model)
		{
			Model = model;
		}
		
		public ThreadModel Model {get; set;}
	}
}
