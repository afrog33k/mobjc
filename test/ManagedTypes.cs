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

[Register("_NSPoint")]
public struct NSPoint
{
	public float x;
	public float y;
}

[Register("_NSSize")]
public struct NSSize
{
	public float width;
	public float height;
}

[Register("_NSRect")]
public struct NSRect
{
	public NSPoint origin;
	public NSSize size;
}

[Register("_NSRange")]
public struct NSRange
{
	public int location;
	public int length;
}

[Register]
public class NSString : NSObject
{		
	public NSString(IntPtr value) : base(value)
	{
	}
	
	public static NSString stringWithString(string str)
	{
		return (NSString) new Class("NSString").Call("stringWithUTF8String:", str);
	}
	
	public override string ToString()
	{
		return (string) Call("UTF8String");
	}
}

[ExportClass("Subclass1", "NSObject", IVars = "myData")]
public class Subclass1 : NSObject
{
	public static Subclass1 makeDefault()
	{
		Subclass1 result = Subclass1.make(0);
		result.m_data = new IVar<NSString>(result, "myData");
		return result;
	}
		
	public static Subclass1 make(int v) 
	{
		Subclass1 result = (Subclass1) new Class("Subclass1").Call("alloc").Call("init");
		result.autorelease();
		result.m_value = v;
		result.m_data = new IVar<NSString>(result, "myData");
		
		return result;
	}
		
	public Subclass1(IntPtr instance) : base(instance)
	{
		m_data = new IVar<NSString>(this, "myData");
	}
		
	[NewMethod]		
	public NSString concat(NSString lhs, NSString rhs) 
	{
		return NSString.stringWithString(lhs.ToString() + rhs.ToString());
	}
	
	[NewMethod]		
	public Subclass1 initValue() 
	{
		m_value = 100;
		return this;
	}
	
	[NewMethod("getValue")]		
	public int GetValue() 
	{
		return m_value;
	}
		
	[NewMethod]		
	public void BadValue() 
	{
		throw new ArgumentException("my error", "alpha");
	}
		
	[NewMethod]		
	public int DiffRange(NSRange range) 
	{
		return range.location - range.length;
	}
		
	[NewMethod]		
	public NSRect GetRect() 
	{
		NSRect result = new NSRect();
		result.origin.x = 1.0f;
		result.origin.y = 2.0f;
		result.size.width = 3.0f;
		result.size.height = 4.0f;
		
		return result;
	}
		
	[NewMethod]		
	public Subclass1 Clone()
	{
		return Subclass1.make(m_value);
	}
		
	[NewMethod]		
	public NSObject MungeRect(NSRect r) 
	{
		NSObject str = (NSObject) new Class("NSMutableString").Call("alloc").Call("init");

		str.Call("appendString:", DoCreateStr(((int) r.origin.x).ToString()));
		str.Call("appendString:", DoCreateStr(((int) r.origin.y).ToString()));
		str.Call("appendString:", DoCreateStr(((int) r.size.width).ToString()));
		str.Call("appendString:", DoCreateStr(((int) r.size.height).ToString()));
		
		return str;
	}
	
	[NewMethod]		
	public int TakeUInt16(UInt16 value) 
	{
		return value;
	}
		
	[NewMethod]		
	public UInt16 TakeUInt162(UInt16 value) 
	{
		return (UInt16) (value + 10);
	}
		
	[NewMethod]		
	public int TakeChar(char value) 
	{
		return (int) value;
	}
		
	[NewMethod]		
	public string TakeString(string s) 
	{
		return s + s;
	}
		
	[NewMethod]		
	public int TakeBase(NSObject s) 
	{
		int r = (int) s.Call("getValue");
		return 3 * r;
	}
		
	[NewMethod]		
	public int TakeDerived(Subclass1 s) 
	{
		return 3 * s.GetValue();
	}
	
	public bool Dead
	{
		get {return m_dead;}
	}
		
	public string Data
	{
		get {return m_data.Value.ToString();}
		set {m_data.Value = NSString.stringWithString(value);}
	}
		
	protected override void OnDealloc()
	{
		m_dead = true;
		base.OnDealloc();
	}
	
	private NSObject DoCreateStr(string s)
	{
		Class nsString = new Class("NSMutableString");
		NSObject str = (NSObject) nsString.Call("alloc");
		return (NSObject) str.Call("initWithUTF8String:", s);
	}
		
	private int m_value;
	private bool m_dead;
	private IVar<NSString> m_data;
}

[ExportClass("PrettyData", "NSConcreteData")]
public class PrettyData : NSObject
{
	public static PrettyData makeDefault() 
	{
		PrettyData result = (PrettyData) new Class("PrettyData").Call("alloc").Call("init");
		result.autorelease();
		return result;
	}
		
	protected PrettyData(IntPtr instance) : base(instance)
	{
	}
		
	[NewMethod]		
	public int get33() 
	{
		return 33;
	}
	
	[OverrideMethod]		
	public new NSObject description() 
	{
		Class nsString = new Class("NSMutableString");
		NSObject str = (NSObject) nsString.Call("alloc");
		str = (NSObject) str.Call("initWithUTF8String:", "pretty: ");

		NSObject ss = (NSObject) SuperCall("description");
		str.Call("appendString:", ss);
	
		return str;
	}
}

[ExportClass("MyBase", "NSSimpleCString")]
public class MyBase : NSObject
{
	protected MyBase(IntPtr instance) : base(instance)
	{
	}
		
	[NewMethod]		
	public int get33() 				// new method
	{
		return 33;
	}
	
	[OverrideMethod]		
	public int integerValue() 		// NSString override
	{
		return 43;
	}
	
	[OverrideMethod]		
	public new NSObject description() 
	{
		Class nsString = new Class("NSMutableString");
		NSObject str = (NSObject) nsString.Call("alloc");
		str = (NSObject) str.Call("initWithUTF8String:", "base");
	
		return str;
	}
}

[ExportClass("MyDerived", "MyBase")]
public class MyDerived : MyBase
{
	protected MyDerived(IntPtr instance) : base(instance)
	{
	}
		
	[NewMethod]		
	public int get63() 				// new method
	{
		return 63;
	}
	
	[OverrideMethod]		
	public int intValue() 			// NSString override
	{
		return 73;
	}
	
	[OverrideMethod]			
	new public int integerValue()	// MyBase override 	
	{
		return 74;
	}
	
	[OverrideMethod]		
	public new NSObject description() 
	{
		Class nsString = new Class("NSMutableString");
		NSObject str = (NSObject) nsString.Call("alloc");
		str = (NSObject) str.Call("initWithUTF8String:", "derived");
	
		return str;
	}
		
	[NewMethod]		
	public NSObject TakeBase(MyBase s) 
	{
		return (NSObject) s.Call("description");
	}
		
	[NewMethod]		
	public NSObject TakeDerived(MyDerived s) 
	{
		return (NSObject) s.Call("description");
	}
}


