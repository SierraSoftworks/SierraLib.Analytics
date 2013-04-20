SierraLib.Analytics is designed to provide an implementation framework for tracking engines within your applications. It aims to improve both the end-user and developer's experiences, making integration of tracking solutions into your application as painless as possible and helping to promote good programming practices along the way.
The library provides automatic persistent request storage to allow for network failures without the risk of loosing requests, as well as a thoroughly tested threaded request dispatcher to ensure that your user experience doesn't suffer. It also provides a number of very useful helper functions and tools for developers using the library to integrate tracking solutions, and those looking to implement their own trackers.

## Tracking Engines
Currently SierraLib.Analytics only supports Google's Universal Analytics tracking engine, however additional engines can be added on request (you'll need to provide the protocol information used by the engine).

## Usage
Using SierraLib.Analytics is quite straightforward, at the core of everything is the `TrackingEngine` implementation which provides the interface between SierraLib.Analytics and the remote tracking platform. The tracking engine allows you to submit tracking requests which are composed of a number of tracking modules (`ITrackingModule`) which are responsible for adding information to the tracking requests you make.

### TrackingEngine
Add the relevant engine's attribute to the assembly/class/method/property you wish to track under that engine (using engine attributes with the same account code will reuse the same engine for performance and sanity's sake). Alternatively, you can also get an instance to the engine by calling `TrackingEngine.Create` with the relevant parameters.

You should also add the `TrackingApplication` attribute if you wish to provide a custom name or version number for any requests which get submitted. If you leave this out then your application's Assembly Title and Assembly Version (set on your project's Properties page) will be used in their place.

```csharp
//On the assembly level
[assembly:UniversalAnalytics("UA-1234-1")]
[assembly:TrackingApplication("My Application", "1.1.2")]

//On the class level
[UniversalAnalytics("UA-1234-1")]
[TrackingApplication("My Application", "1.1.2")]
public class TrackMe
{
    //On the method level
    [UniversalAnalytics("UA-1234-1")]
    [TrackingApplication("My Application", "1.1.2")]
    public void PleaseSir(bool someMore)
    {

    }

    public void GiveSomeMore()
    {
        //Getting an instance without using attributes
        var engine = TrackingEngine.Create("UA-1234-1", x => new UniversalAnalytics(x));
    }
}
```

### Tracking
To track a request you need to make a call to the engine's `Track` method, giving it the modules you'd like to track. 

```csharp
public void TrackMe()
{
    var engine = TrackingEngine.Create("UA-1234-1", x => new UniversalAnalytics(x));
    engine.Track(new PageView(), new Path("https://mywebsite.com"), new Title("Home"));
}
```

SierraLib.Analytics tries to encourage the use of [Attributes](http://msdn.microsoft.com/en-us/library/z0w1kczw%28v%3Dvs.80%29.aspx) wherever possible; using attributes helps keep your code readable and separates much of the tracking logic from your actual code logic. Now, in order to make using attributes possible, we have provided overloads for the `Track` method which use a reference to the current method to determine which modules to include.

```csharp
[PageView]
[Path("https://mywebsite.com")]
[Title("Home")]
public void TrackMe()
{
    var engine = TrackingEngine.Create("UA-1234-1", x => new UniversalAnalytics(x));
    engine.Track(() => TrackMe());
}
```

You'll notice that the `Track` method effectively creates a wrapper around the function, and if we were to run that wrapper we would be calling the function. Don't worry though, we don't call your methods twice or anything like that, we're using one of the really cool features of LINQ, something called Expressions, and it allows us to get the method that you pass there without needing you to go through the mission that is Reflection. If I've lost a few of you, don't worry - you don't need to know the workings of it to be able to use our library (but it's worth the read if you're interested).

We also provide static methods which allow you to track methods without having to get the engine instance first. These work by first checking if the method (or any of its parents) has a tracking engine set and falling back on using the current default tracking engine if none could be found. You can change the current default engine if you'd like (it defaults to the first created engine) by calling `SetDefault()` on an instance of the engine which you'd like to become the default.

```csharp
[UniversalAnalytics("UA-1234-1")]
[PageView]
[Path("https://mywebsite.com")]
[Title("Home")]
public void TrackMe()
{
    TrackingEngine.Track(() => TrackMe());
}
```

### Filtering Attributes
We know that sometimes you need to send different information with tracking requests depending on why you're sending them. To help facilitate this we have provided the `Filter` property on all tracking attributes. By setting this property you can determine which tracking types will include its value.

```csharp
[UniversalAnalytics("UA-1234-1")]
[PageView]
[Path("https://mywebsite.com")]
[Title("Home")]
[Description("Initialized", Filter = TrackOn.Enter)]
[Description("Completed", Filter = TrackOn.Exit)]
public void TrackMe()
{
    TrackingEngine.Track(() => TrackMe());
    try
    {
        // Do some work...
        TrackingEngine.Track(() => TrackMe(), TrackOn.Exit);
    }
    catch(Exception ex)
    {
        TrackingEngine.Track(() => TrackMe(), TrackOn.Exception, new TrackedException(ex));
    }
}
```

I've actually skipped a few steps and shown a rather wide range of things you can do here, including filtering (keep in mind that the default is to trigger on all events), including static modules with attributes, and error handling for exceptions.

### Persistent Storage
Often you'll encounter usage scenarios where it is possible that your tracking requests will not have been submitted by the time the user wants to close the application, or the user may not have an active internet connection. Whatever the reason, the end result is that your trackig telemetry will not have been submitted to the relevant tracking servers. Our library handles this by storing requests in a persistent archive from which they are removed after being sent to the server. To make use of this store you will need to make a call to the `TrackingEngine.ProcessStoredRequests()` method, which tells the library to load all the relevant requests and attempt to transmit them to the server.

**Warning** You need to ensure that the tracking engine that the requests were generated on is initialized before making the call to `ProcessStoredRequests`. Failure to do so will result in your requests not being sent (even if it were possible, or the tracking engine is initialized later).

**Warning** If you are using multiple tracking engines within your application you should initialize all of them before making the call to `ProcessStoredRequests` to ensure that all possible requests are sent.

**Info** Making subsequent requests to `ProcessStoredRequests` will have no effect and will result in no additional requests being transmitted.

### Exiting
When exiting your application it is generally a good idea to give our library a bit of time to clean house. While not entirely necessary (everything should be stored on disk already), doing so allows any pending requests to complete and means less work for the application the next time you start it (processing the stored requests).

When exiting your application, we recommend you use the `WaitForActive` method to wait until all the active requests complete before allowing the application to exit.

```csharp
void CloseMe()
{
    // Wait 3 seconds before continuing anyway (if there are still requests pending)
    TrackingEngine.WaitForActive(TimeSpan.FromMilliseconds(3000));

    // ... Exit?
}
```

### Opting-Out
Most of the time you will want to present your users with the option to opt-out of tracking. We believe that if you don't want to be tracked you shouldn't be, and we know that developers are a lot more likely to give their users the option if it doesn't require any excess effort on their part...

In order to prevent an engine from processing any `Track` calls (essentially preventing any tracking requests from being generated) all you need to do is set the `Enabled` property on the relevant engine to `false`.

```csharp
void LoadSettings()
{
    // ... get your settings

    TrackingEngine.GetEngine(() => LoadThemSettings()).Enabled = !settings.OptOut;
}
```

## Custom Tracking Engines
One of the primary design aims of SierraLib.Analytics was to provide an extremely versatile and easy to use platform for implementing custom tracking solutions through the use of reusable patterns and a number of extremely powerful supporting classes. To give you an idea of how easy it is to implement your own tracking engine, Google's Universal Analytics protocol took us a bare 3 hours to implement.

To create a custom tracking engine, you need to derive from the `TrackingEngine` class and implement the relevant methods. There are a few additional methods which you can also override to provide custom logic, for example *Pre* and *Post* processing of requests on the engine level.

You should also try to ensure that your tracking engines are written in such a way as to minimize stored state information. By doing so you make it easier for the engine to be used across tracking requests from different instances of the application. For example, try to avoid keeping counters in te engine or relying on mappings between the engine and any requests made through it. If you need to store state information for a request, create your own implementation of the `PreparedTrackingRequest` class and store it there (remember to implement the ISerializable interface).

## Thanks
I've use a few brilliant open source libraries to help me develop SierraLib.Analytics, without them it would have taken considerably longer to do and I can guarantee it wouldn't be anywhere near as good as it is today.

### [RestSharp](https://github.com/restsharp/RestSharp) by [Andrew Young](https://github.com/ayoung)
RestSharp is a wrapper around the .NET Framework's built in HttpRequest framework, but don't let that fool you - it is exceptionally easy to use and allows you to access remote services without having to worry about all the nitty-gritty details, it is my go-to library for anything HTTP.

### [Akavache](https://github.com/github/Akavache) by [Paul Betts](https://github.com/xpaulbettsx) and [Phil Haack](https://github.com/Haacked)
Akavache is a custom key-value store for .NET which provides itself as a fully asynchronous (seriously, it doesn't even have synchronous access without some serious hacks) collection. It allows you to store almost anything without having to worry about writing up serialization contracts, ensuring that files exist and other such nonsense. Its other strength lies in its ability to cache content from remote sources in a very easy to use and developer friendly manner. I'd recommend it to anyone looking for a drop-in persistent storage or caching solution.

### [Reactive Extensions](https://rx.codeplex.com/) by [Microsoft Open Technologies, Inc.](https://github.com/MSOpenTech)
Reactive Extensions (or Rx as all the cool kids say) is an awesome project which presents developers with a new way to think about, and handle, events in .NET. The general idea is that events are treated as collections, allowing you to manipulate them in interesting ways. My personal favourite is the `.Throttle` function which has saved my sanity more times than I care to count, and puts a smile on my face each time I use it. Rx is a must have for modern applications which need to react to complex user input in a reliable and efficient manner.