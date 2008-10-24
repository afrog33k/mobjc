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
public class ExportTests
{
	[TestFixtureSetUp]
	public void Init()
	{
		Registrar.CanInit = true;
		m_pool = (NSObject) Native.Call("[[NSAutoreleasePool alloc] init]");
	}
	
	[TestFixtureTearDown]
	public void DeInit()
	{
		if (m_pool != null)
		{
			m_pool.Release();
			m_pool = null;
		}
	}
	
	[SetUp]
	public void Setup()
	{
		GC.Collect(); 				
		System.Threading.Thread.Sleep(200);	// give the finalizer enough time to run
	}
	
	[Test]
	public void NewTest() 
	{
		Subclass1 instance = new Subclass1();
	
		instance.Call("initValue");

		Untyped o = instance.Call("getValue");
		int value = (int) o;
		Assert.AreEqual(100, value);
	}

	[Test]
	public void CreateTest() 
	{	
		NSObject instance = (NSObject) Native.Call("[[Subclass1 alloc] init]");

		instance.Call("initValue");

		int value = (int) instance.Call("getValue");
		Assert.AreEqual(100, value);
	}
	
	[Test]
	public void ManagedExceptionTest() 
	{	
		NSObject instance = (NSObject) Native.Call("[[Subclass1 alloc] init]");

		try
		{
			instance.Call("BadValue");
		}
		catch (TargetInvocationException ie)
		{
			ArgumentException ae = (ArgumentException) ie.InnerException;
			Assert.AreEqual("alpha", ae.ParamName);
		}
	}
		
	[Test]
	public void OverrideTest() 
	{		
		NSObject data = (NSObject) Native.Call("[[PrettyData alloc] init]");

		int value = (int) data.Call("get33");
		Assert.AreEqual(33, value);

		NSObject istr = (NSObject) data.Call("description");
		string str = (string) istr.Call("UTF8String");
		Assert.AreEqual("pretty: <>", str);
	}
	
	[Test]
	[ExpectedException(typeof(InvalidCallException))]
	public void BadSelector() 
	{
		NSObject data = (NSObject) Native.Call("[[PrettyData alloc] init]");

		data.Call("initWithUTF8String::", "hey");
	}

	[Test]
	public void BaseTest() 
	{
		NSObject str = (NSObject) Native.Call("[[MyBase alloc] init]");

		// can call NSString methods
		bool b = (bool) str.Call("boolValue");
		Assert.IsFalse(b);

		// can call new MyBase methods
		int i = (int) str.Call("get33");
		Assert.AreEqual(33, i);

		// can call overriden NSString methods
		i = (int) str.Call("integerValue");
		Assert.AreEqual(43, i);
	}

	[Test]
	public void MyDerived() 
	{
		NSObject str = (NSObject) Native.Call("[[MyDerived alloc] init]");

		// can call NSString methods
		bool b = (bool) str.Call("boolValue");
		Assert.IsFalse(b);

		// can call MyBase methods
		int i = (int) str.Call("get33");
		Assert.AreEqual(33, i);

		// can call new MyDerived methods
		i = (int) str.Call("get63");
		Assert.AreEqual(63, i);

		// can call overriden NSString methods
		i = (int) str.Call("intValue");
		Assert.AreEqual(73, i);

		// can call overriden MyBase methods
		i = (int) str.Call("integerValue");
		Assert.AreEqual(74, i);
	}

	[Test]
	public void IVarTest() 
	{	
		NSObject instance = (NSObject) Native.Call("[[Subclass1 alloc] init]");

		// ivars start out null
		NSObject data = instance["myData"];
		Assert.IsTrue(data == null);
		
		// ivars can be set
		Class klass = new Class("NSString");
		NSObject str = (NSObject) klass.Call("stringWithUTF8String:", "hello");
		long count = str.RetainCount();
		
		instance["myData"] = str;
		
		// the ref count of the value should not change when it is
		// assigned to an ivar
		Assert.AreEqual(count, str.RetainCount());		

		// and we can get the value we set
		NSObject result = instance["myData"];
		Assert.AreEqual((IntPtr) str, (IntPtr) result);		
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void BadIVarGetTest() 
	{	
		NSObject instance = (NSObject) Native.Call("[[Subclass1 alloc] init]");

		NSObject data = instance["xxx"];
		Assert.IsTrue(data != null);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void BadIVarSetTest() 
	{	
		NSObject instance = (NSObject) Native.Call("[[Subclass1 alloc] init]");

		instance["xxx"] = instance;
	}

	[Test]
	public void StructTest() 
	{	
		NSObject instance = (NSObject) Native.Call("[[Subclass1 alloc] init]");
		
		NSRange range = new NSRange();
		range.location = 5;
		range.length = 3;
		
		int result = (int) instance.Call("DiffRange", range);
		Assert.AreEqual(result, 2);		
	}

	[Test]
	public void NestedStructTest1() 
	{	
		NSObject instance = (NSObject) Native.Call("[[Subclass1 alloc] init]");
		
		NSRect r = instance.Call("GetRect").To<NSRect>();
		Assert.AreEqual(1.0f, r.origin.x);		
		Assert.AreEqual(2.0f, r.origin.y);		
		Assert.AreEqual(3.0f, r.size.width);		
		Assert.AreEqual(4.0f, r.size.height);		
	}

	[Test]
	public void NestedStructTest2() 
	{	
		NSObject instance = (NSObject) Native.Call("[[Subclass1 alloc] init]");
		
		NSRect r = new NSRect();
		r.origin.x = 1.0f;
		r.origin.y = 2.0f;
		r.size.width = 3.0f;
		r.size.height = 4.0f;
		
		NSObject istr = (NSObject) instance.Call("MungeRect", r);
		string str = (string) istr.Call("UTF8String");
		Assert.AreEqual("1234", str);
	}

	[Test]
	public void IVarTest2() 
	{	
		Subclass1 instance = new Subclass1();
		
		// Can set ivars using IVar.
		instance.Data = "hey";
		NSString s = new NSString((IntPtr) instance["myData"]);
		Assert.AreEqual("hey", s.ToString());
		
		// Can get ivars using IVar.
		Assert.AreEqual("hey", instance.Data);
	}

	private NSObject m_pool;
}
