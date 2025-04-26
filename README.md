![NuGet Downloads](https://img.shields.io/nuget/dt/Tubes)
![GitHub License](https://img.shields.io/github/license/sduplooy/tubes)
![GitHub Issues or Pull Requests](https://img.shields.io/github/issues-pr/sduplooy/tubes)
![GitHub Issues or Pull Requests](https://img.shields.io/github/issues/sduplooy/tubes)
![GitHub Repo stars](https://img.shields.io/github/stars/sduplooy/tubes)
![GitHub watchers](https://img.shields.io/github/watchers/sduplooy/tubes)
![GitHub forks](https://img.shields.io/github/forks/sduplooy/tubes)

# Tubes

Tubes is a [pipes and filters pattern](https://www.enterpriseintegrationpatterns.com/patterns/messaging/PipesAndFilters.html) implementation. It is based on two [posts written by Steve Bate](https://eventuallyconsistent.net/tag/pipe-and-filters) which is unfortunately no longer available.

## Installation

To install Tubes, run the following command:

`dotnet add package Tubes --version 1.3.0`

## Usage

This library provides two types of pipelines viz., a synchronous and an asynchronous pipeline.

### Synchronous

The synchronous pipeline is used as follows:

```csharp
var loginPipeline = new Pipeline<LoginMessage>();

loginPipeline.Register(msg => new CheckUserSuppliedCredentials(msg))
    .Register(msg => new CheckApiKeyIsEnabledForClient(msg))
    .Register(msg => new IsUserLoginAllowed(msg))
    .Register(msg => new ValidateAgainstMembershipApi(msg))
    .Register(msg => new GetUserDetails(msg));

loginPipeline.Execute(new LoginMessage {
    Username = "User",
    Password = "Password"
});
```

### Asynchronous

The asynchronous pipeline is used as follows:

```csharp
var loginPipeline = new AsyncPipeline<LoginMessage>();

loginPipeline.Register(msg => new CheckUserSuppliedCredentials(msg))
    .Register(msg => new CheckApiKeyIsEnabledForClient(msg))
    .Register(msg => new IsUserLoginAllowed(msg))
    .Register(msg => new ValidateAgainstMembershipApi(msg))
    .Register(msg => new GetUserDetails(msg));

var cts = new CancellationTokenSource();

await loginPipeline.ExecuteAsync(new LoginMessage {
    Username = "Username",
    Password = "Password"
}, cts.Token);
```

### IStopProcessing

If an input message implements the `IStopProcessing` interface, the pipeline (sync and async) will stop processing any subsequent filters if the `Stop` property is set to `true`.

The `AsyncPipeline` respects the `CancellationToken` and will stop processing any subsequent filters when cancellation is requested.

### IMessage<TResult, TError>

The `IMessage<TResult, TError>` interface is used to define a message that returns a result or an error.

### Result<T, TError>

The `Result<T, TError>` is the return type of the `Result` property on the IMessage<TResult, TError> interface. 

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