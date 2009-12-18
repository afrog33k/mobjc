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

namespace MObjc
{
	/// <summary>Identifies the managed classes which can also be used as (new) Objective-C classes.</summary>
	/// <example>Here's an example of how you might use this attribute with a custom view:
	/// <code>
	/// // The outlets should be setup using Interface Builder. The class need not be sealed.
	/// [ExportClass(&quot;MyView&quot;, &quot;NSView&quot;, Outlets = &quot;label1 label2&quot;)]
	/// internal sealed class MyView : NSView
	/// {
	/// 	// This will be called automatically when the nib is loaded.
	/// 	private MyView(IntPtr instance) : base(instance)
	/// 	{
	/// 		// NSObject provides an indexer which allows you to get or set outlets.
	/// 		NSTextField label1 = this[&quot;label1&quot;].To&lt;NSTextField&gt;();
	/// 		label1.setStringValue(NSString.Create(&quot;hi&quot;));
	/// 		
	/// 		<para/>
	/// 		// IBOutlet is a simple wrapper around the NSObject indexer. It&apos;s probably
	/// 		// simplest to just use the indexer, but IBOutlet can be useful if the outlet
	/// 		// is not set right away or its value changes.
	/// 		NSTextField label2 = new IBOutlet&lt;NSTextField&gt;(this, &quot;label1&quot;).Value;
	/// 		label2.setStringValue(NSString.Create(&quot;there&quot;));
	/// 	}
	/// 
	/// 	<para/>
	/// 	// This is normally where you would release any objects you have retained.
	/// 	public override void OnDealloc()
	/// 	{
	/// 		// Note that the base method must be called (if you don&apos;t an exception
	/// 		// will be thrown).
	/// 		base.OnDealloc();
	/// 	}
	/// 	
	/// 	<para/>
	/// 	// Methods which start with a lower case letter are automatically registered
	/// 	// with the Objective-C runtime. These may be either brand-new methods
	/// 	// or overrides of existing base class methods.
	/// 	public void doSomething()
	/// 	{
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <example>And here is an example of how to write a class which is intended
	/// to be created from managed code:
	/// <code>
	/// [ExportClass(&quot;TableItem&quot;, &quot;NSObject&quot;)]
	/// internal sealed class TableItem : NSObject
	/// {
	/// 	// Note that we follow the Cocoa naming conventions so the alloc in AllocAndInitInstance
	/// 	// means that our object will have a reference count of one.
	/// 	public TableItem(string name) : base(NSObject.AllocAndInitInstance(&quot;TableItem&quot;))
	/// 	{
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <seealso cref = "MObjc.RegisterAttribute"/>
	[Serializable]
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class ExportClassAttribute : Attribute
	{
		/// <param name = "derivedName">The Objective-C class name, e.g. "TableItem".</param>
		/// <remarks>The base class will be NSObject.</remarks>
		public ExportClassAttribute(string derivedName) : this(derivedName, "NSObject")
		{
		}
		
		/// <param name = "derivedName">The Objective-C derived class name, e.g. "MyView".</param>
		/// <param name = "baseName">The Objective-C base class name, e.g. "NSView".</param>
		public ExportClassAttribute(string derivedName, string baseName)
		{
			if (string.IsNullOrEmpty(derivedName))
				throw new ArgumentException("derivedName is null or empty");
				
			if (string.IsNullOrEmpty(baseName))
				throw new ArgumentException("baseName is null or empty");
				
			DerivedName = derivedName;
			BaseName = baseName;
		}
		
		[ThreadModel(ThreadModel.Concurrent)]
		public string DerivedName {get; private set;}
		
		[ThreadModel(ThreadModel.Concurrent)]
		public string BaseName {get; private set;}
		
		/// <summary>Space delimited list of instance variables to add to the class.</summary>
		/// <remarks>These usually correspond to the outlets defined in Interface Builder.</remarks>
		[ThreadModel(ThreadModel.Concurrent)]
		public string Outlets {get; set;}
	}
}
