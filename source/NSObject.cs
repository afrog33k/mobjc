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
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace MObjc
{	
	// http://developer.apple.com/documentation/Cocoa/Reference/Foundation/Protocols/NSObject_Protocol/Reference/NSObject.html#//apple_ref/doc/uid/20000052-BBCEBEIC
	[DisableRuleAttribute("S1003", "KeepAlive")]
	[DisableRuleAttribute("P1014", "InlineStaticInit")]
	[DisableRuleAttribute("D1035", "ImplicitCast")]
	[DisableRuleAttribute("P1012", "NotInstantiated")]
	[DisableRuleAttribute("C1026", "NoStaticRemove")]
	[DisableRuleAttribute("D1041", "CircularReference")]
	[DisableRule("D1007", "UseBaseTypes")]
	public partial class NSObject : IFormattable, IEquatable<NSObject>
	{		
		static NSObject()
		{
			Registrar.Init();
		}

		// Constructs a new managed instance using a native id. Note that there
		// may be an arbitrary number of managed instances wrapping the same native
		// id, except for the exported classes. For the exported classes NSObject
		// enforces a constraint that requires that each exported class id is
		// wrapped by no more than one managed instance.
		[DisableRuleAttribute("D1032", "UnusedMethod")]
		public NSObject(IntPtr instance)
		{
			m_instance = instance;				// note that it's legal to send messages to nil
			m_class = IntPtr.Zero;

			if (m_instance != IntPtr.Zero)
			{
				// It's a little inefficient to always grab this information, but it makes
				// dumping objects much easier because we can safely do it even when ref
				// counts are messed up.
				IntPtr exception = IntPtr.Zero;
				m_class = DirectCalls.Callp(m_instance, sclass, ref exception);
				if (exception != IntPtr.Zero)
					CocoaException.Raise(exception);
	
				m_baseClass = DirectCalls.Callp(m_class, ssuperclass, ref exception);
				if (exception != IntPtr.Zero)
					CocoaException.Raise(exception);
	
				ExportClassAttribute attr = Attribute.GetCustomAttribute(GetType(), typeof(ExportClassAttribute)) as ExportClassAttribute;
				if (attr != null)
				{
					lock (ms_instancesLock)
					{
						if (ms_instances.ContainsKey(instance))
							throw new InvalidOperationException(GetType() + " is being constructed twice with the same id.");
					
						ms_instances.Add(instance, this);
					}
				}
#if DEBUG
				ms_refs.Add(this);
#endif
			}
		}
				
		// Convenience method.
		public NSObject(Untyped instance) : this((IntPtr) instance) 
		{
		}
								
		// Note that the class of a class is itself.
		public Class Class
		{
			get {return new Class(m_class);}
		}
				
		public static NSObject Lookup(IntPtr instance)	// thread safe
		{
			NSObject managed = null;
			
			lock (ms_instancesLock)
			{
				if (!ms_instances.TryGetValue(instance, out managed))
				{
					// Nil is tricky to handle because we want to do things like convert
					// id's to NSObject subclasses when marshaling from native to managed
					// code. But we can't do this with nil because it has no class info.
					// So, rather than returning an NSObject which will break marshaling
					// we simply return null.
					if (instance == IntPtr.Zero)		
						return null;
											
					// If we land here then we have a native instance with no associated
					// managed instance. This will happen if the native instance isn't
					// an exported type, or it is exported and it's calling a managed
					// method for the first time.

					// First try to create an NSObject derived instance using the types
					// which were registered or exported.
					Type type = DoGetManagedClass(object_getClass(instance));
					if (type != null)
						managed = (NSObject) Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, 
							null, new object[]{instance}, null);

					// If we can't create a subclass then just create an NSObject.
					else
						managed = new NSObject(instance);
				}
			}
			
			return managed;
		}
									
		public static implicit operator IntPtr(NSObject value) 
		{				
			return value.m_instance;
		}
		
		// Need this for languages like VB that don't support operator overloading.
		public static IntPtr ToIntPtrType(NSObject value)
		{				
			return value.m_instance;
		}
    
		public static bool IsNullOrNil(NSObject o)
		{				
			return o == null || o.m_instance == IntPtr.Zero;
		}
		
		public bool IsNil()
		{				
			return m_instance == IntPtr.Zero;
		}

		// This is for testing. It will always be set correctly for exported classes
		// and for other classes where Release was used, but not if release was used.
		public bool IsDeallocated()
		{
			return m_deallocated;
		}
		
		public Untyped Call(string name, params object[] args)
		{
			DBC.Pre(name != null, "name is null");
			DBC.Assert(!m_deallocated, "ref count is zero");
			
			Untyped result = new Untyped(IntPtr.Zero);

			if (m_instance != IntPtr.Zero)
			{
				if (name == "alloc" && args.Length == 0)	// need this so we can create an auto release pool without leaking NSMethodSignature
				{
					IntPtr exception = IntPtr.Zero;
					result = new Untyped(DirectCalls.Callp(m_instance, salloc, ref exception));
					if (exception != IntPtr.Zero)
						CocoaException.Raise(exception);
				}
				else
				{					
					using (Native native = new Native(m_instance, new Selector(name)))
					{
						native.SetArgs(args);			
						result = native.Invoke();
					}
				}
			}
			
			return result;
		}
		
		public Untyped SuperCall(string name, params object[] args)
		{
			DBC.Pre(name != null, "name is null");
			DBC.Assert(!m_deallocated, "ref count is zero");
				
			Untyped result = new Untyped(IntPtr.Zero);

			if (m_instance != IntPtr.Zero)
			{
				using (Native native = new Native(m_instance, new Selector(name), Class.BaseClass))
				{
					native.SetArgs(args);			
					result = native.Invoke();
				}
			}
			
			return result;
		}
				
		public NSObject this[string ivarName]
		{
			get
			{
				DBC.Pre(!string.IsNullOrEmpty(ivarName), "ivarName is null or empty");
				DBC.Assert(!m_deallocated, "ref count is zero");
				
				NSObject result;
				
				if (m_instance != IntPtr.Zero)
				{					
					IntPtr value = IntPtr.Zero;
					IntPtr ivar = object_getInstanceVariable(m_instance, ivarName, ref value);
				
					if (ivar == IntPtr.Zero)
						throw new ArgumentException(ivarName + " isn't a valid instance variable");
						
					result = Lookup(value);
				}
				else
					result = new NSObject(IntPtr.Zero);
					
				return result;
			}
			
			set
			{
				DBC.Pre(!string.IsNullOrEmpty(ivarName), "ivarName is null or empty");
				DBC.Assert(!m_deallocated, "ref count is zero");
				
				if (m_instance != IntPtr.Zero)
				{
					IntPtr v = (IntPtr) value;
					IntPtr ivar = object_setInstanceVariable(m_instance, ivarName, v);
			
					if (ivar == IntPtr.Zero)
						throw new ArgumentException(ivarName + " isn't a valid instance variable");
				}
			}
		}
		
#if DEBUG
		// Returns a list of all NSObject's which are still alive. Note that some
		// of these may be deallocated.
		public static NSObject[] Snapshot()
		{
			return ms_refs.Snapshot();
		}
#endif
		
		public override bool Equals(object rhsObj)
		{
			if (rhsObj == null)           
				return false;
			
			NSObject rhs = rhsObj as NSObject;
			return this == rhs;
		}
			
		public bool Equals(NSObject rhs)   
		{
			return this == rhs;
		}
	
		[DisableRule("D1007", "UseBaseTypes")]
		public static bool operator==(NSObject lhs, NSObject rhs)
		{
			if (object.ReferenceEquals(lhs, rhs))
				return true;
			
			if ((object) lhs == null || (object) rhs == null)
				return false;
				
			if (lhs.IsDeallocated() && rhs.IsDeallocated())
				return object.ReferenceEquals(lhs, rhs);
			else if (lhs.IsDeallocated())
				return false;
			else if (rhs.IsDeallocated())
				return false;
			
			return (bool) lhs.Call("isEqual:", rhs);
		}
		
		public static bool operator!=(NSObject lhs, NSObject rhs)
		{
			return !(lhs == rhs);
		}
		
		public override int GetHashCode()
		{
			int hash = 47;
			
			unchecked
			{
				if (IsDeallocated())
					hash += base.GetHashCode();
				else
					hash += 23 * (int) Call("hash");
			}
			
			return hash;
		}
    
		internal static void Register(Type klass, string nativeName)
		{
			if (!ms_registeredClasses.ContainsKey(nativeName))
				ms_registeredClasses.Add(nativeName, klass);
			else
				throw new InvalidOperationException(nativeName + " has already been registered.");
		}
		
		internal IntPtr GetBaseClass()
		{
			return m_baseClass;
		}
		 						
		#region Protected Methods ---------------------------------------------
		// Only called for exported types. Derived classes must call the base class implementation.
		protected virtual void OnDealloc()
		{
			bool removed = ms_instances.Remove(m_instance);
			DBC.Assert(removed, "dealloc was called but the instance is not in ms_instances");

			Ignore.Value = SuperCall("dealloc");
			m_deallocated = true;
		}
		#endregion
				
		#region Private Methods -----------------------------------------------
		private static Type DoGetManagedClass(IntPtr klass)
		{
			while (klass != IntPtr.Zero)
			{
				IntPtr ptr = class_getName(klass);
				string name = Marshal.PtrToStringAnsi(ptr);
			
				Type type;
				if (Registrar.TryGetType(name, out type))
					return type;
				else if (ms_registeredClasses.TryGetValue(name, out type))
					return type;
					
				klass = class_getSuperclass(klass);
			}
			
			return null;
		}												
		#endregion
						
		#region P/Invokes -----------------------------------------------------
		[DllImport("/usr/lib/libobjc.dylib")]
		private extern static IntPtr object_getClass(IntPtr obj);

		[DllImport("/usr/lib/libobjc.dylib")]
		private extern static IntPtr class_getName(IntPtr klass);

		[DllImport("/usr/lib/libobjc.dylib")]
		private extern static IntPtr class_getSuperclass(IntPtr klass);

		[DllImport("/usr/lib/libobjc.dylib")]
		private extern static IntPtr object_getInstanceVariable(IntPtr obj, string name, ref IntPtr outValue);

		[DllImport("/usr/lib/libobjc.dylib")]
		private extern static IntPtr object_setInstanceVariable(IntPtr obj, string name, IntPtr value);
		#endregion
					
		#region Fields --------------------------------------------------------
		private IntPtr m_instance;
		private IntPtr m_class;
		private IntPtr m_baseClass;
		private bool m_deallocated;

		private static Dictionary<IntPtr, NSObject> ms_instances = new Dictionary<IntPtr, NSObject>();
		private static object ms_instancesLock = new object();
		private static Dictionary<string, Type> ms_registeredClasses = new Dictionary<string, Type>();
		
#if DEBUG
		private static WeakList<NSObject> ms_refs = new WeakList<NSObject>(64);
#endif

		private static readonly Selector salloc = new Selector("alloc");
		private static readonly Selector sclass = new Selector("class");
		private static readonly Selector srelease = new Selector("release");
		private static readonly Selector sretain = new Selector("retain");
		private static readonly Selector sretainCount = new Selector("retainCount");
		private static readonly Selector ssuperclass = new Selector("superclass");
		#endregion
	}
}
