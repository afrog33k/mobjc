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

using NUnit.Framework;
using MObjc;
using System;
using System.Reflection;
using System.Runtime.InteropServices;

[TestFixture]
public class InstanceTests
{		
	[SetUp]
	public void Setup()
	{
		Registrar.CanInit = true;
		GC.Collect(); 				
		System.Threading.Thread.Sleep(200);	// give the finalizer enough time to run
	}
	
	[Test]
	public void RefCount1Test()	
	{
		NSObject pool = (NSObject) new Class("NSAutoreleasePool").Call("alloc").Call("init");

		// If we use alloc the object will have a ref count of one.
		NSObject instance = (NSObject) new Class("NSHashTable").Call("alloc").Call("init");
		Assert.AreEqual(1L, instance.retainCount());

		// Classes always have a very high retain count (because they
		// are not supposed to go away).
		Class nsSignature = new Class("NSMethodSignature");
		Assert.IsTrue(nsSignature.retainCount() > 1000);

		// If alloc, new, or copy aren't used then the pool owns the object.
		Class nsString = new Class("NSString");
		NSObject str = (NSObject) nsString.Call("stringWithUTF8String:", "hello");
		Assert.AreEqual(1L, str.retainCount());

		// We can have two managed instances on the same native instance
		// and the ref count doesn't change.
		NSObject copy = new NSObject((IntPtr) instance);	
		Assert.AreEqual(1L, copy.retainCount());
		
		// If we send a message to an object its retain count doesn't change.
		instance.Call("description");
		Assert.AreEqual(1L, instance.retainCount());
				
		pool.release();
		
		// Verify our counts after we empty the release pool.
		Assert.AreEqual(1L, instance.retainCount());
		Assert.AreEqual(1L, copy.retainCount());
	}

	[Test]
	public void RefCount2Test()
	{
		NSObject pool = (NSObject) new Class("NSAutoreleasePool").Call("alloc").Call("init");

		// No copy, new, or alloc so ref count is one and it's owned by the pool.
		PrettyData direct = new PrettyData();
		Assert.AreEqual(1L, direct.retainCount());

		// Alloc so pool has no ownership stake.
		NSObject indirect = (NSObject) new Class("PrettyData").Call("alloc").Call("init");
		Assert.AreEqual(1L, indirect.retainCount());

		// If we send a message to an object its retain count doesn't change.
		int value = (int) direct.Call("get33");
		Assert.AreEqual(33, value);
		Assert.AreEqual(1L, direct.retainCount());

		pool.release();
		
		// Verify our counts after we empty the release pool.
		Assert.IsTrue(direct.IsDeallocated());
		Assert.AreEqual(1L, indirect.retainCount());
	}

	[Test]
	public void RefCount3Test()	
	{
		NSObject pool = (NSObject) new Class("NSAutoreleasePool").Call("alloc").Call("init");

		Class klass = new Class("NSHashTable");
		NSObject instance1 = new NSObject(klass.Call("alloc").Call("init"));
		Assert.AreEqual(1L, instance1.retainCount());
		
		NSObject instance2 = new NSObject(new Class("NSHashTable").Call("alloc").Call("init"));
		Assert.AreEqual(1L, instance2.retainCount());
		
		pool.release();
		
		Assert.AreEqual(1L, instance1.retainCount());
		Assert.AreEqual(1L, instance2.retainCount());
	}

	[Test]
	public void ChainedCallTest()
	{
		NSObject pool = (NSObject) new Class("NSAutoreleasePool").Call("alloc").Call("init");

		Class nsString = new Class("NSMutableString");
		NSObject str = (NSObject) nsString.Call("alloc").Call("initWithUTF8String:", "chained!");

		Untyped result = str.Call("UTF8String");
		Assert.AreEqual("chained!", result.Value);

		pool.release();
	}

	[Test]
	public void NilCallTest()
	{
		NSObject pool = (NSObject) new Class("NSAutoreleasePool").Call("alloc").Call("init");

		NSObject nil = new NSObject(IntPtr.Zero);
		
		// Calling an NSObject method on nil does nothing and returns nil.
		Untyped result = nil.Call("hash");
		Assert.AreEqual(typeof(IntPtr), result.Value.GetType());
		Assert.AreEqual(IntPtr.Zero, (IntPtr) result);

		// Calling a unknown method on nil does nothing and returns nil.
		result = nil.Call("foo");
		Assert.AreEqual(typeof(IntPtr), result.Value.GetType());
		Assert.AreEqual(IntPtr.Zero, (IntPtr) result);

		// Can chain calls to nil.
		result = nil.Call("foo").Call("bar");
		Assert.AreEqual(typeof(IntPtr), result.Value.GetType());
		Assert.AreEqual(IntPtr.Zero, (IntPtr) result);

		// Can use Native with null.
		result = nil.Call("foo");
		Assert.AreEqual(typeof(IntPtr), result.Value.GetType());
		Assert.AreEqual(IntPtr.Zero, (IntPtr) result);

		pool.release();
	}

	[Test]
	public void ThreadedTest()
	{
		NSObject pool = (NSObject) new Class("NSAutoreleasePool").Call("alloc").Call("init");

		bool threaded = (bool) new Class("NSThread").Call("isMultiThreaded");
		Assert.IsTrue(threaded);

		pool.release();
	}

	[Test]
	public void DeallocTest()
	{
		NSObject pool = (NSObject) new Class("NSAutoreleasePool").Call("alloc").Call("init");

		Subclass1 instance = new Subclass1((IntPtr) new Class("Subclass1").Call("alloc").Call("init"));
		Assert.AreEqual(1L, instance.retainCount());
		instance.release();
		Assert.IsTrue(instance.Dead);
		
		pool.release();
	}
}
