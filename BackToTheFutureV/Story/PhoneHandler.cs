//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using BackToTheFutureV.Players;
//using GTA;
//using GTA.Native;

//namespace BackToTheFutureV.Story
//{
//    public delegate void OnCallRecieve(bool pickedUp);

//    public class PhoneHandler
//    {
//        /// <summary>
//        /// Returns the handle for the phone scaleform, that can be used in scaleform-related functions.
//        /// </summary>
//        public static int Handle
//        {
//            get
//            {
//                var model = (uint)Main.PlayerPed.Model.Hash;
//                switch (model)
//                {
//                    case (uint)PedHash.Franklin:
//                        return Function.Call<int>(Hash.REQUEST_SCALEFORM_MOVIE, "cellphone_badger");
//                    case (uint)PedHash.Trevor:
//                        return Function.Call<int>(Hash.REQUEST_SCALEFORM_MOVIE, "cellphone_facade");

//                    default:
//                        return Function.Call<int>(Hash.REQUEST_SCALEFORM_MOVIE, "cellphone_ifruit");
//                }
//            }
//        }

//        /// <summary>
//        /// Returns whether the player is in a call or not.
//        /// </summary>
//        public static bool IsInCall => IsInCustomCall || IsInNormalCall;

//        /// <summary>
//        /// Returns whether the player is in a normal call.
//        /// </summary>
//        public static bool IsInNormalCall => Function.Call<bool>(Hash.IS_SCRIPTED_CONVERSATION_ONGOING);

//        /// <summary>
//        /// Returns whether the player is in a custom call or not.
//        /// </summary>
//        public static bool IsInCustomCall { get; private set; }

//        /// <summary>
//        /// Returns whether the playe is getting ringed.
//        /// </summary>
//        public static bool IsRingingWithCall { get; private set; }

//        /// <summary>
//        /// Set icon of the soft key buttons directly.
//        /// </summary>
//        /// <param name="buttonID">The button index</param>
//        /// <param name="icon">Supplied icon</param>
//        public static void SetSoftKeyIcon(int buttonID, SoftKeyIcon icon)
//        {
//            Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION, Handle, "SET_SOFT_KEYS");
//            Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_INT, buttonID);
//            Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_BOOL, true);
//            Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_INT, (int)icon);
//            Function.Call(Hash._POP_SCALEFORM_MOVIE_FUNCTION_VOID);
//        }

//        /// <summary>
//        /// Simulates a call from any caller.
//        /// </summary>
//        /// <param name="callerName">Name that will be displayed, the caller name.</param>
//        /// <param name="picName">The id of the picture that will be displayed.</param>
//        /// <param name="onCallRecieve">Callback for when the call is recieved.</param>
//        /// <returns>Whether the call was succesfully simulated or not.</returns>
//        public static bool SimulateCall(string callerName, string picName = "CELL_300", int callDieAfter = 8000, OnCallRecieve onCallRecieve = null)
//        {
//            if (IsInCall)
//                return false;

//            Function.Call(Hash.TERMINATE_ALL_SCRIPTS_WITH_THIS_NAME, "cellphone_flashhand");

//            DisplayCallUI(Handle, callerName, picName);

//            //ringtone.Play();

//            contactName = callerName;
//            imageName = picName;
//            callRecieveCallBack = onCallRecieve;
//            ringingTimeout = Game.GameTime + 8000;
//            activeDelay = callDieAfter;

//            IsInCustomCall = false;
//            IsRingingWithCall = true;

//            return true;
//        }

//        public static void Update()
//        {
//            if(IsRingingWithCall)
//            {
//                DisplayCallUI(Handle, contactName, imageName);

//                //Disable normal phone controls
//                Game.DisableControlThisFrame(1, Control.PhoneSelect);
//                Game.DisableControlThisFrame(1, Control.PhoneScrollBackward);
//                Game.DisableControlThisFrame(1, Control.PhoneScrollForward);
//                Game.DisableControlThisFrame(1, Control.PhoneUp);
//                Game.DisableControlThisFrame(1, Control.PhoneCancel);
//                Game.DisableControlThisFrame(1, Control.PhoneDown);
//                Game.DisableControlThisFrame(1, Control.PhoneLeft);
//                Game.DisableControlThisFrame(1, Control.PhoneRight);
//                Game.DisableControlThisFrame(1, Control.PhoneOption);

//                if (Game.IsControlJustPressed(2, Control.PhoneSelect))
//                {
//                    IsRingingWithCall = false;
//                    ringingTimeout = 0;

//                    callRecieveCallBack?.Invoke(true);

//                    ringtone.Stop();

//                    IsInCustomCall = true;
//                    activeTimeout = Game.GameTime + activeDelay;
//                }
//                else if(Game.GameTime > ringingTimeout || Game.IsControlJustPressed(2, Control.PhoneCancel))
//                {
//                    IsRingingWithCall = false;
//                    ringingTimeout = 0;

//                    ringtone.Stop();

//                    callRecieveCallBack?.Invoke(false);

//                    Game.SetControlNormal(2, Control.PhoneCancel, -1f);
//                }
//            }

//            if(IsInCustomCall)
//            {
//                DisplayConnectedUI(Handle, contactName, imageName);

//                //Disable normal phone controls
//                Game.DisableControlThisFrame(1, Control.PhoneSelect);
//                Game.DisableControlThisFrame(1, Control.PhoneScrollBackward);
//                Game.DisableControlThisFrame(1, Control.PhoneScrollForward);
//                Game.DisableControlThisFrame(1, Control.PhoneUp);
//                Game.DisableControlThisFrame(1, Control.PhoneCancel);
//                Game.DisableControlThisFrame(1, Control.PhoneDown);
//                Game.DisableControlThisFrame(1, Control.PhoneLeft);
//                Game.DisableControlThisFrame(1, Control.PhoneRight);
//                Game.DisableControlThisFrame(1, Control.PhoneOption);

//                if (Game.GameTime > activeTimeout)
//                {
//                    IsInCustomCall = false;
//                    activeDelay = 0;
//                    activeTimeout = 0;

//                    Function.Call(Hash.REQUEST_SCRIPT, "cellphone_controller");
//                }
//            }
//        }

//        private static OnCallRecieve callRecieveCallBack;
//        private static string contactName;
//        private static string imageName;
//        private static int ringingTimeout;
//        private static int activeTimeout;
//        private static int activeDelay;
//        private static AudioPlayer ringtone = new AudioPlayer("franklin_ringtone.wav", true);

//        private static void DisplayCallUI(int handle, string contactName, string picName)
//        {
//            Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION, handle, "SET_DATA_SLOT_EMPTY");
//            Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_INT, 4);
//            Function.Call(Hash._POP_SCALEFORM_MOVIE_FUNCTION_VOID);

//            Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION, handle, "SET_DATA_SLOT");
//            Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_INT, 4);
//            Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_INT, 0);
//            Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_INT, 2);

//            Function.Call(Hash._BEGIN_TEXT_COMPONENT, "CELL_CONDFON");
//            Function.Call(Hash._0x761B77454205A61D, contactName, -1);
//            Function.Call(Hash._END_TEXT_COMPONENT);

//            Function.Call(Hash._BEGIN_TEXT_COMPONENT, picName);
//            Function.Call(Hash._END_TEXT_COMPONENT);

//            Function.Call(Hash._BEGIN_TEXT_COMPONENT, "CELL_217");
//            Function.Call(Hash._END_TEXT_COMPONENT);

//            Function.Call(Hash._POP_SCALEFORM_MOVIE_FUNCTION_VOID);

//            SetSoftKeyIcon(2, SoftKeyIcon.Call);
//            SetSoftKeyIcon(3, SoftKeyIcon.Hangup);

//            Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION, handle, "DISPLAY_VIEW");
//            Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_INT, 4);
//            Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_INT, 0);
//            Function.Call(Hash._POP_SCALEFORM_MOVIE_FUNCTION_VOID);
//        }

//        private static void DisplayConnectedUI(int handle, string contactName, string picName)
//        {
//            Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION, handle, "SET_DATA_SLOT_EMPTY");
//            Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_INT, 4);
//            Function.Call(Hash._POP_SCALEFORM_MOVIE_FUNCTION_VOID);

//            Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION, handle, "SET_DATA_SLOT");
//            Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_INT, 4);
//            Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_INT, 0);
//            Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_INT, 2);

//            Function.Call(Hash._BEGIN_TEXT_COMPONENT, "STRING");
//            Function.Call(Hash._0x761B77454205A61D, contactName, -1);
//            Function.Call(Hash._END_TEXT_COMPONENT);

//            Function.Call(Hash._BEGIN_TEXT_COMPONENT, picName);
//            Function.Call(Hash._END_TEXT_COMPONENT);

//            Function.Call(Hash._BEGIN_TEXT_COMPONENT, "STRING");
//            Function.Call(Hash._0x761B77454205A61D, "CONNECTED", -1);
//            Function.Call(Hash._END_TEXT_COMPONENT);

//            Function.Call(Hash._POP_SCALEFORM_MOVIE_FUNCTION_VOID);

//            SetSoftKeyIcon(2, SoftKeyIcon.Blank);
//            SetSoftKeyIcon(3, SoftKeyIcon.Hangup);

//            Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION, handle, "DISPLAY_VIEW");
//            Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_INT, 4);
//            Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_INT, 0);
//            Function.Call(Hash._POP_SCALEFORM_MOVIE_FUNCTION_VOID);
//        }

//        private static void DisplayCustomPhone(int handle)
//        {
//            Function.Call(Hash.DRAW_SCALEFORM_MOVIE, handle, 0.0998f, 0.1775f, 0.1983f, 0.364f, 255, 255, 255, 255, 1);
//        }
//    }
//}
