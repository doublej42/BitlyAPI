# BitlyAPI
A C# implementation of the Bit.ly Api V4

All methods are documented and have similar names to the ones in the docs.

V4 is a full rewrite.

Before you can use the library you will need to  generate an Generic Access Token at  https://bitly.is/accesstoken

See the bitly documentation at https://dev.bitly.com/

```
var bitly = new Bitly(_genericAccessToken);
var linkResponse = await bitly.PostShorten("https://www.google.ca/");
var newLink = linkResponse.Link;
```

See unit tests for more examples.

If you need other methods added submit an issue.
