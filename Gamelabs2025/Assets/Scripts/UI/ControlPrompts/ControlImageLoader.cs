using System.Collections.Generic;
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
        private static readonly string PGamepadSearchPath = "P5Gamepad/T_P5_{0}_Light";
        private static readonly string KeybaordSearchPath = "Keyboard_Mouse/T_{0}_Key_White";
        
        public static Sprite Load(string strKey)
        {
            Debug.Log(strKey);
            if(images.ContainsKey(strKey))
                return images[strKey];
            
            Sprite sprite = null;
            var gamepad = Gamepad.current;
            var keyboard = Keyboard.current;
            string searchpath="";
            if (gamepad != null && gamepad.wasUpdatedThisFrame)
            {
                if (gamepad is XInputController)
                {
                    searchpath = string.Format(XGamepadSearchPath, strKey);
                }
                else //hoping PS5 and PS4 have the same key names
                    searchpath = string.Format(PGamepadSearchPath, strKey);
            }
            else
            {
                searchpath = string.Format(KeybaordSearchPath, strKey);
            }
            
            sprite = Resources.Load<Sprite>(searchpath);
            images[strKey] = sprite;
            return sprite;
        }
    }
}