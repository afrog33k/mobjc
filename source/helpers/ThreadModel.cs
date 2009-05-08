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
	public enum ThreadModel
	{
		// The code may execute multiple threads concurrently without user locking 
		// (this is the default for code in the System/Mono namespaces).
		Concurrent = 0x0000,
		
		// The code may execute under multiple threads, but only if the execution is 
		// serialized (e.g. by user level locking).
		Serializable = 0x0001,
		
		// The code runs under an arbitrary thread, but cannot be used concurrently.
		SingleThread = 0x0002,
		
		// The code may run safely only under the main thread (this is the default 
		// for code outside the System/Mono namespaces).
		MainThread = 0x0003,
		
		// The code runs under a single thread with a user specified name.
		NamedThread = 0x0004,
		
		// Or this with the above for the rare cases where the code cannot be shown 
		// to be correct using a static analysis.
		AllowEveryCaller = 0x0008,
	}
	
	// This is used to precisely specify the threading semantics of code. Note that 
	// Gendarme's DecorateThreadsRule will catch problematic code which uses 
	// these attributes (such as concurrent code calling main thread code).
	[Serializable]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | 
	AttributeTargets.Interface | AttributeTargets.Delegate | 
	AttributeTargets.Method | AttributeTargets.Event | AttributeTargets.Property,
	AllowMultiple = false, Inherited = false)]
	public sealed class ThreadModelAttribute : Attribute
	{
		public ThreadModelAttribute(string name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentException("Name is null or empty.");
			
			Model = ThreadModel.NamedThread;
			Name = name;
		}
		
		public ThreadModelAttribute(ThreadModel model)
		{
			if (model == ThreadModel.NamedThread)
				throw new ArgumentException("Use the string constructor to decorate a named thread.");
			
			if (model == ThreadModel.AllowEveryCaller)
				throw new ArgumentException("Or AllowEveryCaller with one of the other enum values.");
			
			Model = model;
		}
		
		public string Name {get; set;}
		
		public ThreadModel Model {get; set;}
	}
}
