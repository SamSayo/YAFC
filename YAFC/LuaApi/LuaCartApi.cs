using MoonSharp.Interpreter;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using YAFC.GameCarts;

namespace YAFC.LuaApi
{
    public sealed class LuaCartApi
    {
        private static readonly Regex ModuleName = new(@"^[A-Za-z0-9_]+(\.[A-Za-z0-9_]+)*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private readonly Script _script;
        private readonly CartFileSystem _files;
        private readonly SaveRam? _save;

        private readonly Dictionary<string, DynValue> _loaded = new();
        private readonly HashSet<string> _loading = new();

        private LuaCartApi(Script script, CartFileSystem files, SaveRam? save)
        {
            _script = script;
            _files = files;
            _save = save;
        }

        public static void Install(Script script, CartFileSystem files, SaveRam? save) => new LuaCartApi(script, files, save).Register();

        private void Register()
        {
            var fs = new Table(_script);
            fs.Set("read", DynValue.NewCallback(FsRead));
            fs.Set("exists", DynValue.NewCallback(FsExists));
            fs.Set("list", DynValue.NewCallback(FsList));
            fs.Set("json", DynValue.NewCallback(FsJson));
            _script.Globals.Set("fs", DynValue.NewTable(fs));

            var save = new Table(_script);
            save.Set("size", DynValue.NewCallback(SaveSize));
            save.Set("read", DynValue.NewCallback(SaveRead));
            save.Set("write", DynValue.NewCallback(SaveWrite));
            _script.Globals.Set("save", DynValue.NewTable(save));

            _script.Globals.Set("require", DynValue.NewCallback(Require));
        }

        private static DynValue Fail(string message)
            => DynValue.NewTuple(DynValue.Nil, DynValue.NewString(message));

        private static DynValue Fail(Exception e) => Fail(e switch
        {
            GameCartException => e.Message,
            FileNotFoundException => "file not found",
            DirectoryNotFoundException => "directory not found",
            JsonException => "json: " + e.Message,
            _ => "io error"
        });

        private static string StringArg(CallbackArguments args, int index, string func)
            => args.AsType(index, func, DataType.String, false).String;

        private static int IntArg(CallbackArguments args, int index, string func)
        {
            double d = args.AsType(index, func, DataType.Number, false).Number;
            if (d != Math.Floor(d) || d < 0 || d > int.MaxValue)
            {
                throw new ScriptRuntimeException($"bad argument #{index + 1} to '{func}' (non-negative integer expected)");
            }
            return (int)d;
        }

        private DynValue FsRead(ScriptExecutionContext ctx, CallbackArguments args)
        {
            string path = StringArg(args, 0, "fs.read");
            bool binary = args.Count > 1 && args[1].Type == DataType.String && args[1].String == "b";

            try
            {
                return DynValue.NewString(binary
                    ? Encoding.Latin1.GetString(_files.ReadBytes(path))
                    : _files.ReadText(path));
            }
            catch (Exception e)
            {
                return Fail(e);
            }
        }

        private DynValue FsExists(ScriptExecutionContext ctx, CallbackArguments args)
            => DynValue.NewBoolean(_files.FileExists(StringArg(args, 0, "fs.exists")));

        private DynValue FsList(ScriptExecutionContext ctx, CallbackArguments args)
        {
            string dir = StringArg(args, 0, "fs.list");

            try
            {
                var table = new Table(_script);
                int i = 1;
                foreach (string name in _files.List(dir))
                {
                    table.Set(i++, DynValue.NewString(name));
                }
                return DynValue.NewTable(table);
            }
            catch (Exception e)
            {
                return Fail(e);
            }
        }

        private DynValue FsJson(ScriptExecutionContext ctx, CallbackArguments args)
        {
            string path = StringArg(args, 0, "fs.json");
            try
            {
                var options = new JsonDocumentOptions
                {
                    MaxDepth = 32,
                    CommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };
                using JsonDocument doc = JsonDocument.Parse(_files.ReadText(path), options);
                return JsonToLua(doc.RootElement);
            }
            catch (Exception e)
            {
                return Fail(e);
            }
        }

        private DynValue JsonToLua(JsonElement e)
        {
            switch (e.ValueKind)
            {
                case JsonValueKind.Object:
                { 
                    var t = new Table(_script);
                    foreach (JsonProperty p in e.EnumerateObject())
                    {
                        t.Set(p.Name, JsonToLua(p.Value));
                    }
                    return DynValue.NewTable(t);
                }

                case JsonValueKind.Array:
                {
                    var t = new Table(_script);
                    int i = 1;
                    foreach (JsonElement item in e.EnumerateArray())
                    {
                        t.Set(i++, JsonToLua(item));
                    }
                    return DynValue.NewTable(t);
                }

                case JsonValueKind.String: return DynValue.NewString(e.GetString());
                case JsonValueKind.Number: return DynValue.NewNumber(e.GetDouble());
                case JsonValueKind.True: return DynValue.True;
                case JsonValueKind.False: return DynValue.False;
                default: return DynValue.Nil;
            }
        }

        private DynValue Require(ScriptExecutionContext ctx, CallbackArguments args)
        {
            string name = StringArg(args, 0, "require");

            if (!ModuleName.IsMatch(name))
            {
                throw new ScriptRuntimeException($"bad module name '{name}'");
            }

            if (_loaded.TryGetValue(name, out DynValue cached))
            {
                return cached;
            }

            if (!_loading.Add(name))
            {
                throw new ScriptRuntimeException($"loop while loading module '{name}'");
            }

            try
            {
                string path = "/" + name.Replace('.', '/') + ".lua";
                if (!_files.FileExists(path))
                {
                    throw new ScriptRuntimeException($"module '{name}' not found (expected {path})");
                }

                DynValue chunk = _script.LoadString(_files.ReadText(path), null, path);
                DynValue result = _script.Call(chunk);
                if (result.IsNil()) result = DynValue.True;

                _loaded[name] = result;
                return result;
            }
            catch (InterpreterException e) when (e is not ScriptRuntimeException)
            {
                throw new ScriptRuntimeException(e.DecoratedMessage ?? e.Message);
            }
            finally
            {
                _loading.Remove(name);
            }
        }

        private DynValue SaveSize(ScriptExecutionContext ctx, CallbackArguments args)
            => DynValue.NewNumber(_save?.Size ?? 0);

        private DynValue SaveRead(ScriptExecutionContext ctx, CallbackArguments args)
        {
            int offset = IntArg(args, 0, "save.read");
            int length = IntArg(args, 1, "save.read");

            if (_save == null) return Fail("cartridge has no save memory");
            if (!_save.TryRead(offset, length, out byte[] data)) return Fail("out of range");

            return DynValue.NewString(Encoding.Latin1.GetString(data));
        }

        private DynValue SaveWrite(ScriptExecutionContext ctx, CallbackArguments args)
        {
            int offset = IntArg(args, 0, "save.write");
            string text = StringArg(args, 1, "save.write");

            if (_save == null) return Fail("gamecart has no save memory");

            var bytes = new byte[text.Length];
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] > 0xFF) return Fail("string contains non-byte characters");
                bytes[i] = (byte)text[i];
            }

            return _save.TryWrite(offset, bytes) ? DynValue.True : Fail("out of range");
        }
    }
}
