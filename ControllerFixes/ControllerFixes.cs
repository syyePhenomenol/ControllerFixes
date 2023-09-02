using HKMirror.Hooks.ILHooks;
using HKMirror.Reflection;

namespace ControllerFixes;

public class ControllerFixes : Mod, IGlobalSettings<GlobalSettings>, ICustomMenuMod
{
    internal static ControllerFixes Instance;
    public static GlobalSettings settings { get; private set; } = new ();
    public void OnLoadGlobal(GlobalSettings s) => settings = s;
    public GlobalSettings OnSaveGlobal() => settings;
    
    public ControllerFixes() : base("Controller Fixes") { }
    public override string GetVersion() => AssemblyUtils.GetAssemblyVersionHash();

    public override void Initialize()
    {
        Instance ??= this;

        // The following fixes some controllers (e.g. Xbox) not working on Mac
        ReflectionHelper.SetProperty<bool>(typeof(InputManager), "NativeInputEnableMFi", true);
        InputManager.Reload();

        On.ControllerDetect.LookForActiveController += LookForActiveController;

        On.HeroController.CanCast += CanCast;

        ILInputManager.AttachDevice += AttachDeviceIL;
        
        // there are 3 overloads and luckily the correct overload is edited by mapi so i can il hook the orig_ version
        ILUIButtonSkins.orig_GetButtonSkinFor += GetButtonSkinForIL;
    }

    private bool CanCast(On.HeroController.orig_CanCast orig, HeroController self)
    {
	    //get value of original conditions
	    var canCast = orig(self);

	    if (settings.NoCast)
	    {
		    canCast = canCast && !InputHandler.Instance.inputActions.cast.WasReleased;
	    }

	    return canCast;

    }

    private void LookForActiveController(On.ControllerDetect.orig_LookForActiveController orig, ControllerDetect self)
    {
	    var controllerDetect = self.Reflect();
	    
	    if (InputHandler.Instance.gamepadState == GamepadState.DETACHED)
	    {
		    controllerDetect.HideButtonLabels();
		    controllerDetect.controllerImage.sprite = self.controllerImages[0].sprite;
		    UIManager.instance.ShowCanvasGroup(self.controllerPrompt);
		    controllerDetect.remapButton.gameObject.SetActive(false);
		    return;
	    }
	    if (InputHandler.Instance.activeGamepadType != GamepadType.NONE)
	    {
		    UIManager.instance.HideCanvasGroup(controllerDetect.controllerPrompt);
		    controllerDetect.remapButton.gameObject.SetActive(true);
		    
		    //added code
		    if (settings.ButtonSkinType >= 0)
		    {
			    controllerDetect.ShowController(settings.ButtonSkinType switch
			    {
				    0 => GamepadType.SWITCH_JOYCON_DUAL,
				    1 or 2 => GamepadType.PS4,
				    3 => GamepadType.PS3_WIN,
				    4 => GamepadType.XBOX_ONE,
				    5 => GamepadType.XBOX_360,
				    _ => InputHandler.Instance.activeGamepadType
			    });
		    }
	    }
	    else
	    {
		    controllerDetect.ShowController(InputHandler.Instance.activeGamepadType);
	    }
	    
    }

    private void GetButtonSkinForIL(ILContext il)
    {
	    var cursor = new ILCursor(il).Goto(0);

	    //go just before the last line
	    cursor.GotoNext(MoveType.Before, i => i.MatchRet());
        //get the current instance of the class for geting values
        cursor.Emit(OpCodes.Ldarg_0);
        
        //get value of first argument (input control type)
        cursor.Emit(OpCodes.Ldarg_1);
        
        //the ButtonSkin is already on the stack before the ret

        //take in the button skin (return value), the class instance and first argument of the function
        //and use that to add this piece of code just before the function returns
        cursor.EmitDelegate<Func<ButtonSkin,UIButtonSkins,InputControlType, ButtonSkin>>(
	        (buttonSkin, _uIButtonSkins, inputControlType) =>
	        {
		        var uIButtonSkins = _uIButtonSkins.Reflect();
		        if (settings.ButtonSkinType >= 0)
		        {
			        int buttonSkinType = settings.ButtonSkinType;
			        int num = (buttonSkinType != 0) ? ((buttonSkinType > 3) ? 2 : 1) : 0;
			        switch (inputControlType)
			        {
				        case InputControlType.DPadUp:
				        {
					        Sprite[] array = new Sprite[]
					        {
						        uIButtonSkins.switchHidDPadUp,
						        uIButtonSkins.dpadUp,
						        uIButtonSkins.dpadUp,
					        };
					        buttonSkin.sprite = array[num];
					        break;
				        }
				        case InputControlType.DPadDown:
				        {
					        Sprite[] array2 = new Sprite[]
					        {
						        uIButtonSkins.switchHidDPadDown,
						        uIButtonSkins.dpadDown,
						        uIButtonSkins.dpadDown,
					        };
					        buttonSkin.sprite = array2[num];
					        break;
				        }
				        case InputControlType.DPadLeft:
				        {
					        Sprite[] array3 = new Sprite[]
					        {
						        uIButtonSkins.switchHidDPadLeft,
						        uIButtonSkins.dpadLeft,
						        uIButtonSkins.dpadLeft,
					        };
					        buttonSkin.sprite = array3[num];
					        break;
				        }
				        case InputControlType.DPadRight:
				        {
					        Sprite[] array4 = new Sprite[]
					        {
						        uIButtonSkins.switchHidDPadRight,
						        uIButtonSkins.dpadRight,
						        uIButtonSkins.dpadRight,
					        };
					        buttonSkin.sprite = array4[num];
					        break;
				        }
				        case InputControlType.LeftTrigger:
				        {
					        Sprite[] array5 = new Sprite[]
					        {
						        uIButtonSkins.switchHidLeftTrigger,
						        uIButtonSkins.ps4lt,
						        uIButtonSkins.lt,
					        };
					        buttonSkin.sprite = array5[num];
					        break;
				        }
				        case InputControlType.RightTrigger:
				        {
					        Sprite[] array6 = new Sprite[]
					        {
						        uIButtonSkins.switchHidRightTrigger,
						        uIButtonSkins.ps4rt,
						        uIButtonSkins.rt,
					        };
					        buttonSkin.sprite = array6[num];
					        break;
				        }
				        case InputControlType.LeftBumper:
				        {
					        Sprite[] array7 = new Sprite[]
					        {
						        uIButtonSkins.switchHidLeftBumper,
						        uIButtonSkins.ps4lb,
						        uIButtonSkins.lb,
					        };
					        buttonSkin.sprite = array7[num];
					        break;
				        }
				        case InputControlType.RightBumper:
				        {
					        Sprite[] array8 = new Sprite[]
					        {
						        uIButtonSkins.switchHidRightBumper,
						        uIButtonSkins.ps4rb,
						        uIButtonSkins.rb,
					        };
					        buttonSkin.sprite = array8[num];
					        break;
				        }
				        case InputControlType.Action1:
				        {
					        Sprite[] array9 = new Sprite[]
					        {
						        settings.SteamNintendoLayout ? uIButtonSkins.switchHidA : uIButtonSkins.switchHidB,
						        uIButtonSkins.ps4x,
						        uIButtonSkins.a,
					        };
					        buttonSkin.sprite = array9[num];
					        break;
				        }
				        case InputControlType.Action2:
				        {
					        Sprite[] array10 = new Sprite[]
					        {
						        settings.SteamNintendoLayout ? uIButtonSkins.switchHidB : uIButtonSkins.switchHidA,
						        uIButtonSkins.ps4circle,
						        uIButtonSkins.b,
					        };
					        buttonSkin.sprite = array10[num];
					        break;
				        }
				        case InputControlType.Action3:
				        {
					        Sprite[] array11 = new Sprite[]
					        {
						        settings.SteamNintendoLayout ? uIButtonSkins.switchHidX : uIButtonSkins.switchHidY,
						        uIButtonSkins.ps4square,
						        uIButtonSkins.x,
					        };
					        buttonSkin.sprite = array11[num];
					        break;
				        }
				        case InputControlType.Action4:
				        {
					        Sprite[] array12 = new Sprite[]
					        {
						        settings.SteamNintendoLayout ? uIButtonSkins.switchHidY : uIButtonSkins.switchHidX,
						        uIButtonSkins.ps4triangle,
						        uIButtonSkins.y,
					        };
					        buttonSkin.sprite = array12[num];
					        break;
				        }
				        default:
				        {
					        switch (inputControlType)
					        {
						        case InputControlType.Back:
						        case InputControlType.Select:
						        case InputControlType.Share:
						        case InputControlType.View:
							        break;
						        case InputControlType.Start:
						        case InputControlType.Options:
						        case InputControlType.Menu:
						        {
							        Sprite[] array13 = new Sprite[]
							        {
								        uIButtonSkins.switchHidPlus,
								        uIButtonSkins.options,
								        uIButtonSkins.options,
								        uIButtonSkins.start,
								        uIButtonSkins.menu,
								        uIButtonSkins.start,
							        };
							        buttonSkin.sprite = array13[buttonSkinType];
							        return buttonSkin;
						        }
						        case InputControlType.System:
						        case InputControlType.Pause:
						        case InputControlType.Home:
							        return buttonSkin;
						        default:
							        if (inputControlType != InputControlType.TouchPadButton)
							        {
								        return buttonSkin;
							        }

							        break;
					        }

					        Sprite[] array14 = new Sprite[]
					        {
						        uIButtonSkins.switchHidMinus,
						        uIButtonSkins.touchpadButton,
						        uIButtonSkins.share,
						        uIButtonSkins.select,
						        uIButtonSkins.view,
						        uIButtonSkins.select,
					        };
					        buttonSkin.sprite = array14[buttonSkinType];
					        break;
				        }
			        }
		        }

		        return buttonSkin;
	        });

    }


    private void AttachDeviceIL(ILContext il)
    {
	    var cursor = new ILCursor(il).Goto(0);
        //replace the condition in
        /*if (!inputDevice.IsSupportedOnThisPlatform)
        {
            return;
        }*/
        cursor.GotoNext(MoveType.After, i => i.MatchCallvirt(out _));
        
        //i want the value of first arg (the input device) so i emit it and take it into the func (is ldarg0 cuz its static)
        cursor.Emit(OpCodes.Ldarg_0);
        
        //get the original return value and the value of first argument and replace the bool with my conditions
        cursor.EmitDelegate<Func<bool, InputDevice, bool>>((oldvalue, inputDevice) =>
        {
            if (settings.EmulatedXboxOnly && !inputDevice.Name.Equals("XInput Controller") &&
                !inputDevice.Name.Equals("Xbox 360 Controller") && !inputDevice.Name.Equals("XBox 360 Controller") &&
                !inputDevice.Name.Equals("Microsoft Xbox 360 Controller"))
            {
                //we are before a brtrue, so we need to return false to not branch and reach the return statement
                return false;
            }

            if (inputDevice.IsUnknown)
            {
                return false;
            }

            //dont edit the code
            return oldvalue;
        });
    }

    public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? _) => 
	    ModMenu.CreateMenuScreen(modListMenu);

    public bool ToggleButtonInsideMenu => false;
}