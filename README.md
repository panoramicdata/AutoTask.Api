[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

[![Codacy Badge](https://app.codacy.com/project/badge/grade/AutoTask.Api)](https://app.codacy.com/gh/panoramicdata/AutoTask.Api/dashboard)

# AutoTask.Api

[![Nuget](https://img.shields.io/nuget/v/AutoTask.Api)](https://www.nuget.org/packages/AutoTask.Api/)

A .NET client for the Autotask Web Services (SOAP) API, version 1.6.

## Installation

```
dotnet add package AutoTask.Api
```

## Credentials

Three things are needed, and all three are required:

- the username and password of an Autotask resource with the **API User (API-only)** security level
  (a normal user login will not work - see
  [Requirements for version 1.6+](https://www.autotask.net/help/developerhelp/Content/APIs/SOAP/General_Topics/SOAP_Requirements.htm)), and
- an **integration code**, obtained by registering the integration in Autotask.

## Querying with QueryXML

`Client` takes [QueryXML](https://www.autotask.net/help/developerhelp/Content/APIs/SOAP/APICalls/QueryXML.htm)
and returns the generated entity types:

```C#
using AutoTask.Api;

using var client = new Client(
	username: "api-user@example.com",
	password: "password",
	integrationCode: "YOUR_INTEGRATION_CODE");

var wsdlVersion = await client.GetWsdlVersion(cancellationToken);

const string queryXml =
	"<queryxml><entity>Account</entity><query><field>id<expression op=\"greaterthan\">0</expression></field></query></queryxml>";

// A single page of results. Autotask returns at most 500 entities per query call.
var firstPage = await client.QueryAsync(queryXml, cancellationToken);

// ...or every matching entity, paging past the 500-record limit on your behalf.
var everything = await client.GetAllAsync(queryXml, cancellationToken);
```

`Client` also offers `GetFieldInfoAsync`, `CreateAsync`, `UpdateAsync` and `DeleteAsync`.
An `ILogger` can be passed as the fourth argument to log the raw SOAP requests and responses
(requests at `Debug`, responses at `Trace`), and a `ClientOptions` as the fifth to set timeouts or
to pin a known Autotask server id and skip the zone lookup.

## Querying with filters

`AutoTaskClient` builds the QueryXML for you from a `Filter`, and returns typed entities:

```C#
using AutoTask.Api;
using AutoTask.Api.Config;
using AutoTask.Api.Filters;

using var autoTaskClient = new AutoTaskClient(new AutoTaskConfiguration
{
	Username = "api-user@example.com",
	Password = "password",
	IntegrationCode = "YOUR_INTEGRATION_CODE"
});

var tickets = await autoTaskClient.GetAsync<Ticket>(new Filter
{
	Items =
	[
		new FilterItem { Field = "ticketType", Operator = Operator.Equals, Value = "2" },
		new FilterItem { Field = "Status", Operator = Operator.NotEquals, Value = "5" }
	]
});
```

Filters can also be parsed from a compact string form, where each comma-delimited item uses one of
the operator tokens `:` (equals), `!:` (not equals), `>`, `<`, `>:`, `<:`, `%` (like), `!%`,
`^` (begins with), `$` (ends with), `:~` (regex matches) or `!~`:

```C#
var tickets = await autoTaskClient.GetAsync<Ticket>(new Filter("ticketType:2,Status!:5"));
```

A field name prefixed with `UDF ` is treated as a user-defined field.

## A note on the API

Autotask put the SOAP API into a limited enhancement phase in Q4 2020 and intend to deactivate
version 1.6 eventually; new integrations should consider the REST API instead. See the
[introduction to the Web Services SOAP API](https://www.autotask.net/help/developerhelp/Content/APIs/SOAP/General_Topics/WebServicesAPI_INTRO.htm)
for the current status.

Contributions welcome!
