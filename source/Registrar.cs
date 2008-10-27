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
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace MObjc
{	
	// Iterates through all types in the loaded assemblies and handles any that
	// are marked with ExportClassAttribute or RegisterAttribute.
	[DisableRuleAttribute("C1026", "NoStaticRemove")]
	public static class Registrar
	{
		internal static void Init()
		{
			if (!ms_inited)
			{
				if (!ms_canInit)
					throw new InvalidOperationException("mobjc was used, but CanInit is false");
					
				NSObject pool = new Class("NSAutoreleasePool").Call("alloc").Call("init").To<NSObject>();
				
				DoInit();
		
				// NSAutoreleasePool cannot be used within Posix threads unless Cocoa
				// is switched to "multithreading mode". We always have at least two
				// threads (the main thread and the finalizer thread) so we'll switch
				// to multithreading mode here.
				Selector selector = new Selector("foo");
				NSObject thread = new Class("NSThread").Call("alloc").Call("initWithTarget:selector:object:", null, selector, null).To<NSObject>();
				Ignore.Value = thread.Call("start");
		
				ms_inited = true;
				pool.release();
			}
		}
				
		// Registrar::Init is called automagically the first time an NSObject is contructed.
		// Init will grovel through all the loaded .NET types, find the ones which are registered
		// with mobjc, and define the associated Objective-C classes. However this can be
		// a bit tricky if multiple assemblies register types. Because this leads to rather icky
		// hard to track down errors, you must explicitly notify objc-sharp when it is safe to
		// call Init.
		public static bool CanInit
		{
			get {return ms_canInit;}
			set {ms_canInit = value;}
		}

		internal static bool TryGetType(string name, out Type type)
		{
			return ms_typeNames.TryGetValue(name, out type);
		}

		#region Private Methods -----------------------------------------------
		private static void DoInit()
		{
			DoValidateTypes();
			
			List<ClassEntry> exports = new List<ClassEntry>();
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) 
			{
				foreach (Type type in assembly.GetTypes()) 
				{
					ExportClassAttribute attr = Attribute.GetCustomAttribute(type, typeof(ExportClassAttribute)) as ExportClassAttribute;
					if (attr != null)
						exports.Add(new ClassEntry(type, attr));

					RegisterAttribute attr2 = Attribute.GetCustomAttribute(type, typeof(RegisterAttribute)) as RegisterAttribute;
					if (attr2 != null)
					{
						string name = attr2.Name ?? type.Name;
						
						if (type.IsValueType)
							TypeEncoder.Register(type, name);
						else
							NSObject.Register(type, name);
						}
				}
			}
			
			// Note that we need to initialize classes after structs in case a class method refers
			// to a struct.
			exports.Sort();									// must process base types before derived types
			foreach (ClassEntry export in exports)
			{
				if (ms_typeNames.ContainsKey(export.Attr.DerivedName))
					throw new ArgumentException(string.Format("{0} exports {1} but that class name has already been exported.", export.Type, export.Attr.DerivedName));
				
				ms_typeNames.Add(export.Attr.DerivedName, export.Type);
				ms_classNames.Add(export.Type, export.Attr.DerivedName);
				
				DoInitClass(export.Attr.DerivedName, export.Attr.BaseName, export.Type, export.Attr.IVars);
			}
		}
							
		private static void DoInitClass(string name, string baseName, Type type, string ivars)
		{
			Class superClass = new Class(baseName);
			
			IntPtr exception = IntPtr.Zero;
			IntPtr klass = CreateClass(superClass, name, ref exception);			
			if (exception != IntPtr.Zero)
				CocoaException.Raise(exception);
			
			if (ivars != null)
			{
				string[] ivnames = ivars.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
				
				int size = Marshal.SizeOf(typeof(IntPtr));
				byte alignment = (byte) Math.Log(size, 2);
				foreach (string ivname in ivnames)
				{
					byte added = class_addIvar(klass, ivname, new IntPtr(size), alignment, "@");
					if (added == 0)
						throw new ArgumentException(string.Format("Couldn't add ivar {0} to {1}. Is there already an ivar with that name?", ivname, name));
				}
			}

			foreach (MethodInfo info in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)) 
			{
				MethodAttribute attr = Attribute.GetCustomAttribute(info, typeof(MethodAttribute)) as MethodAttribute;
				if (attr != null) 
					DoAddMethod(name, info, attr.Name ?? info.Name, klass, superClass, attr.IsOverride);
			}

			MethodInfo info2 = typeof(NSObject).GetMethod("OnDealloc", BindingFlags.Instance | BindingFlags.NonPublic);
			DoAddMethod(name, info2, "dealloc", klass, superClass, true);

			RegisterClass(klass, ref exception);
			if (exception != IntPtr.Zero)
				CocoaException.Raise(exception);
		}
		
		[DisableRule("D1047", "TooManyArgs")]
		private static void DoAddMethod(string name, MethodInfo info, string selName, IntPtr klass, Class superClass, bool isOverride)
		{
			string encoding = DoGetEncoding(info);
			Managed method = new Managed(info, encoding);
			ManagedImp mimp = method.Call;					

			Selector selector = new Selector(selName);
			
			IntPtr cif = DoCreateCif(info);
			int result = AddMethod(superClass, klass, (IntPtr) selector, encoding, mimp, cif, isOverride);
			
			if (result != 0)
				if (result == 0xADDF)
					throw new ArgumentException(string.Format("Couldn't add {0}::{1}. Is the method already defined?", name, selector.Name));
				else
					throw new ArgumentException(string.Format("Couldn't create a closure for {0}::{1} ({2})", name, selector.Name, result));

			ms_imps.Add(mimp);
		}
		
		private static IntPtr DoCreateCif(MethodInfo info)
		{
			IntPtr resultType = Ffi.GetFfiType(info.ReturnType);

			ParameterInfo[] parms = info.GetParameters();
			int numArgs = parms.Length;
			
			PtrArray argTypes = new PtrArray(numArgs + 3);
			argTypes[0] = Ffi.GetFfiType(typeof(NSObject));
			argTypes[1] = Ffi.GetFfiType(typeof(Selector));

			for (int i = 0; i < numArgs; ++i)
			{
				Type type = parms[i].ParameterType;
				argTypes[i + 2] = Ffi.GetFfiType(type);
			}
			
			argTypes[numArgs + 2] = IntPtr.Zero;
			
			return Ffi.AllocCif(resultType, argTypes);		// note that we don't want to free argTypes
		}

		private static string DoGetEncoding(MethodInfo info)
		{
			StringBuilder sig = new StringBuilder();
			
			string s = TypeEncoder.Encode(info.ReturnType);
			sig.Append(s);
			sig.Append("@:");
			
			foreach (ParameterInfo param in info.GetParameters())
			{
				s = TypeEncoder.Encode(param.ParameterType);
				sig.Append(s);
			}
						
			return sig.ToString();
		}
		
		private static void DoValidateTypes()
		{
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) 
			{
				foreach (Type type in assembly.GetTypes()) 
				{
					DoValidateType(type);
				}
			}
		}

		private static void DoValidateType(Type type)
		{
			foreach (MethodInfo info in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)) 
			{
				ExportClassAttribute attr = Attribute.GetCustomAttribute(type, typeof(ExportClassAttribute)) as ExportClassAttribute;
				if (attr != null)
				{
					if (!typeof(NSObject).IsAssignableFrom(type))
						throw new ArgumentException(string.Format("{0} uses ExportClassAttribute, but does not descend from NSObject.", type));
				}

				DoValidateMethod(type, info);
			}
		}

		private static void DoValidateMethod(Type type, MethodInfo info)
		{
			ExportClassAttribute klassAttr = Attribute.GetCustomAttribute(type, typeof(ExportClassAttribute)) as ExportClassAttribute;
			MethodAttribute methodAttr = Attribute.GetCustomAttribute(info, typeof(MethodAttribute)) as MethodAttribute;

			if (klassAttr != null)
			{
			}
			else
			{
				if (methodAttr != null) 
				{					
					throw new ArgumentException(string.Format("{0} uses MethodAttribute, but not ExportClassAttribute.", type));
				}
			}
		}
		#endregion
		
		#region Private Types -------------------------------------------------
		private struct ClassEntry : IComparable<ClassEntry>, IEquatable<ClassEntry>
		{        
			public ClassEntry(Type type, ExportClassAttribute attr)
			{
				m_type = type;
				m_attr = attr;
			}
			
			public Type Type
			{
				get {return m_type;}
			}
			
			public ExportClassAttribute Attr
			{
				get {return m_attr;}
			}
			
			public int CompareTo(ClassEntry rhs)    
			{
				if (m_attr.DerivedName == rhs.m_attr.BaseName)	// if lhs is rhs's base then it needs to be processed before rhs	
					return -1;
					
				else if (m_attr.BaseName == rhs.m_attr.DerivedName)	// if lhs is a subclass of rhs then it needs to be processed after rhs
					return +1;
					
				else
					return 0;
			}
		 
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)        
					return false;
				
				if (GetType() != rhsObj.GetType()) 
					return false;
				
				ClassEntry rhs = (ClassEntry) rhsObj; 
				return CompareTo(rhs) == 0;
			}
				
			public bool Equals(ClassEntry rhs)    
			{
				return CompareTo(rhs) == 0;
			}

			public override int GetHashCode()
			{
				int hash;
				
				unchecked
				{
					hash = m_attr.BaseName.GetHashCode() + m_attr.DerivedName.GetHashCode();
				}
				
				return hash;
			}
			
			private readonly Type m_type;
			private readonly ExportClassAttribute m_attr;
		}

		private delegate IntPtr ManagedImp(IntPtr dummy, IntPtr result, IntPtr args);
		#endregion
		
		#region P/Invokes -----------------------------------------------------
		[DllImport("mobjc-glue.dylib")] 
		private extern static IntPtr CreateClass(IntPtr superClass, string name, ref IntPtr exception);

		[DllImport("mobjc-glue.dylib")] 
		private extern static void RegisterClass(IntPtr klass, ref IntPtr exception);
		
		[DllImport("mobjc-glue.dylib")] 
		private extern static int AddMethod(IntPtr superClass, IntPtr klass, IntPtr selector, string sig, ManagedImp imp, IntPtr cif, [MarshalAs(UnmanagedType.U1)] bool isOverride);
		
		[DllImport("/usr/lib/libobjc.dylib")]		// use IntPtr for size because size_t has the same size as a pointer
		private extern static byte class_addIvar(IntPtr cls, string name, IntPtr size, byte alignment, string encoding);
		#endregion

		#region Fields --------------------------------------------------------
		private static bool ms_canInit;
		private static Dictionary<string, Type> ms_typeNames = new Dictionary<string, Type>();
		private static Dictionary<Type, string> ms_classNames = new Dictionary<Type, string>();
		private static List<ManagedImp> ms_imps = new List<ManagedImp>();
		private static bool ms_inited;
		#endregion
	}
}
