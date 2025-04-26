![NuGet Downloads](https://img.shields.io/nuget/dt/Tubes)
![GitHub License](https://img.shields.io/github/license/sduplooy/tubes)
![GitHub Issues or Pull Requests](https://img.shields.io/github/issues-pr/sduplooy/tubes)
![GitHub Issues or Pull Requests](https://img.shields.io/github/issues/sduplooy/tubes)
![GitHub Repo stars](https://img.shields.io/github/stars/sduplooy/tubes)
![GitHub watchers](https://img.shields.io/github/watchers/sduplooy/tubes)
![GitHub forks](https://img.shields.io/github/forks/sduplooy/tubes)

# Tubes

Tubes is a [pipe and filters pattern](https://www.enterpriseintegrationpatterns.com/patterns/messaging/PipesAndFilters.html) implementation. It is based on two [posts written by Steve Bate](https://eventuallyconsistent.net/tag/pipe-and-filters) which is unfortunately no longer available.

## Installation

To install Tubes, run the following command:

```sh
dotnet add package Tubes
```

## Usage

### Synchronous Pipeline

Synchronous filters must implement `IFilter<TMessage>` where `TMessage` is the type of message that will be processed
through the pipeline.

The pipeline is then constructed using the `Register` method on the `Pipeline<TMessage>` class.
The filters are executed in the order in which they are registered.

```csharp
var loginPipeline = new Pipeline<LoginMessage>();

loginPipeline.Register(new CheckUserSuppliedCredentials())
    .Register(new ValidateAgainstMembershipApi())
    .Register(new CheckApiKeyIsEnabledForClient())
    .Register(new IsUserLoginAllowed())
    .Register(new GetUserDetails());

loginPipeline.Execute(new LoginMessage {
    Username = "User",
    Password = "Password"
});
```

### Asynchronous Pipeline

Asynchronous filters must implement `IAsyncFilter<TMessage>` where `TMessage` is the type of message that will be processed through the pipeline.

The pipeline is then constructed using the `Register` method on the `AsyncPipeline<TMessage>` class.
The filters are executed in the order in which they are registered.

```csharp
var loginPipeline = new AsyncPipeline<LoginMessage>();

loginPipeline.Register(new CheckUserSuppliedCredentials())
    .Register(new CheckApiKeyIsEnabledForClient())
    .Register(new IsUserLoginAllowed())
    .Register(new ValidateAgainstMembershipApi())
    .Register(new GetUserDetails());

var cts = new CancellationTokenSource();

await loginPipeline.ExecuteAsync(new LoginMessage {
    Username = "Username",
    Password = "Password"
}, cts.Token);
```

### IStopProcessing

If an input message implements the `IStopProcessing` interface, the pipeline (sync and async) will stop processing any subsequent filters if the `Stop` property is set to `true`.

The `AsyncPipeline` respects the `CancellationToken` and will stop processing any subsequent filters when cancellation is requested.

## Aspects

The library also includes several aspects, viz.:

1. Exception logging aspect
2. Message logging aspect
3. Retry aspect
4. Transaction aspect

Aspects can be applied in the following manner and will be executed in the order in which they are applied:

```csharp
public void Invoke(LoginMessage message)
{ 
    var logHandler = new MessageLoggingAspect<LoginMessage>(_loginPipeline.Execute) 
    var errorHandler = new ExceptionLoggingAspect<LogInMessage>(logHandler.Execute) 
    errorHandler.Execute(message);
}
```

## Credits

Icon by [ColourCreatype](https://freeicons.io/tools-and-construction-26768/pipe-pipeline-water-instalation-construction-icon-949729) on [freeicons.io](https://freeicons.io)