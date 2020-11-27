using System;
using GTA.Native;
using System.Drawing;
using GTA.Math;
using GTA;
using System.Collections.Generic;

namespace FusionLibrary
{
    public class ScaleformGui
    {
        public ScaleformGui(string scaleformID)
        {
            ScaleformId = scaleformID;

            Load();
        }

        public string ScaleformId { get; }

        public int Handle => _handle;

        public bool IsValid
        {
            get
            {
                return Handle != 0;
            }
        }

        public bool IsLoaded
        {
            get
            {
                return Function.Call<bool>(Hash.HAS_SCALEFORM_MOVIE_LOADED, Handle);
            }
        }
        public bool DrawInPauseMenu { get; set; }

        private int _handle;

        public void CallFunction(string function, params object[] arguments)
        {
            Function.Call(Hash.BEGIN_SCALEFORM_MOVIE_METHOD, Handle, function);
            foreach (var argument in arguments)
            {
                if (argument is int)
                {
                    Function.Call(Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT, (int)argument);
                }
                else if (argument is string)
                {
                    Function.Call(Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_PLAYER_NAME_STRING, (string)argument);
                }
                else if (argument is char)
                {
                    Function.Call(Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_PLAYER_NAME_STRING, argument.ToString());
                }
                else if (argument is float)
                {
                    Function.Call(Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_FLOAT, (float)argument);
                }
                else if (argument is bool)
                {
                    Function.Call(Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_BOOL, (bool)argument);
                }
                else
                {
                    throw new ArgumentException(string.Format("Unknown argument type {0} passed to scaleform with handle {1}.", argument.GetType().Name, Handle), "arguments");
                }
            }

            Function.Call(Hash.END_SCALEFORM_MOVIE_METHOD);
        }

        public void Unload()
        {
            int handle = _handle;
            unsafe
            {
                Function.Call(Hash.SET_SCALEFORM_MOVIE_AS_NO_LONGER_NEEDED, &handle);
            }

            _handle = -1;
        }

        public void Load()
        {
            _handle = Function.Call<int>(Hash.REQUEST_SCALEFORM_MOVIE, ScaleformId);
        }

        public void Render2DFullscreen()
        {
            Function.Call(Hash.SET_SCRIPT_GFX_DRAW_BEHIND_PAUSEMENU, DrawInPauseMenu);

            Function.Call(Hash.DRAW_SCALEFORM_MOVIE_FULLSCREEN, Handle, 255, 255, 255, 255, 0);
        }

        public void Render2D(PointF location, float scale)
        {
            Function.Call(Hash.SET_SCRIPT_GFX_DRAW_BEHIND_PAUSEMENU, DrawInPauseMenu);

            Function.Call(Hash.DRAW_SCALEFORM_MOVIE, Handle, location.X, location.Y, scale, scale, 255, 255, 255, 255, 0);
        }

        public void Render2D(PointF location, SizeF size)
        {
            Function.Call(Hash.SET_SCRIPT_GFX_DRAW_BEHIND_PAUSEMENU, DrawInPauseMenu);

            Function.Call(Hash.DRAW_SCALEFORM_MOVIE, Handle, location.X, location.Y, size.Width, size.Height, 0, 0, 0, 0, 0);
        }

        public void Render3D(Vector3 position, Vector3 rotation, Vector3 scale)
        {
            Function.Call(Hash.SET_SCRIPT_GFX_DRAW_BEHIND_PAUSEMENU, DrawInPauseMenu);

            Function.Call(Hash.DRAW_SCALEFORM_MOVIE_3D_SOLID, Handle, position.X, position.Y, position.Z, rotation.X, rotation.Y, rotation.Z, 2.0f, 2.0f, 1.0f, scale.X, scale.Y, scale.Z, 2);
        }

        public void Render3DAdditive(Vector3 position, Vector3 rotation, Vector3 scale)
        {
            Function.Call(Hash.SET_SCRIPT_GFX_DRAW_BEHIND_PAUSEMENU, DrawInPauseMenu);

            Function.Call(Hash.DRAW_SCALEFORM_MOVIE_3D, Handle, position.X, position.Y, position.Z, rotation.X, rotation.Y, rotation.Z, 2.0f, 2.0f, 1.0f, scale.X, scale.Y, scale.Z, 2);
        }
    }
}
