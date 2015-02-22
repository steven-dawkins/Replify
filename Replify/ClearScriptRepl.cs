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

        public ClearScriptRepl()
            : this(new DefaultConstructorThingFactory())
        {

        }

        public ClearScriptRepl(IThingFactory factory)
        {
            this.hostObjects = new Dictionary<object, NamedObject>();

            var commandTypes = from type in Assembly.GetEntryAssembly().GetTypes().Union(Assembly.GetExecutingAssembly().GetTypes())
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

        public void StartReplLoop()
        {                                    
            Console.Write("> ");
            string line;
            while ((line = Console.ReadLine()) != null)
            {
                File.AppendAllText(HistoryFile, String.Format("{0:yyyy-dd-mm HH:MM} > {1}\n", DateTime.Now, line));

                if (!Execute(line))
                {
                    return;
                }

                Console.Write("> ");
            }
        }

        public bool Execute(string line)
        {
            try
            {
                switch (line)
                {
                    case "help":
                        Console.WriteLine("Available commands:\n");

                        var commandNames = commands.Select(command => command.Name).Union(hostObjects.Values.Select(obj => obj.Name)).OrderBy(command => command);

                        foreach (string commandName in commandNames)
                        {
                            Console.WriteLine(commandName);
                        }

                        Console.WriteLine("help");
                        Console.WriteLine("history");
                        Console.WriteLine("clearHistory");
                        Console.WriteLine("quit | exit");
                        break;
                    case "history":
                        Console.WriteLine(File.ReadAllText(HistoryFile));
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
                        var result = engine.Evaluate(line);
                        timer.Stop();

                        if (result is IReplCommand)
                        {
                            Console.WriteLine(((IReplCommand)result).Help());
                        }
                        else if (hostObjects.ContainsKey(result))
                        {
                            Console.WriteLine(HelpManager.GetHelpInfo(result));                                                        
                        }
                        else if (result is VoidResult || result is Undefined)
                        {
                        }                     
                        else if (result is string)
                        {
                            Console.WriteLine(result);
                        }
                        else
                        {
                            var settings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Include };

                            settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

                            var json = JsonConvert.SerializeObject(result, Formatting.Indented, settings);
                            Console.WriteLine(json);
                        }
                        Console.WriteLine("completed in {0}ms", timer.ElapsedMilliseconds);

                        break;
                }
            }
            catch (Exception ex)
            {
                while (ex != null)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    ex = ex.InnerException;
                }
            }
            return true;
        }        
    }
}
