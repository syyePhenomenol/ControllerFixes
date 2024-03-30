namespace ControllerFixes;

public static class ModMenu
{
    private static Menu MenuRef;
    private static GlobalSettings settings => ControllerFixes.settings;
    private static readonly List<string> Descriptions = new ()
    {
        "Normal controller prompts",
        "Joy-Cons image, Nintendo prompts",
        "DS4, PS prompts (Prompt for Inventory: Touchpad)",
        "DS4, PS prompts (Prompt for Inventory: Share button)",
        "DS4, PS prompts (Prompt for Inventory: Back/Select button)",
        "Xbox One, XB prompts (Prompt for Inventory: View)",
        "Xbox 360, XB prompts (Prompt for Inventory: Back/Select)",
    };

    public static MenuScreen CreateMenuScreen(MenuScreen modListMenu)
    {
        MenuRef = new Menu("Controller Fixes", new Element[]
        {
            new MenuButton("Link to explanations",
                "If anything is unclear, please read the steam discussion that this mod is based on",
                _ => Application.OpenURL("https://steamcommunity.com/sharedfiles/filedetails/?id=2525497719")),
            
            Blueprints.HorizontalBoolOption("No Cast",
                "Remove cast action from focus button/key",
                b => settings.NoCast = b,
                () => settings.NoCast),

            new HorizontalOption("Button Skin Type", 
            Descriptions[settings.ButtonSkinType + 1],
            new[] {"Default", "1", "2", "3", "4", "5", "6"},
            (i) =>
            {
                settings.ButtonSkinType = i - 1;

                (MenuRef.Find("ButtonPrompt") as HorizontalOption)!.Description = Descriptions[i];
                MenuRef.Update();
            },
            () => settings.ButtonSkinType + 1,
            Id: "ButtonPrompt"),
        
            Blueprints.HorizontalBoolOption("Emulated Xbox Only", 
            "Forces game to use xbox 360 controller",
            b => settings.EmulatedXboxOnly = b,
            () => settings.EmulatedXboxOnly),
            
            Blueprints.HorizontalBoolOption("Steam Nintendo Layout",
                "Use Steam's nintendo controller layout",
                b => settings.SteamNintendoLayout = b,
                () => settings.SteamNintendoLayout),

            Blueprints.HorizontalBoolOption("Override Menu Prompts",
                "For menu buttons, always display the menu binding regardless of contradicting jump/focus mappings.",
                b => settings.OverrideMenuPrompts = b,
                () => settings.OverrideMenuPrompts)
        });

        return MenuRef.GetMenuScreen(modListMenu);
    }
}