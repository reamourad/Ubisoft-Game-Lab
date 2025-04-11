using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;

namespace Player.UI.ControlPrompts
{
    public static class ControlImageLoader
    {
        private static Dictionary<string, Sprite> images = new Dictionary<string, Sprite>();

        private static readonly string XGamepadSearchPath = "XGamepad/T_X_{0}_White_Light";
        private static readonly string XGamepadSearchPathAlt = "XGamepad/T_X_{0}_Light";
        
        private static readonly string PGamepadSearchPath = "P5Gamepad/T_P5_{0}_Light";
        private static readonly string KeybaordSearchPath = "Keyboard_Mouse/T_{0}_Key_White";

        private static Dictionary<string, string> retargets;
        private static bool isGamepad = false;
        public static Sprite Load(InputAction action, bool hasPositiveNegative, bool showPositive)
        {
            if (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame)
            {
                isGamepad = true;
            }
            else if (Keyboard.current != null && Keyboard.current.wasUpdatedThisFrame)
            {
                isGamepad = false;
            }
            string strKey = "";

            if (!hasPositiveNegative)
            {
                strKey = InputReader.GetCurrentBindingText(action);
            }
            else
            {
                strKey = GetBindingText(action, showPositive);
            }

            strKey = Reformat(strKey);
            if(images.ContainsKey(strKey))
                return images[strKey];
            
            Sprite sprite = null;
            var gamepad = Gamepad.current;
            string searchpath="";
            if (isGamepad)
            {
                if (gamepad is XInputController)
                {
                    searchpath = string.Format(XGamepadSearchPath, strKey);
                    sprite = Resources.Load<Sprite>(searchpath);
                    if (sprite == null)
                    {
                        searchpath = string.Format(XGamepadSearchPathAlt, strKey);
                        sprite = Resources.Load<Sprite>(searchpath);
                    }
                }
                else
                {
                    //hoping PS5 and PS4 have the same key names
                    searchpath = string.Format(PGamepadSearchPath, strKey);
                    sprite = Resources.Load<Sprite>(searchpath);
                }
            }
            else
            {
                searchpath = string.Format(KeybaordSearchPath, strKey);
                sprite = Resources.Load<Sprite>(searchpath);
            }
            
            images[strKey] = sprite;
            return sprite;
        }

        private static string GetBindingText(InputAction action, bool showPositive)
        {
            
            var str = "";
            //We are assuming, Controller is always second, in the input-bindings asset. so when we have a +/- type input
            //ie - two bindings per actions (composite), we compute index as such - BASE_INDEX + id (id is base (0), 1 - negative, 2 - positive).
            //so keyboard is at index 0 (returns Q/E) and takes till index 2.
            //and Controller is at index 3 (returns D-Pad Left/D-Pad Right) and takes till 5.
            
            if (isGamepad)
            {
                str = action.GetBindingDisplayString(3, InputBinding.DisplayStringOptions.DontIncludeInteractions);
            }
            else // Defaults to keyboard
            {
                str = action.GetBindingDisplayString(0, InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
            }
            var split = str.Split('/');
            return showPositive ? split[1] : split[0];
        }

        private static string Reformat(string str)
        {
            if (retargets == null)
            {
                var txt = Resources.Load<TextAsset>("KeyCodeRetargets");
                retargets = JsonConvert.DeserializeObject<Dictionary<string, string>>(txt.text);
            }

            if (retargets.ContainsKey(str))
            {
                return retargets[str];
            }
            
            return str;
        }
    }
}