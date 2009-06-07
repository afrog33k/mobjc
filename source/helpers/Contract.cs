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
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace MObjc.Helpers
{
	/// <summary>Marks a method as an invariant method.</summary>
	/// <remarks>The method is typically named ObjectInvariant and should check
	/// the object's state using one of the <see cref = "Contract">Contract</see> Invariant methods. The method should
	/// be called manually at the end of all public methods.</remarks>
	/// <seealso cref = "Contract"/>
	[Serializable]
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public sealed class ContractInvariantMethodAttribute : Attribute
	{
	}
	
	/// <summary>Signals that an abstract class or interface has an associated class
	/// which defines contracts.</summary>
	/// <example>Usage is like this:
	/// <code>
	/// [ContractClass(typeof(IFooContract))]
	/// public interface IFoo
	/// {
	/// 	int Work(object data);
	/// }<para/>
	/// 
	/// [ContractClassFor(typeof(IFoo))]
	/// public sealed class IFooContract : IFoo
	/// {
	/// 	int IFoo.Work(object data)
	/// 	{
	/// 		Contract.Requires(data != null, "data is null");
	/// 		return default(int);
	/// 	}
	/// }
	/// </code></example>
	/// <seealso cref = "ContractClassForAttribute"/>
	/// <seealso cref = "Contract"/>
	[Serializable]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
	public sealed class ContractClassAttribute : Attribute
	{
		public ContractClassAttribute(Type otherClass)
		{
			OtherClass = otherClass;
		}
		
		public Type OtherClass {get; private set;}
	}
	
	/// <summary>Marks a class as containing contracts for an abstract class or interface.</summary>
	/// <example>Usage is like this:
	/// <code>
	/// [ContractClass(typeof(IFooContract))]
	/// public interface IFoo
	/// {
	/// 	int Work(object data);
	/// }<para/>
	/// 
	/// [ContractClassFor(typeof(IFoo))]
	/// public sealed class IFooContract : IFoo
	/// {
	/// 	int IFoo.Work(object data)
	/// 	{
	/// 		Contract.Requires(data != null, "data is null");
	/// 		return default(int);
	/// 	}
	/// }
	/// </code></example>
	/// <seealso cref = "ContractClassAttribute"/>
	/// <seealso cref = "Contract"/>
	[Serializable]
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public sealed class ContractClassForAttribute : Attribute
	{
		public ContractClassForAttribute(Type otherClass)
		{
			OtherClass = otherClass;
		}
		
		public Type OtherClass {get; private set;}
	}
	
	/// <summary>Signals that a method or delegate has no visible side effects (and 
	/// therefore can be used within a <see cref = "Contract">Contract</see> call).</summary>
	/// <remarks>Note that getters are assumed to be pure and so do not need to be decorated.</remarks>
	[Serializable]
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Delegate, AllowMultiple = false)]
	public sealed class PureAttribute : Attribute
	{
	}
	
	/// <summary>Thrown when a <see cref = "Contract">Contract</see> method fails.</summary>
	[Serializable]
	public class ContractException : Exception
	{
		public ContractException()
		{
		}
		
		public ContractException(string text) : base(text)
		{
		}
		
		public ContractException(string text, Exception inner) : base (text, inner)
		{
		}
		
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		protected ContractException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
	
	/// <summary>Partial implementation of .NET <a href = "http://msdn.microsoft.com/en-us/devlabs/dd491992.aspx">code contracts</a>.</summary>
	/// <remarks>Note that contract violations will result in <see cref = "ContractException">ContractException</see>s being thrown.</remarks>
	// throw
	[ThreadModel(ThreadModel.Concurrent)]
	public static class Contract
	{
		#region Asserts
		[Conditional("CONTRACTS_FULL")]
		[Conditional("DEBUG")]
		public static void Assert(bool predicate)
		{
			if (!predicate)
				throw new ContractException("assert failure");
		}
		
		[Conditional("CONTRACTS_FULL")]
		[Conditional("DEBUG")]
		public static void Assert(bool predicate, string mesg)
		{
			if (!predicate)
				throw new ContractException(mesg);
		}
		
		/// <summary>Like <c>Assert</c> except that the static verifier will make use
		/// of the predicates.</summary>
		[Conditional("CONTRACTS_FULL")]
		[Conditional("DEBUG")]
		public static void Assume(bool predicate)
		{
			if (!predicate)
				throw new ContractException("assume failure");
		}
		
		/// <summary>Like <c>Assert</c> except that the static verifier will make use
		/// of the predicates.</summary>
		[Conditional("CONTRACTS_FULL")]
		[Conditional("DEBUG")]
		public static void Assume(bool predicate, string mesg)
		{
			if (!predicate)
				throw new ContractException(mesg);
		}
		#endregion
		
		#region Design by Contract
		/// <summary>Used to verify that the caller is correctly calling the method.</summary>
		/// <remarks>Like <c>Requires</c> except that it is never compiled out.</remarks>
		public static void RequiresAlways(bool predicate)
		{
			if (!predicate)
				throw new ContractException("requires failure");
		}
		
		/// <summary>Used to verify that the caller is correctly calling the method.</summary>
		/// <remarks>Like <c>Requires</c> except that it is never compiled out.</remarks>
		public static void RequiresAlways(bool predicate, string mesg)
		{
			if (!predicate)
				throw new ContractException(mesg);
		}
		
		/// <summary>Used to verify that the caller is correctly calling the method.</summary>
		/// <remarks>These should be conditions that the caller can easily check for.</remarks>
		[Conditional("CONTRACTS_FULL")]
		[Conditional("CONTRACTS_PRECONDITIONS")]
		public static void Requires(bool predicate)
		{
			if (!predicate)
				throw new ContractException("requires failure");
		}
		
		/// <summary>Used to verify that the caller is correctly calling the method.</summary>
		/// <remarks>These should be conditions that the caller can easily check for.</remarks>
		[Conditional("CONTRACTS_FULL")]
		[Conditional("CONTRACTS_PRECONDITIONS")]
		public static void Requires(bool predicate, string mesg)
		{
			if (!predicate)
				throw new ContractException(mesg);
		}
		
		/// <summary>Used to verify that the method did what it was supposed to.</summary>
		/// <remarks>Unlike the real contracts code these are placed at the end,
		/// not the start of methods.</remarks>
		[Conditional("CONTRACTS_FULL")]
		public static void Ensures(bool predicate)
		{
			if (!predicate)
				throw new ContractException("ensures failure");
		}
		
		/// <summary>Used to verify that the method did what it was supposed to.</summary>
		/// <remarks>Unlike the real contracts code these are placed at the end,
		/// not the start of methods.</remarks>
		[Conditional("CONTRACTS_FULL")]
		public static void Ensures(bool predicate, string mesg)
		{
			if (!predicate)
				throw new ContractException(mesg);
		}
		
		/// <summary>Used to verify that the method did what it was supposed to when exiting via an exception.</summary>
		/// <typeparam name = "T">An exception type, e.g. InvalidOperationException.</typeparam>
		/// <remarks>This is not called automatically as it is with the real contracts code.</remarks>
		[Conditional("CONTRACTS_FULL")]
		public static void EnsuresOnThrow<T>(bool predicate)
		{
			if (!predicate)
				throw new ContractException("ensures failure");
		}
		
		/// <summary>Used to verify that the method did what it was supposed to when exiting via an exception.</summary>
		/// <typeparam name = "T">An exception type, e.g. InvalidOperationException.</typeparam>
		/// <remarks>This is not called automatically as it is with the real contracts code.</remarks>
		[Conditional("CONTRACTS_FULL")]
		public static void EnsuresOnThrow<T>(bool predicate, string mesg)
		{
			if (!predicate)
				throw new ContractException(mesg);
		}
		
		/// <summary>Used to verify that the state of an object is sane.</summary>
		/// <remarks>This should only be used within a method decorated with
		/// <see cref = "ContractInvariantMethodAttribute">ContractInvariantMethodAttribute</see>. 
		/// By convention the method is named ObjectInvariant. Unlike the real contracts code 
		/// the invariant method will not be called automatically so it should be called manually 
		/// at the end of every public method.</remarks>
		[Conditional("CONTRACTS_FULL")]
		public static void Invariant(bool predicate)
		{
			if (!predicate)
				throw new ContractException("invariant failure");
		}
		
		/// <summary>Used to verify that the state of an object is sane.</summary>
		/// <remarks>This should only be used within a method decorated with
		/// <see cref = "ContractInvariantMethodAttribute">ContractInvariantMethodAttribute</see>. By convention the method
		/// is named ObjectInvariant. Unlike the real contracts code the invariant method
		/// will not be called automatically so it should be called manually at the end of every
		/// public method.</remarks>
		[Conditional("CONTRACTS_FULL")]
		public static void Invariant(bool predicate, string mesg)
		{
			if (!predicate)
				throw new ContractException(mesg);
		}
		#endregion
		
		#region Quantifiers
		/// <summary>Returns true if the predicate is true for every value.</summary>
		/// <remarks>Returns true if there are no values as well.</remarks>
		public static bool ForAll<T>(IEnumerable<T> values, Func<T, bool> predicate)
		{
			Requires(values != null, "values is null");
			Requires(predicate != null, "predicate is null");
			
			foreach (T value in values)
			{
				if (!predicate(value))
					return false;
			}
			
			return true;
		}
		
		/// <summary>Returns true if the predicate is true for every index.</summary>
		/// <remarks>Also returns true if the range is empty.</remarks>
		public static bool ForAll(int lowerBound, int upperBound, Func<int, bool> predicate)
		{
			Requires(lowerBound >= 0, "lowerBound is negative");
			Requires(lowerBound <= upperBound, "lowerBound is larger than upperBound");
			Requires(predicate != null, "predicate is null");
			
			for (int index = lowerBound; index < upperBound; ++index)
			{
				if (!predicate(index))
					return false;
			}
			
			return true;
		}
		
		/// <summary>Returns true if the predicate returns true for at least one value.</summary>
		public static bool Exists<T>(IEnumerable<T> values, Func<T, bool> predicate)
		{
			Requires(values != null, "values is null");
			Requires(predicate != null, "predicate is null");
			
			foreach (T value in values)
			{
				if (predicate(value))
					return true;
			}
			
			return false;
		}
		
		/// <summary>Returns true if the predicate returns true for at least one index.</summary>
		public static bool Exists(int lowerBound, int upperBound, Func<int, bool> predicate)
		{
			Requires(lowerBound >= 0, "lowerBound is negative");
			Requires(lowerBound <= upperBound, "lowerBound is larger than upperBound");
			Requires(predicate != null, "predicate is null");
			
			for (int index = lowerBound; index < upperBound; ++index)
			{
				if (predicate(index))
					return true;
			}
			
			return false;
		}
		#endregion
	}
}
