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
using System.Reflection;

#if OLD
namespace MObjc
{
	// Loosely typed value retrieved from Objective-C. Note that this also
	// provides better error messages than "Cannot cast from source type
	// to destination type."
	[DisableRuleAttribute("D1041", "CircularReference")]
	public struct Untyped : IEquatable<Untyped>	// thread safe
	{
		internal Untyped(object value)
		{
			m_value = value;
		}
		
		public object Value
		{
			get {return m_value;}
		}
		
		public bool IsNull
		{
			get 
			{
				if (m_value == null)
					return true;
					
				else if (m_value.GetType() == typeof(IntPtr))							
					return (IntPtr) m_value == IntPtr.Zero;
					
				else
					return false;
			}
		}
		
		// These are provided so Call expressions can be chained.
		public Untyped Call(string selector, params object[] args)
		{
			return Call(new Selector(selector), args);
		}
	
		public Untyped Call(Selector selector, params object[] args)
		{			
			Untyped result = new Untyped(IntPtr.Zero);;
			
			if ((IntPtr) this != IntPtr.Zero)
			{
				if (selector.Name == "init" && args.Length == 0)
				{
					IntPtr exception = IntPtr.Zero;
					result = new Untyped(DirectCalls.Callp((IntPtr) this, sinit, ref exception));
					if (exception != IntPtr.Zero)
						CocoaException.Raise(exception);
				}
				else
				{
					using (Native invoke = new Native((IntPtr) this, selector))
					{
						invoke.SetArgs(args);			
						result = invoke.Invoke();
					}
				}
			}
			
			return result;
		}
	
		public override bool Equals(object rhsObj)
		{
			if (rhsObj == null)
				return false;
			
			if (GetType() != rhsObj.GetType()) 
				return false;
		
			Untyped rhs = (Untyped) rhsObj;                    
			return this == rhs;
		}
        
		public bool Equals(Untyped rhs)   
		{                    
			return this == rhs;
		}
	
		public static bool operator==(Untyped lhs, Untyped rhs)
		{
			if (object.ReferenceEquals(lhs.m_value, rhs.m_value))
				return true;
			
			if (lhs.m_value == null)
				return rhs.m_value == null;
        
        	return lhs.m_value.Equals(rhs.m_value);
		}
		
		public static bool operator!=(Untyped lhs, Untyped rhs)
		{
			return !(lhs == rhs);
		}
		
		public override int GetHashCode()
		{
			int hash = 47;
			
			unchecked
			{
				if (m_value != null)
					hash += 23*m_value.GetHashCode();
			}
			
			return hash;
		}
		
		public override string ToString()
		{
			return m_value != null ? m_value.ToString() : "null";
		}
    
		public static explicit operator Boolean(Untyped value) 
		{
			if (value.m_value == null)
				throw new InvalidCastException("Value is null");
				
			Type type = value.m_value.GetType();
			if (type == typeof(bool))							
				return (bool) value.m_value;
				
			throw new InvalidCastException("Can't cast from " + type + " to Boolean.");
		}
												
		public static explicit operator Byte(Untyped value) 
		{
			if (value.m_value == null)
				throw new InvalidCastException("Value is null");
				
			Type type = value.m_value.GetType();
			if (type == typeof(byte))							
				return (byte) value.m_value;
				
			throw new InvalidCastException("Can't cast from " + type + " to Byte.");
		}
												
		public static explicit operator Char(Untyped value) 
		{
			if (value.m_value == null)
				throw new InvalidCastException("Value is null");
				
			Type type = value.m_value.GetType();
			if (type == typeof(UInt16))							
				return (char) (UInt16) value.m_value;
		
			else if (type == typeof(Char))							
				return (char) value.m_value;
				
			throw new InvalidCastException("Can't cast from " + type + " to Char.");
		}
																								
		public static explicit operator Int16(Untyped value) 
		{
			if (value.m_value == null)
				throw new InvalidCastException("Value is null");
				
			Type type = value.m_value.GetType();
			if (type == typeof(byte))							
				return (Int16) (byte) value.m_value;
				
			else if (type == typeof(Int16))							
				return (Int16) value.m_value;
		
			else if (type == typeof(UInt16))							
				return (Int16) (UInt16) value.m_value;
				
			throw new InvalidCastException("Can't cast from " + type + " to Int16.");
		}
																								
		public static explicit operator UInt16(Untyped value) 
		{
			if (value.m_value == null)
				throw new InvalidCastException("Value is null");
				
			Type type = value.m_value.GetType();
			if (type == typeof(byte))							
				return (UInt16) (byte) value.m_value;
				
			else if (type == typeof(Int16))							
				return (UInt16) (Int16) value.m_value;
		
			else if (type == typeof(UInt16))							
				return (UInt16) value.m_value;
				
			else if (type == typeof(UInt32))				// need this for ffi's crappy short return type madness
			{
				return (UInt16) (UInt32) value.m_value;
			}
			
			throw new InvalidCastException("Can't cast from " + type + " to UInt16.");
		}
																								
		public static explicit operator Int32(Untyped value) 
		{
			if (value.m_value == null)
				throw new InvalidCastException("Value is null");
				
			Type type = value.m_value.GetType();
			if (type == typeof(byte))							
				return (Int32) (byte) value.m_value;
				
			else if (type == typeof(Int16))							
				return (Int32) (Int16) value.m_value;
		
			else if (type == typeof(UInt16))							
				return (Int32) (UInt16) value.m_value;
		
			else if (type == typeof(Int32))							
				return (Int32) value.m_value;
		
			else if (type == typeof(UInt32))							
				return (Int32) (UInt32) value.m_value;
		
			throw new InvalidCastException("Can't cast from " + type + " to Int32.");
		}
																								
		public static explicit operator UInt32(Untyped value) 
		{
			if (value.m_value == null)
				throw new InvalidCastException("Value is null");
				
			Type type = value.m_value.GetType();
			if (type == typeof(byte))							
				return (UInt32) (byte) value.m_value;
				
			else if (type == typeof(Int16))							
				return (UInt32) (Int16) value.m_value;
		
			else if (type == typeof(UInt16))							
				return (UInt32) (UInt16) value.m_value;
		
			else if (type == typeof(Int32))							
				return (UInt32) (Int32) value.m_value;
		
			else if (type == typeof(UInt32))							
				return (UInt32) value.m_value;
		
			throw new InvalidCastException("Can't cast from " + type + " to UInt32.");
		}
																								
		public static explicit operator Int64(Untyped value) 
		{
			if (value.m_value == null)
				throw new InvalidCastException("Value is null");
				
			Type type = value.m_value.GetType();
			if (type == typeof(byte))							
				return (Int64) (byte) value.m_value;
				
			else if (type == typeof(Int16))							
				return (Int64) (Int16) value.m_value;
		
			else if (type == typeof(UInt16))							
				return (Int64) (UInt16) value.m_value;
		
			else if (type == typeof(Int32))							
				return (Int64) (Int32) value.m_value;
		
			else if (type == typeof(UInt32))							
				return (Int64) (UInt32) value.m_value;
		
			else if (type == typeof(Int64))							
				return (Int64) value.m_value;
		
			else if (type == typeof(UInt64))							
				return (Int64) (UInt64) value.m_value;
		
			throw new InvalidCastException("Can't cast from " + type + " to Int64.");
		}
												
		public static explicit operator UInt64(Untyped value) 
		{
			if (value.m_value == null)
				throw new InvalidCastException("Value is null");
				
			Type type = value.m_value.GetType();
			if (type == typeof(byte))							
				return (UInt64) (byte) value.m_value;
				
			else if (type == typeof(Int16))							
				return (UInt64) (Int16) value.m_value;
		
			else if (type == typeof(UInt16))							
				return (UInt64) (UInt16) value.m_value;
		
			else if (type == typeof(Int32))							
				return (UInt64) (Int32) value.m_value;
		
			else if (type == typeof(UInt32))							
				return (UInt64) (UInt32) value.m_value;
		
			else if (type == typeof(Int64))							
				return (UInt64) (Int64) value.m_value;
		
			else if (type == typeof(UInt64))							
				return (UInt64) value.m_value;
		
			throw new InvalidCastException("Can't cast from " + type + " to UInt64.");
		}
												
		public static explicit operator Single(Untyped value) 
		{
			if (value.m_value == null)
				throw new InvalidCastException("Value is null");
				
			Type type = value.m_value.GetType();
			if (type == typeof(byte))							
				return (float) (byte) value.m_value;
				
			else if (type == typeof(Int16))							
				return (float) (Int16) value.m_value;
		
			else if (type == typeof(UInt16))							
				return (float) (UInt16) value.m_value;
		
			else if (type == typeof(Int32))							
				return (float) (Int32) value.m_value;
		
			else if (type == typeof(UInt32))							
				return (float) (UInt32) value.m_value;
				
			else if (type == typeof(float))							
				return (float) value.m_value;
		
			throw new InvalidCastException("Can't cast from " + type + " to Single.");
		}
												
		public static explicit operator Double(Untyped value) 
		{
			if (value.m_value == null)
				throw new InvalidCastException("Value is null");
				
			Type type = value.m_value.GetType();
			if (type == typeof(byte))							
				return (double) (byte) value.m_value;
				
			else if (type == typeof(Int16))							
				return (double) (Int16) value.m_value;
		
			else if (type == typeof(UInt16))							
				return (double) (UInt16) value.m_value;
		
			else if (type == typeof(Int32))							
				return (double) (Int32) value.m_value;
		
			else if (type == typeof(UInt32))							
				return (double) (UInt32) value.m_value;
		
			else if (type == typeof(Int64))							
				return (double) (Int64) value.m_value;
		
			else if (type == typeof(UInt64))							
				return (double) (UInt64) value.m_value;
		
			else if (type == typeof(float))							
				return (double) (float) value.m_value;
		
			else if (type == typeof(double))							
				return (double) value.m_value;
		
			throw new InvalidCastException("Can't cast from " + type + " to Double.");
		}
												
		public static explicit operator IntPtr(Untyped value) 
		{
			if (value.m_value == null)
				return IntPtr.Zero;
				
			Type type = value.m_value.GetType();
			if (type == typeof(IntPtr))							
				return (IntPtr) value.m_value;
				
			else if (typeof(NSObject).IsAssignableFrom(type))
				return (IntPtr) (NSObject) value.m_value;
		
			else if (type == typeof(Class))							
				return (IntPtr) (Class) value.m_value;
				
			else if (type == typeof(Selector))							
				return (IntPtr) (Selector) value.m_value;
				
			throw new InvalidCastException("Can't cast from " + type + " to IntPtr.");
		}
												
		public static explicit operator NSObject(Untyped value) 
		{
			if (value.m_value == null)
				return new NSObject(IntPtr.Zero);
				
			Type type = value.m_value.GetType();
			if (type == typeof(IntPtr))							
				return NSObject.Lookup((IntPtr) value.m_value);
				
			else if (typeof(NSObject).IsAssignableFrom(type))
				return (NSObject) value.m_value;
		
			else if (type == typeof(Class))							
				return (Class) value.m_value;
				
			throw new InvalidCastException("Can't cast from " + type + " to NSObject.");
		}
												
		public static explicit operator Class(Untyped value) 
		{
			if (value.m_value == null)
				throw new InvalidCastException("Value is null");
				
			Type type = value.m_value.GetType();
			if (type == typeof(IntPtr))							
				return new Class((IntPtr) value.m_value);
						
			else if (type == typeof(Class))							
				return (Class) value.m_value;
				
			throw new InvalidCastException("Can't cast from " + type + " to Class.");
		}
												
		public static explicit operator Selector(Untyped value) 
		{
			if (value.m_value == null)
				throw new InvalidCastException("Value is null");
				
			Type type = value.m_value.GetType();
			if (type == typeof(IntPtr))							
				return new Selector((IntPtr) value.m_value);

			else if (type == typeof(Selector))							
				return (Selector) value.m_value;
										
			throw new InvalidCastException("Can't cast from " + type + " to Selector.");
		}
												
		public static explicit operator String(Untyped value) 
		{
			if (value.m_value == null)
				throw new InvalidCastException("Value is null");
				
			Type type = value.m_value.GetType();
			if (type == typeof(string))							
				return (string) value.m_value;
						
			throw new InvalidCastException("Can't cast from " + type + " to String.");
		}
				
		public T To<T>() 			// can't have generic conversion operator...
		{
			// If the value is null then return some form of null.
			if (m_value == null || m_value.Equals(IntPtr.Zero))
				if (typeof(T).IsValueType)
					throw new InvalidCastException("Value is null");

				else if (typeof(T) == typeof(NSObject))
					return (T) (object) new NSObject(IntPtr.Zero);

				else if (typeof(NSObject).IsAssignableFrom(typeof(T)))
					return (T) Activator.CreateInstance(typeof(T), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, 
						new object[]{IntPtr.Zero}, null);

				else
					return default(T);
				
			// If T matches the value type then just cast value to that type.
			Type type = m_value.GetType();
			if (type == typeof(T))							
				return (T) m_value;
			
			// If T is NSObject, a derived class, or IntPtr then do a lookup and
			// return the correct managed class.
			if (type == typeof(IntPtr) || typeof(NSObject).IsAssignableFrom(typeof(T)))
			{
				IntPtr instance = IntPtr.Zero;
				if (type == typeof(IntPtr))							
					instance = (IntPtr) m_value;
				else
					instance = (IntPtr) (NSObject) m_value;
					
				NSObject obj = NSObject.Lookup(instance);
				if (typeof(T).IsAssignableFrom(obj.GetType()))
					return (T) (object) obj;
			}
			
			throw new InvalidCastException("Can't cast from " + type + " to " + typeof(T) + ".");
		}
																								
		// Need these for languages like VB that don't support operator overloading.
		public static Boolean ToBoolean(Untyped value)
		{
			return (Boolean) value;
		}
    
		public static Byte ToByte(Untyped value)
		{
			return (Byte) value;
		}
    
		public static Int16 ToInt16(Untyped value)
		{
			return (Int16) value;
		}
    
		public static UInt16 ToIntU16(Untyped value)
		{
			return (UInt16) value;
		}
    
		public static Int32 ToInt32(Untyped value)
		{
			return (Int32) value;
		}
    
		public static UInt32 ToUInt32(Untyped value)
		{
			return (UInt32) value;
		}
    
		public static Int64 ToInt64(Untyped value)
		{
			return (Int64) value;
		}
    
		public static UInt64 ToUInt64(Untyped value)
		{
			return (UInt64) value;
		}
    
		public static Single ToSingle(Untyped value)
		{
			return (Single) value;
		}
    
		public static Double ToDouble(Untyped value)
		{
			return (Double) value;
		}
    
		public static IntPtr ToIntPtr(Untyped value)
		{
			return (IntPtr) value;
		}
    
		public static NSObject ToNSObject(Untyped value)
		{
			return (NSObject) value;
		}
    
		public static Class ToClass(Untyped value)
		{
			return (Class) value;
		}
    
		public static Selector ToSelector(Untyped value)
		{
			return (Selector) value;
		}
    
		public static String ToString(Untyped value)
		{
			return (String) value;
		}
                
		private object m_value;
		private static readonly Selector sinit = new Selector("init");
	}
}
#endif