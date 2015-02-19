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

        public ClearScriptRepl()
            : this(new DefaultConstructorThingFactory())
        {

        }

        public ClearScriptRepl(IThingFactory factory)
        {
            var commandTypes = from type in Assembly.GetEntryAssembly().GetTypes()
                               where typeof(IReplCommand).IsAssignableFrom(type) && type.IsInterface == false && type.IsAbstract == false
                               select type;

            this.commands = from type in commandTypes
                            select factory.Create(type) as IReplCommand;

            var runtime = new V8Runtime();

            var engine = runtime.CreateScriptEngine();

            foreach (IReplCommand service in commands)
            {
                engine.AddHostObject(service.Name, service);
            }

            this.engine = engine;
        }

        public void AddHostObject(string name, object obj)
        {
            this.engine.AddHostObject(name, obj);
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
                        Console.WriteLine("help");
                        Console.WriteLine("history");
                        Console.WriteLine("clearHistory");
                        Console.WriteLine("quit | exit");
                        foreach (IReplCommand command in commands)
                        {
                            Console.WriteLine(command.Name);
                        }
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
                        else if (result is VoidResult || result is Undefined)
                        {
                        }                     
                        else if (result is string)
                        {
                            Console.WriteLine(result);
                        }
                        else
                        {
                            var json = JsonConvert.SerializeObject(result, Formatting.Indented);
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
