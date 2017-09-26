using ContinuousDataIntegrityChecking.Commands;
using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Replify
{
    public interface IThingFactory
    {
        object Create(Type type);
    }

    /// <summary>
    /// Create command objects using parameterless constructor
    /// </summary>
    public class DefaultConstructorThingFactory : IThingFactory
    {
        public object Create(Type type)
        {
            return Activator.CreateInstance(type);
        }
    }

    public class ClearScriptRepl
    {
        public const string HistoryFile = "history.txt";

        private readonly IEnumerable<IReplCommand> commands;
        private readonly V8ScriptEngine engine;
        private readonly Dictionary<Object, NamedObject> hostObjects;

        public readonly TextWriter Output;        

        public ClearScriptRepl()
            : this(new DefaultConstructorThingFactory(), Console.Out)
        {

        }

        public ClearScriptRepl(IThingFactory factory)
            : this(factory, Console.Out)
        {
        }

        public ClearScriptRepl(TextWriter output)
            : this(new DefaultConstructorThingFactory(), output)
        {

        }

        public ClearScriptRepl(IThingFactory factory, TextWriter output)
        {
            this.Output = output;
            this.hostObjects = new Dictionary<object, NamedObject>();

            IEnumerable<Type> types = Assembly.GetExecutingAssembly().GetTypes();

            if (Assembly.GetEntryAssembly() != null)
            {
                types = types.Union(Assembly.GetEntryAssembly().GetTypes());
            }

            var commandTypes = from type in types
                               where typeof(IReplCommand).IsAssignableFrom(type) && type.IsInterface == false && type.IsAbstract == false
                               select type;

            this.commands = from type in commandTypes
                            select factory.Create(type) as IReplCommand;

            var runtime = new V8Runtime();

            this.engine = runtime.CreateScriptEngine();

            foreach (IReplCommand service in commands)
            {                
                this.AddHostObject(service.Name, service);            
            }

            var scriptCommand = new ScriptCommand(this);
            this.AddHostObject("Script", scriptCommand);            
        }

        public class NamedObject
        {
            public readonly string Name;
            public readonly object Object;

            public NamedObject(string name, object obj)
            {
                this.Name = name;
                this.Object = obj;
            }
        }

        public void AddHostObject(string name, object obj)
        {
            this.engine.AddHostObject(name, obj);
            this.hostObjects.Add(obj, new NamedObject(name, obj));
        }

        public void AddHostType(string name, Type type)
        {
            this.engine.AddHostType(name, type);
        }

        public void AddHostType<T>()
        {
            this.engine.AddHostType(nameof(T), typeof(T));
        }

        public void StartReplLoop(params string[] args)
        {
            // import any javascript script files included in the program args
            if (args.Length >= 1)
            {
                if (File.Exists(args[0]))
                {
                    Execute(File.ReadAllText(args[0]));
                }
                else
                {
                    Output.WriteLine("Unable to launch script: {0}, not found", args[0]);
                }
            }      

            Console.Write("> ");
            string line;
            while ((line = Console.ReadLine()) != null)
            {
                File.AppendAllText(HistoryFile, String.Format("{0:yyyy-dd-mm HH:MM} > {1}\n", DateTime.Now, line));

                if (!Execute(line))
                {
                    return;
                }

                Output.Write("> ");
            }
        }

        public bool Execute(string line)
        {
            try
            {
                switch (line)
                {
                    case "help":
                        Output.WriteLine("Available commands:\n");

                        var commandNames = commands.Select(command => command.Name).Union(hostObjects.Values.Select(obj => obj.Name)).OrderBy(command => command);

                        foreach (string commandName in commandNames)
                        {
                            Output.WriteLine(commandName);
                        }

                        Output.WriteLine();
                        Output.WriteLine("help");
                        Output.WriteLine("history");
                        Output.WriteLine("clearHistory");
                        Output.WriteLine("quit | exit");
                        break;
                    case "history":
                        Output.WriteLine(File.ReadAllText(HistoryFile));
                        break;
                    case "clearHistory":
                        File.Delete(HistoryFile);
                        break;
                    case "exit":
                    case "quit":
                        return false;
                    default:
                        var timer = new Stopwatch();
                        timer.Start();

                        // ignore trailing .
                        if (line.EndsWith("."))
                        {
                            line = line.Substring(0, line.Length - 1);
                        }
                        var result = engine.Evaluate(line);
                        timer.Stop();

                        if (result is IReplCommand)
                        {
                            Output.WriteLine(((IReplCommand)result).Help());
                        }
                        else if (hostObjects.ContainsKey(result))
                        {
                            Output.WriteLine(HelpManager.GetHelpInfo(result));                                                        
                        }
                        else if (result is VoidResult || result is Undefined)
                        {
                        }                     
                        else if (result is string)
                        {
                            Output.WriteLine(result);
                        }
                        else
                        {
                            if (result.GetType().GetGenericTypeDefinition() == typeof(Task<>))
                            {
                                NewMethodAsync((dynamic)result);
                            }
                            else
                            {
                                NewMethod(result);
                            }
                        }

                        if (timer.ElapsedMilliseconds < 10000)
                        {
                            Output.WriteLine("completed in {0}ms", timer.ElapsedMilliseconds);
                        }
                        else
                        {
                            Output.WriteLine("completed in {0:0.0}s", timer.ElapsedMilliseconds / 1000.0);
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                while (ex != null)
                {
                    Output.WriteLine(ex.Message);
                    Output.WriteLine(ex.StackTrace);
                    ex = ex.InnerException;
                }
            }
            return true;
        }

        private void NewMethodAsync<T>(Task<T> task)
        {
            NewMethod(task.Result);
        }

        private void NewMethod(object result)
        {
            var settings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Include };

            settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

            var json = JsonConvert.SerializeObject(result, Formatting.Indented, settings);
            Output.WriteLine(json);
        }
    }
}
