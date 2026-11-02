using MoonSharp.Interpreter;
using YAFC.Input;
using YAFC.LuaApi.Extensions;

namespace YAFC.LuaApi
{
    public sealed class LuaInputApi : ILuaApi
    {
        private readonly Script _script;
        private readonly InputManager _input;

        public LuaInputApi(LuaApiContext ctx)
        {
            _script = ctx.Script;
            _input = ctx.Input;
        }

        public void Register()
        {
            _script.Globals.Set("btn", DynValue.NewCallback(btn));
            _script.Globals.Set("btnp", DynValue.NewCallback(btnp));
        }

        private DynValue btn(ScriptExecutionContext ctx, CallbackArguments args)
        {
            int btnId = LuaApiExtensions.IntArg(args, 0, "btn");
            return DynValue.NewBoolean(_input.IsBtnDown(btnId));
        }

        private DynValue btnp(ScriptExecutionContext ctx, CallbackArguments args)
        {
            int btnId = LuaApiExtensions.IntArg(args, 0, "btnp");
            return DynValue.NewBoolean(_input.IsBtnPressed(btnId));
        }
    }
}
