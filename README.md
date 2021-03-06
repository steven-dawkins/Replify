# Replify
Add a javascript REPL to your c# library
[![Build status](https://ci.appveyor.com/api/projects/status/ofmp8835nj6lnhhn?svg=true)](https://ci.appveyor.com/project/steven-dawkins/replify)

## Example REPL console application
```c#
// declare REPL, this will reflect the calling assembly identifying any
// classes which implement IReplCommand
var repl = new ClearScriptRepl();

// import underscore, you can't write anything useful in javascript without underscore
repl.Execute(File.ReadAllText("Scripts/underscore-min.js"));

// import System.Console so we can write stuff to the console
repl.AddHostType("Console", typeof(Console));

// start the REPL loop, this will exit gracefully when the user quits
repl.StartReplLoop();
```
## Example REPL command
```c#
// REPL commands are identified by implementing IReplCommand
public class GenerateCommand : IReplCommand
{
    // All public methods will be callable and identified in the command help
    public double[] Floats(int count)
    {            
        var rand = new Random();

        var results = from i in Enumerable.Range(0, count)
                      select rand.NextDouble();
        
        return results.ToArray();
    }
}
```

## Example REPL host object
```c#
// Alternatively REPL objects can be manually registered
public class GenerateCommand
{
    // All public methods will be callable and identified in the command help
    public double[] Floats(int count)
    {            
        var rand = new Random();

        var results = from i in Enumerable.Range(0, count)
                      select rand.NextDouble();
        
        return results.ToArray();
    }
}

repl.AddHostObject("Generate2", new GenerateCommand());
```

## REPL commands
- Help - List available commands
- (Command) - Display command help
- History - List REPL interaction history
- ClearHistory - Clear REPL interaction history
- quit - Exit REPL

## Example REPL interaction
```
> help
Available commands:

help
history
clearHistory
quit | exit
Generate
> Generate
Floats(count:System.Int32)

completed in 0ms
> Generate.Floats(5)
[
  0.413136830280133,
  0.10324922721052972,
  0.77985977371216741,
  0.4040036063659953,
  0.28329070856947952
]
completed in 41ms
>
```

## Ninject
The recommended method to support Ninject commands is to provide a ThingFactory implementation which delegates to Ninject.
```c#
public class NinjectThingFactory : IThingFactory
{
    private readonly StandardKernel kernel;

    public NinjectThingFactory(StandardKernel kernel)
    {
        this.kernel = kernel;
    }

    public object Create(Type type)
    {
        return this.kernel.Get(type);
    }
}
    
var repl = new ClearScriptRepl(new NinjectThingFactory(kernel));
```

## External script files
Manages scripts deployed to /Scripts
*Script.List()*: List all available scripts
*Script.Execute(index)*: Execute script by index
*Script.Execute(filename)*: Execute script by filename
