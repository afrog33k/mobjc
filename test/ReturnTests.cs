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
using System.Runtime.InteropServices;

[TestFixture]
public class ReturnTests 	
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
	public void UInt16Test() 
	{
		Class klass = new Class("NSNumber");
		
		NSObject num = (NSObject) klass.Call("alloc");
		num = (NSObject) num.Call("initWithInt:", 5000);
		
		UInt16 result = (UInt16) num.Call("unsignedShortValue");
		Assert.AreEqual(5000, result);
	}
	
	[Test]
	public void UInt32Test() 
	{
		NSObject table = (NSObject) Native.Call("[[NSHashTable alloc] init]");
		NSObject value1 = (NSObject) Native.Call("[[NSString alloc] init]");
		NSObject value2 = (NSObject) Native.Call("[[NSDate alloc] init]");
	
		Untyped result = table.Call("addObject:", value1);
		Assert.IsTrue(result.IsNull);

		table.Call("addObject:", value2);		
		result = table.Call("count");

		Assert.AreEqual(typeof(uint), result.Value.GetType());
		Assert.AreEqual((uint) 2, result.Value);
	}
	
	[Test]
	public void VoidTest() 
	{
		NSObject table = (NSObject) Native.Call("[[NSHashTable alloc] init]");
		NSObject value = (NSObject) Native.Call("[[NSString alloc] init]");
	
		Untyped result = table.Call("addObject:", value);

		Assert.IsTrue(result.IsNull);
	}

	[Test]
	public void IntTest() 
	{
		NSObject str = (NSObject) new Class("NSString").Call("stringWithUTF8String:", "100");
		Assert.IsTrue(str != null);
	
		Untyped result = str.Call("intValue");

		Assert.AreEqual(typeof(int), result.Value.GetType());
		Assert.AreEqual(100, (int) result);
	}
	
	[Test]
	public void ObjectTest() 
	{
		NSObject table = (NSObject) Native.Call("[[NSHashTable alloc] init]");
		Untyped result = table.Call("copy");

		Assert.AreEqual(typeof(IntPtr), result.Value.GetType());
		Assert.AreEqual("NSConcreteHashTable", ((NSObject) result).Class.Name);
	}
	
	[Test]
	public void ClassTest() 
	{
		NSObject table = (NSObject) Native.Call("[[NSHashTable alloc] init]");
		Untyped result = table.Call("class");

		Assert.AreEqual(typeof(Class), result.Value.GetType());
		Assert.AreEqual("NSConcreteHashTable", ((Class) result).Name);
	}
	
	[Test]
	public void SelectorTest() 
	{
		NSObject table = (NSObject) Native.Call("[[NSHashTable alloc] init]");
		Selector selector = new Selector("addObject:");
		NSObject sig = (NSObject) table.Call("methodSignatureForSelector:", selector);
		
		Class klass = new Class("NSInvocation");
		NSObject invoke = (NSObject) klass.Call("invocationWithMethodSignature:", sig);
		
		invoke.Call("setSelector:", selector);
		Untyped result = invoke.Call("selector");

		Assert.AreEqual(typeof(Selector), result.Value.GetType());
		
		Selector s = (Selector) result;
		Assert.AreEqual("addObject:", s.Name);
	}
	
	[Test]
	public void CharsTest() 
	{
		Class klass = new Class("NSString");
		
		NSObject str = (NSObject) klass.Call("stringWithUTF8String:", "hey");
		Untyped result = str.Call("UTF8String");
		Assert.AreEqual(typeof(string), result.Value.GetType());		
		Assert.AreEqual("hey", (string) result);
		
		const char omega = '\x3AA';
		const char ellipsis = '\x2027';
		string u32 = new string(new char[]{omega, ellipsis});

		str = (NSObject) klass.Call("alloc").Call("initWithCharacters:length:", u32, 2);
		string s = (string) str.Call("UTF8String");
		Assert.AreEqual((int) omega, (int) s[0]);
		Assert.AreEqual((int) ellipsis, (int) s[1]);

		str = (NSObject) klass.Call("stringWithUTF8String:", u32);
		s = (string) str.Call("UTF8String");
		Assert.AreEqual((int) omega, (int) s[0]);
		Assert.AreEqual((int) ellipsis, (int) s[1]);
	}
	
	[Test]
	public void BoolTest2() 
	{
		Class klass = new Class("NSString");
		NSObject str1 = (NSObject) klass.Call("stringWithUTF8String:", "hey");
		NSObject str2 = (NSObject) klass.Call("stringWithUTF8String:", "hello");

		Untyped result = str1.Call("isEqual:", str2);

		Assert.AreEqual(typeof(bool), result.Value.GetType());		
		Assert.AreEqual(false, (bool) result);

		result = str1.Call("isEqual:", str1);

		Assert.AreEqual(typeof(bool), result.Value.GetType());		
		Assert.AreEqual(true, result.Value);
	}

	[Test]
	public void FloatTest() 
	{
		Class klass = new Class("NSString");
		NSObject str = (NSObject) klass.Call("stringWithUTF8String:", "3.14");
		
		float result = (float) str.Call("floatValue");
		Assert.IsTrue(Math.Abs(3.14 - result) < 0.01);
	}

	[Test]
	public void DoubleTest() 
	{
		Class klass = new Class("NSString");
		NSObject str = (NSObject) klass.Call("stringWithUTF8String:", "5.14");
		
		double result = (double) str.Call("doubleValue");
		Assert.IsTrue(Math.Abs(5.14 - result) < 0.01);
	}

	[Test]
	public void StructTest() 
	{
		Class klass = new Class("NSString");
		NSObject str = (NSObject) klass.Call("stringWithUTF8String:", "the quick brown fox");
		NSObject quick = (NSObject) klass.Call("stringWithUTF8String:", "quick");
		
		NSRange result = str.Call("rangeOfString:", quick).To<NSRange>();
		Assert.AreEqual(4, result.location);
		Assert.AreEqual(5, result.length);
	}
	
	[Test]
	public void LooseTest() 
	{
		NSObject table = (NSObject) Native.Call("[[NSHashTable alloc] init]");
		NSObject value1 = (NSObject) Native.Call("[[NSString alloc] init]");
		NSObject value2 = (NSObject) Native.Call("[[NSDate alloc] init]");
	
		table.Call("addObject:", value1);
		table.Call("addObject:", value2);		
		
		long i = (long) table.Call("count");

		Assert.AreEqual(2L, i);
	}

	[Test]
	public void DerivedClassCastTest() 
	{
		NSObject klass = new Class("Subclass1");
		NSObject o = (NSObject) klass.Call("alloc").Call("initValue");
	
		NSObject p = (NSObject) o.Call("Clone");
		Assert.IsTrue((IntPtr) o != (IntPtr) p);
		Assert.AreEqual(100, (int) p.Call("getValue"));
		
		Subclass1 q = o.Call("Clone").To<Subclass1>();
		Assert.IsTrue((IntPtr) o != (IntPtr) q);
		Assert.AreEqual(100, q.GetValue());
	}
		
	[Test]
	public void CharTest() 
	{
		Class klass = new Class("NSNumber");
		
		NSObject num = (NSObject) klass.Call("alloc");
		num = (NSObject) num.Call("initWithInt:", (int) 'x');
		
		char result = (char) num.Call("unsignedShortValue");
		Assert.AreEqual('x', result);
	}

	private NSObject m_pool;
}
