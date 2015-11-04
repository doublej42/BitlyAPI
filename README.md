# BitlyAPI
A C# implementation of the Bit.ly Api

All methods are documented and have similar names to the ones in the docs with the / removed.
Before you can use the library you will need to  generate an api key at  https://bitly.com/a/oauth_apps
in web.config <add key="bitlyAccess_Token" value="YOUR_API_KEY" />

```
//Example usage:
 public ActionResult Index(int page = 0)
        {
            Bitly Bt = new Bitly();// or specify the key as a parameter
            var model = new VmAdmin
                {
                    UserInfo = Bt.UserInfo(),
                    UserHistory = Bt.UserLinkHistory(offset: page*50),
                    page = page
                };
            return View(model);
        }
```

Many fields in response will be null as they are used for other calls.
