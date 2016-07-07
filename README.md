This is a collection of tools for interacting with HOBOlink. It doesn't do much yet.

To make the API server work, create hobolink.config in the same directory as web.config and make it look like this:

```xml
<appSettings>
  <add key="HobolinkUri" value="https://your.hobolink/url/here" />
</appSettings>```