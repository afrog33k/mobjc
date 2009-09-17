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
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace MObjc
{
	/// <summary>Base class for all Cocoa classes.</summary>
	/// <remarks>They don't appear in the documentation but this class exposes all of the
	/// standard <a href = "http://developer.apple.com/documentation/Cocoa/Reference/Foundation/Classes/NSObject_Class/Reference/Reference.html">NSObject</a> 
	/// methods except that the only perform method exposed is performSelectorOnMainThread:withObject:waitUntilDone:.
	/// The rest may be added in the future, but it's much easier to use mcocoa's NSApplication::BeginInvoke
	/// method to do this sort of thing.</remarks>
	[ThreadModel(ThreadModel.Concurrent)]
	public partial class NSObject : IFormattable, IEquatable<NSObject>
	{
		static NSObject()
		{
			try
			{
				Registrar.Init();
				ms_class = new Class("NSObject");
			}
			catch (Exception e)
			{
				// This is a fatal error and the nunit which ships with mono does a poor
				// job printing inner exceptions. So, we'll print the exception ourself
				// and shutdown the app.
				Console.Error.WriteLine("Registrar.Init raised an exception:");
				Console.Error.WriteLine("{0}", e);
				Console.Out.Flush();
				Console.Error.Flush();
				Environment.Exit(13);
			}
		}
		
#if DEBUG
		/// <summary>Debug method to enable recording stack traces for all NSObjects.</summary>
		/// <remarks>ToString(&quot;S&quot;) will print the traces.</remarks>
		public static bool SaveStackTraces {get; set;}
		
		private string[] StackTrace {get; set;}
#endif
		
		/// <summary>Constructs a managed object which is associated with an unmanaged object.</summary>
		/// <remarks>In general multiple NSObject instances can be associated with the same unmanaged object.
		/// The exception is that only one <see cref = "ExportClassAttribute">ExportClassAttribute</see> object can be associated with an
		/// unmanaged object. If an attempt is made to construct two NSObjects pointing to the same
		/// exported object an exception will be thrown. (The reason for this is that exported instances may
		/// have managed state which should be associated with one and only one unmanaged instance).</remarks>
		public NSObject(IntPtr instance)
		{
			m_instance = instance;				// note that it's legal to send messages to nil
			m_class = IntPtr.Zero;
			
			if (m_instance != IntPtr.Zero)
			{
				// It's a little inefficient to always grab this information, but it makes
				// dumping objects much easier because we can safely do it even when ref
				// counts are zero.
				IntPtr exception = IntPtr.Zero;
				m_class = DirectCalls.Callp(m_instance, Selector.Class, ref exception);
				if (exception != IntPtr.Zero)
					CocoaException.Raise(exception);
				
				m_baseClass = DirectCalls.Callp(m_class, Selector.SuperClass, ref exception);
				if (exception != IntPtr.Zero)
					CocoaException.Raise(exception);
				
#if DEBUG
				if (SaveStackTraces)
				{
					var stack = new System.Diagnostics.StackTrace(1);
					StackTrace = new string[stack.FrameCount];
					for (int i = 0; i < stack.FrameCount; ++i)	// TODO: provide a MaxStackDepth method?
					{
						StackFrame frame = stack.GetFrame(i);
						
						if (!string.IsNullOrEmpty(frame.GetFileName()))
							StackTrace[i] = string.Format("{0}|{1} {2}:{3}", frame.GetMethod().DeclaringType, frame.GetMethod(), frame.GetFileName(), frame.GetFileLineNumber());
						else
							StackTrace[i] = string.Format("{0}|{1}", frame.GetMethod().DeclaringType, frame.GetMethod());
					}
				}
#endif

				lock (ms_instancesLock)
				{
					bool exported;
					Type type = GetType();
					if (!ms_exports.TryGetValue(type, out exported))	// GetCustomAttribute turns out to be very slow
					{
						ExportClassAttribute attr = Attribute.GetCustomAttribute(type, typeof(ExportClassAttribute)) as ExportClassAttribute;
						exported = attr != null;
						ms_exports.Add(type, exported);
					}
					
					if (exported)
					{
						if (ms_instances.ContainsKey(instance))
							throw new InvalidOperationException(type + " is being constructed twice with the same id.");
						
						ms_instances.Add(instance, this);
					}
				}
#if DEBUG
				ms_refs.Add(this);
#endif
			}
		}
		
		/// <summary>Creates a new uninitialized unmanaged object with a reference count of one.</summary>
		/// <param name = "name">The class name, e.g. "MyView".</param>
		/// <remarks>This is commonly used with exported types which can be created via managed code 
		/// (as opposed to via Interface Builder).</remarks>
		public static IntPtr AllocInstance(string name)
		{
			Class klass = new Class(name);
			
			IntPtr exception = IntPtr.Zero;
			IntPtr instance = DirectCalls.Callp(klass, Selector.Alloc, ref exception);
			if (exception != IntPtr.Zero)
				CocoaException.Raise(exception);
			
			return instance;
		}
		
		/// <summary>Creates a new default initialized unmanaged object with a reference count of one.</summary>
		/// <param name = "name">The class name, e.g. "MyView".</param>
		/// <remarks>This is commonly used with exported types which can be created via managed code 
		/// (as opposed to via Interface Builder).</remarks>
		public static IntPtr AllocAndInitInstance(string name)
		{
			Class klass = new Class(name);
			
			IntPtr exception = IntPtr.Zero;
			IntPtr instance = DirectCalls.Callp(klass, Selector.Alloc, ref exception);
			if (exception != IntPtr.Zero)
				CocoaException.Raise(exception);
			
			instance = DirectCalls.Callp(instance, Selector.Init, ref exception);
			if (exception != IntPtr.Zero)
				CocoaException.Raise(exception);
			
			return instance;
		}
		
		/// <summary>Increments the objects instance count.</summary>
		/// <remarks>Retain counts work exactly like in native code and both mobjc and mcocoa
		/// adhere to the cocoa memory management naming conventions. So, for example, mcocoa's
		/// Create methods create auto-released objects with retain counts of one. See Apple's
		/// <a href = "http://developer.apple.com/documentation/Cocoa/Conceptual/MemoryMgmt/MemoryMgmt.html">Memory Management Programming Guide</a>
		/// for more details.
		///
		/// <para/>Note that you can also use <c>retain</c> but <c>Retain</c> is often more useful
		/// because mcocoa provides <c>Retain</c> methods with better return types (which allows
		/// you do do things like <c>NSString m_name = NSString.Create("Fred").Retain();</c>).</remarks>
		public NSObject Retain()
		{
			Unused.Value = retain();
			return this;
		}
		
		/// <summary>Always returns type NSObject.</summary>
		/// <remarks>To get the dynamic type of an instance use the <c>class_</c> method.</remarks>
		public static Class Class
		{
			get {return ms_class;}
		}
		
		/// <summary>Returns a new or existing NSObject associated with the unmanaged instance.</summary>
		public static NSObject Lookup(IntPtr instance)
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
					{
						Func<IntPtr, NSObject> factory;
						if (!ms_factories.TryGetValue(type, out factory))	// Activator.CreateInstance is also a bottleneck
						{
							ConstructorInfo info = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, ms_factoryArgTypes, null);
							if (info == null)
								throw new InvalidOperationException("Couldn't find an (IntPtr) ctor for " + type);
							factory = i => (NSObject) info.Invoke(new object[]{i});
							ms_factories.Add(type, factory);
						}
						managed = factory(instance);
					}
					// If we can't create a subclass then just create an NSObject.
					else
						managed = new NSObject(instance);
				}
			}
			
			return managed;
		}
		
		/// <summary>Returns the unmanaged instance.</summary>
		public static implicit operator IntPtr(NSObject value)
		{
			return value != null ? value.m_instance : IntPtr.Zero;
		}
		
		/// <summary>Returns the unmanaged instance.</summary>
		/// <remarks>This is needed for languages that don't support operator overloading.</remarks>
		public static IntPtr ToIntPtrType(NSObject value)
		{
			return value != null ? value.m_instance : IntPtr.Zero;
		}
		
		[Pure]
		public static bool IsNullOrNil(NSObject o)
		{
			return o == null || o.m_instance == IntPtr.Zero;
		}
		
		[Pure]
		public bool IsNil()
		{
			return m_instance == IntPtr.Zero;
		}
		
		/// <summary>Returns true if the unmanaged instance has been deleted.</summary>
		/// <remarks>This should only be used for debugging and testing.
		///
		/// <para/>This will be set correctly only if the last release call was from managed
		/// code or the type is exported.</remarks>
		[Pure]
		public bool IsDeallocated()
		{
			return m_deallocated;
		}
		
		/// <summary>Dynamic method call.</summary>
		/// <param name = "name">A method name, e.g. "setFrame:display:".</param>
		/// <param name = "args">The arguments to pass to the method. If the arity or argument types
		/// don't match the unmanaged code an exception will be thrown.</param>
		public object Call(string name, params object[] args)
		{
			Contract.Requires(name != null, "name is null");
			Contract.Requires(!m_deallocated, "ref count is zero");
			
			object result;
			
			if (m_instance != IntPtr.Zero)
				result = Native.Call(m_instance, name, args);
			else
				result = new NSObject(IntPtr.Zero);
			
			return result;
		}
		
		/// <summary>Dynamic call to an exported type&apos;s base class method.</summary>
		/// <param name = "baseClass">The class for the type (or a descendent of the type) containing the method you want to call.</param>
		/// <param name = "name">A method name, e.g. &quot;setFrame:display:&quot;.</param>
		/// <param name = "args">The arguments to pass to the method. If the arity or argument types
		/// don't match the unmanaged code an exception will be thrown.</param>
		public object SuperCall(Class baseClass, string name, params object[] args)
		{
			Contract.Requires(baseClass != null, "baseClass is null");
			Contract.Requires(!string.IsNullOrEmpty(name), "name is null or empty");
			Contract.Requires(!m_deallocated, "ref count is zero");
			
			object result = IntPtr.Zero;
			
			if (m_instance != IntPtr.Zero)
			{
				Native native = new Native(m_instance, new Selector(name), baseClass);
				native.SetArgs(args);
				result = native.Invoke();
			}
			
			return result;
		}
		
		/// <summary>Get or set an ivar.</summary>
		/// <remarks>Usually these will be Interface Builder outlets.</remarks>
		public NSObject this[string ivarName]
		{
			get
			{
				Contract.Requires(!string.IsNullOrEmpty(ivarName), "ivarName is null or empty");
				Contract.Requires(!m_deallocated, "ref count is zero");
				
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
				Contract.Requires(!string.IsNullOrEmpty(ivarName), "ivarName is null or empty");
				Contract.Requires(!m_deallocated, "ref count is zero");
				
				if (m_instance != IntPtr.Zero)
				{
					// Retain the new value (if any).
					IntPtr exception = IntPtr.Zero, dummy = IntPtr.Zero;
					if (!NSObject.IsNullOrNil(value))
					{
						Unused.Value = DirectCalls.Callp(value, Selector.Retain, ref exception);
						if (exception != IntPtr.Zero)
							CocoaException.Raise(exception);
					}
					
					// Release the old value (if any).
					IntPtr oldValue = IntPtr.Zero;
					IntPtr ivar = object_getInstanceVariable(m_instance, ivarName, ref oldValue);				
					if (ivar == IntPtr.Zero)
					{
						Unused.Value = DirectCalls.Callp(value, Selector.Release, ref dummy);
						throw new ArgumentException(ivarName + " isn't a valid instance variable");
					}
					
					if (oldValue != IntPtr.Zero)
					{
						Unused.Value = DirectCalls.Callp(oldValue, Selector.Release, ref exception);
						if (exception != IntPtr.Zero)
						{
							Unused.Value = DirectCalls.Callp(value, Selector.Release, ref dummy);
							CocoaException.Raise(exception);
						}
					}
					
					// Set the value.
					ivar = object_setInstanceVariable(m_instance, ivarName, value);
				}
			}
		}
		
#if DEBUG
		/// <summary>Debug method which returns a list of all NSObject instances still in use.</summary>
		/// <remarks>Note that the unmanaged instance may have been deleted.</remarks>
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
			
			return lhs.isEqual(rhs);
		}
		
		public static bool operator!=(NSObject lhs, NSObject rhs)
		{
			return !(lhs == rhs);
		}
		
		public override int GetHashCode()
		{
			return hash();
		}
		
		[ThreadModel(ThreadModel.SingleThread)]
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
		
		internal void Deallocated()
		{
			if (!m_deallocated)
			{
				// Allow the managed classes to clean up their state.
				OnDealloc();
				Contract.Assert(m_deallocated, "base.OnDealloc wasn't called");
				
				// Remove the instance from our list.
				lock (ms_instancesLock)
				{
					bool removed = ms_instances.Remove(m_instance);
					Contract.Assert(removed, "dealloc was called but the instance is not in ms_instances");
				}
				
				// Allow the unmanaged base class to clean itself up. Note that we have to be
				// careful how we do this to avoid falling into infinite loops...
				if (m_instance != IntPtr.Zero)
				{
					IntPtr klass = DoGetNonExportedBaseClass();
					Native native = new Native(m_instance, Selector.Dealloc, klass);
					Unused.Value = native.Invoke();
				}
			}
		}
		
		#region Protected Methods
		/// <summary>Called just before the unmanaged instance of an exported type is deleted.</summary>
		/// <remarks>Typically this is where you will release any objects you have retained. Note
		/// that the base OnDealloc method must be called (if you don't an <see cref = "ContractException">ContractException</see> 
		/// will be thrown).</remarks>
		protected virtual void OnDealloc()
		{
			m_deallocated = true;
		}
		#endregion
		
		#region Private Methods
		private IntPtr DoGetNonExportedBaseClass()
		{
			IntPtr klass = object_getClass(m_instance);
			
			while (klass != IntPtr.Zero)
			{
				IntPtr ptr = class_getName(klass);
				string name = Marshal.PtrToStringAnsi(ptr);
				
				Type type;
				if (!Registrar.TryGetType(name, out type))
					break;
					
				klass = class_getSuperclass(klass);
			}
			Contract.Assert(klass != IntPtr.Zero, "couldn't find a non-exported base class");	// should have found NSObject at least
			
			return klass;
		}
		
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
		
		#region P/Invokes
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
		
		#region Fields
		private readonly IntPtr m_instance;
		private readonly IntPtr m_class;
		private readonly IntPtr m_baseClass;
		private volatile bool m_deallocated;
		
		private static Dictionary<string, Type> ms_registeredClasses = new Dictionary<string, Type>();
		private static object ms_instancesLock = new object();
			private static Dictionary<IntPtr, NSObject> ms_instances = new Dictionary<IntPtr, NSObject>();
			private static Dictionary<Type, bool> ms_exports = new Dictionary<Type, bool>();
			private static Dictionary<Type, Func<IntPtr, NSObject>> ms_factories = new Dictionary<Type, Func<IntPtr, NSObject>>();
			private static Type[] ms_factoryArgTypes = new Type[]{typeof(IntPtr)};
				
#if DEBUG
		private static WeakList<NSObject> ms_refs = new WeakList<NSObject>(64);
#endif
		#endregion
	}
}
