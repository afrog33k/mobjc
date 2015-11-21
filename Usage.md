## Basic Usage ##

In general [mcocoa](http://code.google.com/p/mcocoa/) is what you should typically be using to interoperate with Objective-C code. However mobjc does need to be used directly on occasion. This is usually because you need to make a dynamic method call or you need to define a new Objective-C class using managed code.

Dynamic method calls are needed fairly often. For example, validateUserInterfaceItem methods may be called with an NSMenuItem or NSToolbarItem sender so a dynamic call is needed to get things like the action selector. Here's an example which uses mcocoa:

```
// If we know that sender will always be an NSMenuItem then we can make 
// the sender a NSMenuItem which would allow us to avoid the dynamic call
// and simply call the action method of NSMenuItem.
public bool validateUserInterfaceItem(NSObject sender)
{
    // It's also possible to cast the result instead of using the To method,
    // but To is a bit smarter than a simple cast and provides better error
    // messages on failures.
    Selector sel = sender.Call("action").To<Selector>();
    
    bool valid = false;
    if (sel.Name == "processSelection:")
    {
        NSRange range = m_textView.selectedRange();
        valid = range.length > 0;
    }
    else if (respondsToSelector(sel))
    {
        valid = true;
    }
    
    return valid;
}
```

## Defining New Objective-C Classes ##

In order to create a new Objective-C class you need to decorate it with [ExportClassAttribute](https://home.comcast.net/~jesse98/public/mobjc_docs/types/MObjc-ExportClassAttribute.html). Here's an example:


```
// The outlets should have the same names as the outlets in Interface Builder.
[ExportClass("MyView", "NSView", Outlets = "label ok_button")]
internal sealed class MyView : NSView
{
    // This will be called automagically when the nib is loaded.
    private MyView(IntPtr instance) : base(instance)
    {
    }
        
    // If the sender is not an NSControl an exception will be thrown.
    public void addABox(NSControl sender)
    {
        // ...
    }

    // Methods which may be called from native code need to be registered.
    // This registration is automatic for methods of exported types which
    // start with a lower case letter but you can use RegisterAttribute if
    // you want the managed and unmanaged names to differ.
    public int tag()
    {
        return 33;
    }
}
```

The Outlets part of the ExportClassAttribute is optional.

## Memory Management ##

In general, memory management works exactly as it does with native code. The only wrinkle is that there can be a managed instance associated with native instances. If the native instance is an exported class the library will enforce a constraint that ensures that there is only one managed instance for the native instance and the managed instance becomes eligible for garbage collection as soon as the native instance's reference count drops to zero.

If the class is not exported there may be multiple managed instances associated with a single native instance. These managed instances are garbage collected normally and should not contain any state not derived from the native instance.

Note that references in custom classes should be released in OnDealloc not dealloc (if you try to override dealloc an exception will be thrown).

## Thread Safety ##

Thread safety is specified using [ThreadModelAttribute](https://home.comcast.net/~jesse98/public/mobjc_docs/types/MObjc-Helpers-ThreadModelAttribute.html). Types and methods which are not decorated with this attribute may only be used from within the main thread.

For specifics look at the API Reference, but in general once the types have been registered mobjc can be used from any thread.