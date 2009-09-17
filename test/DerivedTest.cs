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

public class ManagedString : NSString
{
	public ManagedString(IntPtr value) : base(value)
	{
	}
	
	public void shouldNotBeRegistered()
	{
	}
}

public class ManagedPretty : PrettyData
{
	public ManagedPretty(IntPtr instance) : base(instance)
	{
	}
	
	public void shouldNotBeRegistered()
	{
	}
}

[TestFixture]
public class DerivedTest
{
	[TestFixtureSetUp]
	public void Init()
	{
		Registrar.CanInit = true;
		m_pool = new NSObject(NSObject.AllocAndInitInstance("NSAutoreleasePool"));
	}
	
	[TestFixtureTearDown]
	public void DeInit()
	{
		if (m_pool != null)
		{
			m_pool.release();
			m_pool = null;
		}
	}
	
	[Test]
	public void Registered()
	{
		Class klass = new Class("NSString");
		Assert.AreEqual("NSString", klass.Name);
		
		klass = new Class("PrettyData");
		Assert.AreEqual("PrettyData", klass.Name);
	}
	
	[Test]
	public void SuperCall1()
	{
		try
		{
			MyBase a = NSObject.AllocAndInitInstance("MyBase").To<MyBase>();
			
			int result = a.accumulate(0);
			Assert.AreEqual(2, result);
		}
		catch (Exception e)
		{
			Console.WriteLine("SuperCall1 failed:");
			Console.WriteLine("{0}", e);
		}
	}
	
	[Test]
	public void SuperCall2()
	{
		try
		{
			MyDerived b = NSObject.AllocAndInitInstance("MyDerived").To<MyDerived>();
			
			int result = b.accumulate(0);
			Assert.AreEqual(5, result);
		}
		catch (Exception e)
		{
			Console.WriteLine("SuperCall2 failed:");
			Console.WriteLine("{0}", e);
		}
	}
	
	[Test]
	public void SuperCall3()
	{
		try
		{
			DerivedSquared c = NSObject.AllocAndInitInstance("DerivedSquared").To<DerivedSquared>();
			
			int result = c.accumulate(0);
			Assert.AreEqual(5, result);
		}
		catch (Exception e)
		{
			Console.WriteLine("SuperCall3 failed:");
			Console.WriteLine("{0}", e);
		}
	}
	
	[Test]
	public void SuperCall4()
	{
		try
		{
			MyBase a = NSObject.AllocAndInitInstance("MyBase").To<MyBase>();
			
			a.SuperCall(NSObject.Class, "accumulate:", 5);
			Assert.Fail("SuperCall should have thrown an exception");
		}
		catch (InvalidCallException)
		{
		}
	}
	
	[Test]
	public void SuperCall5()
	{
		MyDerived a = NSObject.AllocAndInitInstance("MyDerived").To<MyDerived>();
		
		int result = a.SuperCall(MyBase.Class, "accumulate:", 5).To<int>();
		Assert.AreEqual(2, result);
	}
	
	[Test]
	public void BadSuperCall()
	{
		try
		{
			MyBase a = NSObject.AllocAndInitInstance("MyBase").To<MyBase>();
			a.badAccumulate(0);
			Assert.Fail("badAccumulate should have thrown an exception");
		}
		catch (InvalidCallException i)
		{
			if (!i.Message.Contains("NSSimpleCString"))
				Assert.Fail("Expected 'NSSimpleCString' in '{0}", i.Message);
			
			if (!i.Message.Contains("badAccumulate"))
				Assert.Fail("Expected 'badAccumulate' in '{0}", i.Message);
		}
		catch (Exception e)
		{
			Assert.Fail("badAccumulate should have thrown an InvalidCallException, not a {0}", e.GetType());
		}
	}
	
	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void ManagedStringIsNotRegistere()
	{
		new Class("ManagedString");
	}
	
	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void ManagedPrettyIsNotRegistere()
	{
		new Class("ManagedPretty");
	}
	
	private NSObject m_pool;
}
