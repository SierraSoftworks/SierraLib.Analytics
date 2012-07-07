#What is SierraLib.Analytics?
SierraLib.Analytics is a native C# implementation of the original Google Analytics tracking library for Android devices. It has been modified and optimised for tracking Windows applications and provides a SQLite backing store for maintaining persistent tracking records.

For more information about the functionality provided in the Google Analytics tracking library, [review the original documentation](https://developers.google.com/analytics/devguides/collection/android/).

##What functionality is provided?
###Page View tracking
The library is capable of tracking virtual page views, allowing you to track the amount of time users spend on each "page". Page Views can also make use of **Custom Variables** to allow for additional information to be submitted with each page view.
###Event tracking
Events can be used to track particular actions that are made by the user within the application. Events may also be assigned a numeric value to help keep track of values which may fluctuate.
###Custom Variables
Custom Variables can be set on a User, Session or Page basis. User space variables will persist until they are overwritten by new ones, they will last the extent of the application lifespan on the installed system. Session variables will persist for the run time of the application, or until overwritten; and Page variables will only be transmitted with the next Page view.

##Usage
The tracker is accessed through a lazily initialized static property, **Tracker.Instance** within the **SierraLib.Analytics.Google** namespace. The tracker needs to be initialized before it can be used, this is done by calling the **StartSession** function.

#####Example Initialization
    Tracker.Instance.Initialize("UA-123456-1", "tracker.db", true, 20);
Will initialize the tracker with the given account code, using `Path.Combine(Environment.CurrentDirectory, "tracker.db")`
as the database file to write to, and starting a new visit (as opposed to using a previous one). The dispatch interval will be set for 20 seconds. Meaning that every 20 seconds new events will be pushed to the google analytics servers.

#####Example Page View
    Tracker.Instance.TrackPageView("/page","Page Title");
    Tracker.Instance.TrackPageView("/page2");
Both of these will track page views, the first will additionally provide the Page Title parameter to Google Analytics, which is useful for generating a human-readable page report.

#####Example Custom Variables
    Tracker.Instance.SetCustomVariable(1, "Variable Name", "Variable Value", CustomVariable.Scopes.Page);
    Tracker.Instance.CustomVariables[1] = new CustomVariable(1, "Variable Name", "Variable Value", CustomVariable.Scopes.Session);
Note that the indices for the custom variables are between **1 and 5**.
