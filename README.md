# AutoTask.Api

To install the AutoTask API in your project, add the nuget package:

AutoTask.Api

To execute a query using the [AutoTask query language](https://ww4.autotask.net/help/Content/LinkedDOCUMENTS/WSAPI/T_WebServicesAPIv1_5.pdf), use code as follows:

```C#
var client = new Client(autoTaskCredentials.Username, autoTaskCredentials.Password);

var version = await client.GetVersion();

var result = await client
	.ExecuteQueryAsync("<queryxml><entity>Account</entity><query><field>id<expression op=\"greaterthan\">0</expression></field></query></queryxml>")
	.ConfigureAwait(false);
```
Contributions welcome!