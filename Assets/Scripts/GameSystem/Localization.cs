﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using FullSerializer;


/// <summary>
/// A language list that can be edited by user
/// </summary>
public struct Language
{
    [SerializeField]
    public string name;
    [SerializeField]
    public string code;
    [SerializeField]
    public int unitynumber;

    public Language(string _name, string _code, int _number)
    {
        name = _name;
        code = _code;
        unitynumber = _number;
    }
}

/// <summary>
/// Meta data for in-game subtitles
/// </summary>
/// 
public struct subtitleMeta
{
    [SerializeField]
    public string subtitle;
    [SerializeField]
    public float delay;
    [SerializeField]
    public float duration;
    [SerializeField]
    public string nextSubtitle;
    [SerializeField]
    public string character;
    [SerializeField]
    public bool noFormat;
}

public static class Localization
{
    static string folderPath = Path.Combine(Application.streamingAssetsPath, GlobalValues.localName);
    static string langCode = "EN";




    static Dictionary<string, Dictionary<string, string>> defStrings = new Dictionary<string, Dictionary<string, string>>();
    static Dictionary<string, Dictionary<string, string>> localStrings = new Dictionary<string, Dictionary<string, string>>();

    static Dictionary<string, subtitleMeta> defSub = new Dictionary<string, subtitleMeta>();
    static Dictionary<string, subtitleMeta> localSub = new Dictionary<string, subtitleMeta>();

    //Localization

    static Dictionary<int, Language> def_langs = new Dictionary<int, Language>()
    {
        {10, new Language ("English", "EN", 10) },
        {34, new Language ("Español", "ES", 34) },
        {15, new Language ("Deutsch", "DE", 15) },
        {6, new Language ("简体中文", "CH", 6) },
        {40, new Language ("简体中文", "CH", 6) },
        {41, new Language ("简体中文", "CH", 6) },
    };

    static Dictionary<int, Language> langs;


    static public void SetLanguage(int lang)
    {
        if (lang == -1)
            lang = (int)Application.systemLanguage;

        defStrings = new Dictionary<string, Dictionary<string, string>>();
        localStrings = new Dictionary<string, Dictionary<string, string>>();

        defStrings.Add("uiStrings", uiStrings_EN);
        defStrings.Add("itemStrings", itemStrings_EN);
        defStrings.Add("charaStrings", charaStrings_EN);
        defStrings.Add("playStrings", playStrings_EN);
        defStrings.Add("loadStrings", loadStrings_EN);
        defStrings.Add("deathStrings", deathStrings_EN);
        defStrings.Add("tutoStrings", tutoStrings_EN);
        defSub = GetSubtitles();

        if (langs.ContainsKey(lang))
        {
            langCode = langs[lang].code;

            foreach (var table in defStrings)
            {
                localStrings.Add(table.Key, GetTable(table.Key));
            }
        }
        else
        {
            localStrings.Add("uiStrings", new Dictionary<string, string>());
            localStrings.Add("itemStrings", new Dictionary<string, string>());
            localStrings.Add("playStrings", new Dictionary<string, string>());
            localStrings.Add("charaStrings", new Dictionary<string, string>());
            localStrings.Add("loadStrings", new Dictionary<string, string>());
            localStrings.Add("deathStrings", new Dictionary<string, string>());
            localStrings.Add("tutoStrings", new Dictionary<string, string>());
        }

        localSub = new Dictionary<string, subtitleMeta>();
    }



    static public string GetString(string table, string id)
    {
        if (localStrings.ContainsKey(table))
        {
            if (localStrings[table].ContainsKey(id))
            {
                return localStrings[table][id];
            }
            else if (defStrings[table].ContainsKey(id))
            {
                return defStrings[table][id];
            }
            else
                return ("Missing Subtitle: " + id);

        }
        else
            return ("Missing Table " + table);
    }

    static public subtitleMeta GetSubtitle(string id)
    {
        subtitleMeta temp = new subtitleMeta();
        temp.subtitle = "MISSING SUBTITLE";
        temp.noFormat = true;
    

        if (localSub.ContainsKey(id))
        {
            temp = localSub[id];
        }
        else if (defSub.ContainsKey(id))
        {
            temp = defSub[id];
        }

        return temp;
    }

    static public void ExportDefault()
    {
        foreach (var table in defStrings)
        {
            Debug.Log("Exportando tabla " + table.Key);
            SaveTable(table.Key, "EN", table.Value);
        }
    }

    static public void BuildSubsDefault()
    {
        Dictionary<string, subtitleMeta> tempSubs = new Dictionary<string, subtitleMeta>();
        foreach (var tcadena in sceneStrings_EN)
        {
            subtitleMeta sub = new subtitleMeta();
            sub.subtitle = tcadena.Value;
            sub.delay = 0;
            sub.duration = 6;
            sub.nextSubtitle = "";
            sub.character = "CHANGE_THIS";
            sub.noFormat = false;

            tempSubs.Add(tcadena.Key, sub);
        }

        SaveSub("sceneStrings", "EN", tempSubs);

    }

    static void SaveSub(string FileName, string LanguageCode, Dictionary<string, subtitleMeta> table)
    {
        fsSerializer _serializer = new fsSerializer();

        if (!Directory.Exists(Path.Combine(folderPath, LanguageCode)))
            Directory.CreateDirectory(Path.Combine(folderPath, LanguageCode));

        fsData data;
        _serializer.TrySerialize(table, out data).AssertSuccessWithoutWarnings();

        // emit the data via JSON
        using (StreamWriter streamWriter = File.CreateText(Path.Combine(folderPath, LanguageCode, FileName + ".subs")))
        {
            streamWriter.Write(fsJsonPrinter.PrettyJson(data));
        }
    }

    static void SaveTable(string FileName, string LanguageCode, Dictionary<string, string> table)
    {
        fsSerializer _serializer = new fsSerializer();

        if (!Directory.Exists(Path.Combine(folderPath, LanguageCode)))
            Directory.CreateDirectory(Path.Combine(folderPath, LanguageCode));

        fsData data;
        _serializer.TrySerialize(table, out data).AssertSuccessWithoutWarnings();

        // emit the data via JSON
        using (StreamWriter streamWriter = File.CreateText(Path.Combine(folderPath, LanguageCode, FileName + ".subs")))
        {
            streamWriter.Write(fsJsonPrinter.PrettyJson(data));
        }
    }

    public static void AddMissing()
    {
        Debug.Log("Iniciando Proceso");
        foreach (var table in defStrings)
        {
            Dictionary<string, string> currTable = new Dictionary<string, string>();

            if (!File.Exists(Path.Combine(folderPath, langCode, table.Key + ".subs")))
            {
                Debug.Log("Tabla no existe, agregando todos los valores");
                foreach (var value in table.Value)
                {
                    currTable.Add(value.Key, " MISSING SUBTITLE ");
                }
            }
            else
            {
                Debug.Log("Tabla si existe, examinando paso a paso");
                currTable = localStrings[table.Key];
                foreach (var value in table.Value)
                {
                    if (!localStrings[table.Key].ContainsKey(value.Key))
                    {
                        Debug.Log("Subtitulo faltante en " + value.Key);
                        currTable.Add(value.Key, " MISSING SUBTITLE ");
                    }
                }
            }

            SaveTable(table.Key, langCode, currTable);
        }
    }

    static Dictionary<string, string> GetTable(string FileName)
    {
        Debug.Log("Cargando tabla " + FileName + " en la posicion " + Path.Combine(folderPath, langCode, FileName + ".subs"));
        fsSerializer _serializer = new fsSerializer();
        Dictionary<string, string> loadedTable = new Dictionary<string, string>();

        if (File.Exists(Path.Combine(folderPath, langCode, FileName + ".subs")))
        {
            using (StreamReader streamReader = File.OpenText(Path.Combine(folderPath, langCode, FileName + ".subs")))
            {
                string jsonString = streamReader.ReadToEnd();
                fsData data = fsJsonParser.Parse(jsonString);

                _serializer.TryDeserialize(data, ref loadedTable).AssertSuccessWithoutWarnings();
            }
        }

        return loadedTable;
    }

    static Dictionary<string, subtitleMeta> GetSubtitles()
    {
        fsSerializer _serializer = new fsSerializer();
        Dictionary<string, subtitleMeta> loadedTable = new Dictionary<string, subtitleMeta>();

        if (File.Exists(Path.Combine(folderPath, langCode, "sceneStrings" + ".subs")))
        {
            using (StreamReader streamReader = File.OpenText(Path.Combine(folderPath, langCode, "sceneStrings" + ".subs")))
            {
                string jsonString = streamReader.ReadToEnd();
                fsData data = fsJsonParser.Parse(jsonString);

                _serializer.TryDeserialize(data, ref loadedTable).AssertSuccessWithoutWarnings();
            }
        }

        return loadedTable;
    }

    static public void CheckLangs()
    {
        fsSerializer _serializer = new fsSerializer();


        if (!File.Exists(Path.Combine(folderPath, "languages.langs")))
        {
            fsData data;
            _serializer.TrySerialize(def_langs, out data).AssertSuccessWithoutWarnings();

            // emit the data via JSON
            using (StreamWriter streamWriter = File.CreateText(Path.Combine(folderPath, "languages.langs")))
            {
                streamWriter.Write(fsJsonPrinter.PrettyJson(data));
            }
        }

        using (StreamReader streamReader = File.OpenText(Path.Combine(folderPath, "languages.langs")))
        {
            string jsonString = streamReader.ReadToEnd();
            fsData data = fsJsonParser.Parse(jsonString);

            langs = def_langs;
            _serializer.TryDeserialize(data, ref langs).AssertSuccessWithoutWarnings();
        }
    }

    static public Dictionary<int, Language> GetLangs()
    {
        int number = 0;
        Dictionary<int, Language> langList = new Dictionary<int, Language>()
        {
            {number, new Language ("Auto", "auto", 10) },
        };
        Debug.Log("Parsing Languages, detected " + langs.Count);
        foreach (var lang in langs)
        {
            number++;
            if (!langList.ContainsValue(lang.Value))
            {
                Debug.Log("Language " + lang.Value.name);
                langList.Add(number, lang.Value);
            }
        }

        return (langList);
    }







    /// <summary>
    /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>
    //~~~~~~~~~~~~~~~~~~~~~~~~~~UI ENGLISH STRINGS~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public static Dictionary<string, string> uiStrings_EN = new Dictionary<string, string>()
    {
        {"ui_generic_back", "Back"},
        {"ui_main_play", "Play"},
        {"ui_main_extras", "Extras"},
        {"ui_main_options", "Options"},
        {"ui_main_exit", "Exit"},

        {"ui_main_play_pro", "Prologue"},
        {"ui_main_play_chap", "Chapters"},

        {"ui_main_play_new", "New Game"},
        {"ui_main_play_load", "Load Game"},
        {"ui_main_play_start", "Start"},
        {"ui_main_play_seed", "Map Generation Seed"},
        {"ui_main_play_sname", "Savefile Name"},
        {"ui_main_play_intro", "Play Intro"},
        {"ui_main_play_sload", "Load"},
        {"ui_main_play_sdelete", "Delete"},

        {"ui_in_anykey", "Press any key to Start"},
        {"ui_in_pause", "PAUSE"},
        {"ui_in_info", "Designation: {0} \nName: {1} \n\nSave File: {2}\nMap Seed: {3}"},
        {"ui_in_resume", "Resume"},
        {"ui_in_save", "Save and Quit"},
        {"ui_in_quit", "Quit"},
        {"ui_in_death", "YOU DIED"},
        {"ui_in_load", "Load Save"},
        {"ui_in_saved", "Game Saved" },
        {"ui_in_nosave", "Can't save at this location" },


        {"ui_op_gfx", "Graphics"},
        {"ui_op_sfx", "Audio"},
        {"ui_op_input", "Input"},
        {"ui_op_misc", "Advanced"},

        {"ui_gfx_quality", "Graphics Quality"},
        {"ui_gfx_post", "Post Processing Quality"},
        {"ui_gfx_lang", "Language"},
        {"ui_gfx_vsync", "V-Sync"},
        {"ui_gfx_frame", "Set Framerate Target"},
        {"ui_gfx_gamma", "Screen Gamma"},

        {"ui_sfx_master", "Master Volume"},
        {"ui_sfx_music", "Music"},
        {"ui_sfx_ambiance", "Ambiance"},
        {"ui_sfx_sfx", "Sound Effects"},
        {"ui_sfx_voice", "Voice"},
        {"ui_sfx_sub", "Show Subtitles"},

        {"ui_input_invert", "Invert Mouse Y-Axis"},
        {"ui_input_acc", "Mouse Acceleration"},

        {"ui_misc_debug", "Debug Console"},
        {"ui_misc_tuto", "Show In-game Tutorials"},

        {"ui_demo_end", "Thanks for Playing the our Demo" },
        {"ui_demo_end_body", "Our first objective with this remaster is to replicate the same ambiance and feeling from the original <color=white><i>SCP: Containment Breach</i></color> in a more detailed rendering engine, and a (hopefully!) more reliable engine.\n\nWe hope you think we have delivered!\n\nWe have a long way too go to reach feature parity with the current version. However as a creator, you have that itch to both perfect your work, but also get in on the hands of the people who will enjoy it. I believe I have turned this engine into something that will deliver a definitive SCP:CB experience quick and in high quality.\n\nYou can keep playing and explore the work we have in the Heavy Containment Zone by reloading this save.\n\nKeep an eye out for the next major update! Hopefully there you'll have a taste of the new features and ideas we want to build into the main game." },

        {"ui_map_noconnect", "UNABLE TO CONNECT TO MAP DATABASE" },
        {"ui_map_lock", "LOCKDOWN IN AREAS: " },
        {"ui_map_heavy", "HEAVY CONT. ZONE " },
        {"ui_map_entrance", "ENTRANCE ZONE " },

        {"ui_radio_channel2", " WARNING - CONTAINMENT BREACH " },
        {"ui_radio_channel3", " SCP Foundation On-Site Radio " },
        {"ui_radio_channel4", " EMERGENCY CHANNEL - RESERVED FOR COMMUNICATION IN THE EVENT OF A CONTAINMENT BREACH " },



    };

    public static Dictionary<string, string> loadStrings_EN = new Dictionary<string, string>()
    {
        {"title_173", "SCP-173"},
        {"body1_173", "SCP-173 is constructed from concrete and rebar with traces of Krylon brand spray paint. It is animate and extremely hostile."},
        {"body2_173", "The object cannot move while within a direct line of sight. Line of sight must not be broken at any time with SCP-173. Personnel assigned to enter container are instructed to alert one another before blinking."},

        {"title_scp", "The SCP Foundation"},
        {"body1_scp", "The SCP Foundation is an organization dedicated to the containment and research of anomalous artifacts and lifeforms."},
        {"body2_scp", "''SCP'' stands for ''Special Containment Procedures'' (and the official motto of the foundation, ''Secure, Contain, Protect''.) - which sums up both the goals and methods of the Foundation's actions."},

        {"title_dclass", "Class-D Personnel"},
        {"body1_dclass", "Class-D personnel are designated staff used to handle the Keter-level objects."},
        {"body2_dclass", "Class-D personnel are recruited from prison inmates. Condemned persons are preferred; in times of duress, Protocol 12 can be authorized, allowing recruitment of innocents or persons incarcerated for lesser crimes."},
        {"body3_dclass", "All Class-D personnel are to be terminated at the first of the month, and a new staff must be ready to replace them."},

        {"title_012", "SCP-012" },
        {"body1_012" , "SCP-012 was retrieved by Archaeologist K.M. Sandoval during the excavation of a northern Italian tomb. The object, a piece of handwritten musical score entitled \"On Mount Golgotha\", appears to be incomplete."},
        {"body2_012" , "The red/black ink, first thought to be some form of berry or natural dye ink, was later found to be human blood from multiple subjects." },
        {"body3_012" , "Multiple test subjects have been allowed access to the score. In every case, the subjects mutilated themselves in order to use their own blood to finish the piece, resulting in subsequent symptoms of psychosis and massive trauma." },

        {"title_106", "SCP-106" },
        {"body1_106","SCP-106 appears to be an elderly humanoid, with a general appearance of advanced decomposition. This appearance may vary, but the \"rotting\" quality is observed in all forms." },
        {"body2_106", "SCP-106 causes a \"corrosion\" effect in all solid matter it touches, engaging a physical breakdown in materials several seconds after contact. This is observed as rusting, rotting, and cracking of materials, and the creation of a black, mucus-like substance similar to the material coating SCP-106." },
        {"body3_106","SCP-106 can pass through solid matter and will capture and kill its prey by pulling it into what is assumed to be its personal \"pocket dimension\"."},

        {"title_294","SCP-294" },
        {"body1_294", "Item SCP-294 appears to be a standard coffee vending machine, the only noticeable difference being an entry touchpad with buttons corresponding to an English QWERTY keyboard." },
        {"body2_294", "Upon entering the name of any liquid using the touchpad, a standard 12-ounce paper drinking cup is placed and the liquid indicated is poured." },
        {"body3_294", "Ninety-seven initial test runs were performed (including requests for water, coffee, beer, and soda, non-consumable liquids such as sulfuric acid, wiper fluid, and motor oil, as well as substances that do not usually exist in liquid state, such as nitrogen, iron and glass) and each one returned a success." },

        {"title_914", "SCP-914" },
        {"body1_914" , "SCP-914 is a large clockwork device weighing several tons and covering an area of eighteen square meters, consisting of screw drives, belts, pulleys, gears, springs and other clockwork." },
        {"body2_914","When an object is placed in the Intake Booth and the key is wound up, SCP-914 will \"refine\" the object. No energy is lost in the process, and the object is refined based on the setting specified on 914's front panel." },
        {"body3_914","No organic matter is to be entered in to SCP-914 at any time." },

        {"title_939", "SCP-939" },
        {"body1_939","SCP-939 are endothermic, pack-based predators which display atrophy of various systems similar to troglobitic organisms. SCP-939 average 2.2 meters tall standing upright and weigh an average of 250 kg, though weight is highly variable." },
        {"body2_939","SCP-939's primary method of luring prey is the imitation of human speech in the voices of prior victims, though imitation of other species and active nocturnal hunts have been documented." },
        {"body3_939","Prey is usually killed with a single bite to the cranium or neck; bite forces have been measured in excess of 35 MPa." },


    };

    public static Dictionary<string, string> tutoStrings_EN = new Dictionary<string, string>()
    {
        {"tutograb", "Click the Interact button when the Hand icon shows up to interact with different objects. Grab Items, move lever, etc."},
        {"tutoinv1", "Press the Inventory key to see your collected items. Click on the to equip them or use them"},
        {"tutoinv2", "Drop items into the slots of other items to combine them for different results. You can also drop them outside"},
        {"tutoinv3", "Card Readers around the facility require a keycard with the appropiate clearance. Some doors may be locked remotely. Find ways to unlock them"},
        {"tutodead", "You may find interesting items in the pockets of the victims of the breach"},
        {"tutorun", "Hold the sprint button to Sprint. Be aware your footsteps may attract creatures aware of their enviroment"},
        {"tutohide1", "Crouching and hiding behind objects can deter Enemies aware of their enviroment"},
        {"tutohide2", "Creatures can hear your footsteps. Crouch or walk slowly if you think they are aware of your presence"},
        {"tutoradio", "Press keys 1 to 5 to change the Radio Channel"},
        };

    public static Dictionary<string, string> playStrings_EN = new Dictionary<string, string>()
    {
        {"play_button_nocard", "You need a Keycard for this door"},
        {"play_button_lowcard", "You need a Keycard with a Higher clearance"},
        {"play_button_card", "You slide the Keycard in the slot"},
        {"play_button_failcard", "You slide the Keycard but nothing happened"},
        {"play_button_elev", "You called the Elevator"},
        {"play_button", "You pushed the button"},
        {"play_equip_fem", "You put on the {0}" },
        {"play_equip_male", "You put on the {0}" },
        {"play_equip_uni", "You put on {0}" },
        {"play_dequip_fem", "You removed the {0}" },
        {"play_dequip_male", "You removed the {0}" },
        {"play_dequip_uni", "You removed {0}" },
        {"play_used_fem", "You used the {0}" },
        {"play_used_male", "You used the {0}" },
        {"play_used_uni", "You used {0}" },
        {"play_picked_uni", "Picked up {0}" },
        {"play_picked_fem", "Picked up a {0}" },
        {"play_picked_male", "Picked up a {0}" },
        {"play_equiped", "Using {0}" },
        {"play_fullinv", "Can't carry more items" },
        {"play_cure", "You patch yourself up" },
        {"play_cureblood", "You patch yourself up a bit, and stop the bloodloss" },
        {"play_cureblood2", "You patch yourself up, but didnt managed to stop the bloodloss completely" },

        {"play_012_1", "You start pushing your nails into your wrist, drawing blood." },
        {"play_012_2", "You tear open your left wrist and start writing on the composition with your blood." },
        {"play_012_3", "You push your fingers deeper into the wound." },
        {"play_012_4", "You rip the wound wide open. Grabbing scoops of blood pouring out." },


    };

    public static Dictionary<string, string> itemStrings_EN = new Dictionary<string, string>()
    {
        {"bat_nor", "9V Battery"},
        {"doc_ori", "Orientation Leaflet"},
        {"doc_173", "173 Containment Procedures"},
        {"doc012", "SCP-012 Containment Procedures"},
        {"doc079", "SCP-079 Containment Procedures"},
        {"doc096", "SCP-096 Containment Procedures"},
        {"doc682", "SCP-682 Containment Procedures"},
        {"doc939", "SCP-939 Containment Procedures"},
        {"doc966", "SCP-966 Containment Procedures"},
        {"doc1048", "SCP-1048 Containment Procedures"},
        {"doc1123", "SCP-1123 Containment Procedures"},
        {"doc372", "SCP-372 Containment Procedures"},
        {"doc914", "SCP-914 Document"},

        {"docAC", "Burned Note"},

        {"docL1", "Dr. L Note"},

        {"doc500", "SCP-500 Document"},
        {"docSC", "Security Clearance Document"},
        {"docRAND3", "173 Procedures Revision"},


        {"deadpaper", "Shreded paper"},
        {"origami", "Origami"},
        {"clipboard", "Clipboard"},
        {"elec", "Destroyed Electronics"},

        {"gasmask1", "Gas Mask"},
        {"gasmask2", "Gas Mask"},
        {"eye", "Eye Drops"},
        {"vest", "Ballistic Vest"},
        {"key0", "Janitor Keycard"},
        {"key1", "Researcher Keycard LVL 1"},
        {"key2", "Researcher Keycard LVL 2"},
        {"key3", "Researcher Keycard LVL 3"},
        {"key4", "Agent Keycard LVL 4"},
        {"key5", "Agent Keycard LVL 5"},
        {"badge", "Badge"},
        {"keycredit", "Master Card"},
        {"keyj", "Playing Card"},
        {"keyomni", "OmniCard"},
        {"medkit1", "Light MedKit"},
        {"ring", "SCP-714"},
        {"snav", "S-Nav 300"},
        {"snav2", "S-Nav 310"},
        {"snav3", "S-Nav Ultimate"},
        {"radio", "Radio"},




    };

    public static Dictionary<string, string> sceneStrings_EN = new Dictionary<string, string>()
    {
        {"BeforeDoorOpen", "<b>{0}</b> : Control, this is Agent Ulgrin. I need to request open up Cell 3-11."},
        {"ExitCell", "<b>{0}</b> : Hey, they've got some work for ya. Do me a favor, and step out of your cell"},
        {"ExitCellRefuse1", "<b>{0}</b> : What are you stupid or something? I said step out of the cell. If you don't step out of the cell I'm gonna kick your ass."},
        {"ExitCellRefuse2", "<b>{0}</b> : Look buddy I don't have all day. I'm trying to be polite about this. If you don't step out of the cell I'm gonna kick your ass."},
        {"CellGas1", "<b>{0}</b> : Huh, you have got to be the dumbest test subject we've ever had. Oh well, shut the doors and open the gas valves."},
        {"CellGas2", "<b>{0}</b> : Huh, I'm actually kinda disappointed you didn't put up a fight. I was looking forward to punching you in the face."},
        {"EscortRun", "<b>{0}</b> : Hey, dumbass! You're going the wrong way. Get the hell over here right now!"},
        {"EscortRefuse1", "<b>{0}</b> : Hurry up! They're waiting for you."},
        {"EscortRefuse2", "<b>{0}</b> : Look dude, I already hate my job. Why are you making it more difficult for me?"},
        {"EscortPissedOff1", "<b>{0}</b> : I ain't in the mood for this shit, I have no problem putting a bullet in your brain if you don't start cooperating."},
        {"EscortPissedOff2", "<b>{0}</b> : I ain't in the mood for this shit, I have no problem putting a bullet in your brain if you don't start cooperating."},
        {"EscortKill1", "<b>{0}</b> : Alright, you know what? Fine, be that way. We'll just get somebody else then"},
        {"EscortKill2", "<b>{0}</b> : Alright fine, be that way. We'll just get somebody else then."},


        {"Intro_Convo1_1", "<b>{0}</b> : So, uh, how's it going?"},
        {"Intro_Convo1_2", "<b>{0}</b> : Uh, a-are you talking to me?"},
        {"Intro_Convo1_3", "<b>{0}</b> : Well yeah, who do you think I'm talking to, this guy with the punchable face? Course I'm talking to you."},
        {"Intro_Convo1_4", "<b>{0}</b> : Oh, I'm just a little surprised. I think this is the first time you've ever spoken to me."},
        {"Intro_Convo1_5", "<b>{0}</b> : Well yeah, it's your first day working here."},
        {"Intro_Convo1_6", "<b>{0}</b> : Uh, actually, we've worked together now for about 5 months."},
        {"Intro_Convo1_7", "<b>{0}</b> : Really? Wow. That's weird."},

        {"Intro_Convo2_1", "<b>{0}</b> : Uh, so you see any good movies lately?"},
        {"Intro_Convo2_2", "<b>{0}</b> : Uh, I don't really watch movies. I mostly read books."},
        {"Intro_Convo2_3", "<b>{0}</b> : Yeah? What kind of books?"},
        {"Intro_Convo2_4", "<b>{0}</b> : Uh, horror, science fiction, anything like that?"},
        {"Intro_Convo2_5", "<b>{0}</b> : You're kidding me."},
        {"Intro_Convo2_6", "<b>{0}</b> : What?"},
        {"Intro_Convo2_7", "<b>{0}</b> : Your whole job revolves around horror and science fiction, except, you know, it's not actually fiction."},
        {"Intro_Convo2_8", "<b>{0}</b> : Well, actually, I'm planning on writing a book about my exper-"},
        {"Intro_Convo2_9", "<b>{0}</b> : Yeah, look, no offense, but I've already lost interest in what you're talking about."},

        {"Intro_Convo3_1", "<b>{0}</b> : Man, I'm hungry. Hey, today's pizza day down at the cafeteria, right?"},
        {"Intro_Convo3_2", "<b>{0}</b> : Uh, a-actually, I think it's tuna casserole."},
        {"Intro_Convo3_3", "<b>{0}</b> : Oh, god dammit. Well, my day's ruined. The only reason I still come here is for the pizza. I don't know what it is about that pizza, but it's delicious. Tuna casserole, on the other hand, is a disgusting abomination and it should be locked up in here with the rest of these freaks."},
        {"Intro_Convo3_4", "<b>{0}</b> : Uh, okay.."},

        {"Intro_Convo4_1", "<b>{0}</b> : Let me guess. You don't have a girlfriend, do you?"},
        {"Intro_Convo4_2", "<b>{0}</b> : Uh, a-are you talking to me?"},
        {"Intro_Convo4_3", "<b>{0}</b> : Course I'm talking to you."},
        {"Intro_Convo4_4", "<b>{0}</b> : Is it that obvious?"},
        {"Intro_Convo4_5", "<b>{0}</b> : Well I'm definitely not a mind reader, otherwise I'd be locked up in this place, so, yeah, I'd say it's pretty damn obvious"},
        {"Intro_Convo4_6", "<b>{0}</b> : Well, how am I supposed to get a girlfriend anyway when I have this job? I mean, I can't tell her about it, so what am I supposed to do?"},
        {"Intro_Convo4_7", "<b>{0}</b> : Just lie to her. Tell her you work at some coffee shop or something."},
        {"Intro_Convo4_8", "<b>{0}</b> : Well, what if I accidentally forgot to wash my hands here at work, and I came home and there was blood on my hands? What would I say to her then?"},
        {"Intro_Convo4_9", "<b>{0}</b> : Uh, I don't know, tell her it's, uh... ketchup."},
        {"Intro_Convo4_10", "<b>{0}</b> : Ketchup? Why would I have ketchup on my hands if I worked at a coffee shop?"},
        {"Intro_Convo4_11", "<b>{0}</b> : Ugh, j-just forget it."},

        {"Intro_Convo5_1", "<b>{0}</b> : Uh, so you see any good movies lately?"},
        {"Intro_Convo5_2", "<b>{0}</b> : Uh, I don't really watch movies."},
        {"Intro_Convo5_3", "<b>{0}</b> : Oh ok. Well what about video games? You know that reminds me, someone should make a video game based on this place."},
        {"Intro_Convo5_4", "<b>{0}</b> : Why would anyone do that?"},
        {"Intro_Convo5_5", "<b>{0}</b> : I don't know, just thought it was kind of a cool idea."},
        {"Intro_Convo5_6", "<b>{0}</b> : Well, I don't play video games either."},



        {"Escort1", "<b>{0}</b> : Just follow me. Oh and by the way, we're authorized to kill any disobedient test subjects, so don't try anything stupid."},
        {"Escort2", "<b>{0}</b> : Just follow me. Oh and by the way, we're authorized to kill any disobedient test subjects, so don't try anything stupid."},
        {"EscortDone1", "<b>{0}</b> : Well, we're here. Just get in there and follow all the instructions and uh, you'll probably be fine."},
        {"EscortDone2", "<b>{0}</b> : Well, we're here. I'm still disappointed I didn't get to punch you, but whatever."},
        {"EscortDone3", "<b>{0}</b> : Well, we're here. I'm still disappointed I didn't get to punch you in the face, but *sigh* whatever."},
        {"EscortDone4", "<b>{0}</b> : Just get in there and follow all the instructions and uh, you'll probably be fine. Or maybe you won't be. Either way, I don't really care."},
        {"EscortDone5", "<b>{0}</b> : Well anyway, let's not waste anymore time. They're waiting for you down in the chamber."},


        {"EnterChamber","<b>{0}</b> : Attention all Class D Personel, Please enter the Containment Chamber" },
        {"Approach173","<b>{0}</b> : Please approach <color=yellow>SCP-173</color> for testing" },
        { "Problem","<b>{0}</b> : Uh, there seems to be a problem with the door control system,  the doors aren't responding to any of our attempts to close it, so Uhm, Please maintain direct eye contact with SCP-173 and-" },
        { "Refuse1", "<b>{0}</b> : Subject D-9341, enter the containment chamber or you will be terminated."  },
        { "Refuse2", "<b>{0}</b> : This is your last warning. You have five seconds to comply." },
        { "Refuse3", "<b>{0}</b> : Subject D-9341 designated for termination. Fire at will." },



        { "Escape1","<b>{0}</b> : I think the shortest way out, is through the South-east wing, follow me!" },
        { "Escape2","<b>{0}</b> : Did you hear that? I hope it wasnt-" },






        {"scene_BreachStart_1", "<b>{0}</b> : Agent, Behind You!"},
        {"scene_BreachStart_2", "<b>{0}</b> : Oh, shit!"},
        {"scene_BreachStart_3", "<b>{0}</b> : Keep your eyes on Him!"},
        {"scene_BreachStart_4", "<b>{0}</b> : Okay, I'm going to blink, just keep on watching him"},
        {"scene_BreachStart_5", "<b>{0}</b> : Alright, got it"},


        {"scene_012_1", "<i>I have to... I have to finish it...</i>"},
        {"scene_012_2", "<i>Do you really wanna do it... I don't... think... I can do this.</i>"},
        {"scene_012_3", "<i>I... I... must... do it.</i>"},
        {"scene_012_4", "<i>I-I... have... no... ch-choice!</i>"},
        {"scene_012_5", "<i>Balloons... This....this makes...no sense!</i>"},
        {"scene_012_6", "<i>No... this... this is... impossible!</i>"},
        {"scene_012_7", "<i>It can't... It can't be completed!</i>"},

        { "kneel106", "KNEEL"},
    };

    public static Dictionary<string, string> charaStrings_EN = new Dictionary<string, string>()
    {
        {"chara_franklin", "S.C. Franklin"},
        {"chara_ulgrin", "Agent Ulgrin"},
        {"chara_guard", "Guard"},
        {"chara_sci", "Doctor"},
    };

    public static Dictionary<string, string> deathStrings_EN = new Dictionary<string, string>()
    {
        {"death_173", "Subject D-9341: \nFatal cervical fracture. Assumed to be attacked by SCP-173."},
        {"death_173_doors", "\"<i>If I'm not mistaken, one of the main purposes of these rooms was to stop SCP-173 from moving further in the event of a containment breach. So, whose brilliant idea was it to put <b>A GODDAMN MAN-SIZED VENTILATION DUCT</b> in there?</i>\""},
        {"death_173_surv", "Subject: D-9341. \nCause of Death: Fatal cervical fracture. The surveillance tapes confirm that the subject was killed by SCP-173." },
        {"death_173_intro" , "Subject: D-9341. \nCause of death: Fatal cervical fracture. According to Security Chief Franklin who was present at SCP-173's containment chamber during the breach, the subject was killed by SCP-173 as soon as the disruptions in the electrical network started."},

        {"death_106_stone", "\" [...]<i>In addition to the decomposed appearance typical of the victims of SCP-106, the body exhibits injuries that have not been observed before: massive skull fracture, three broken ribs, fractured shoulder and heavy lacerations.</i>\"" },
        {"death_939" ,"\"[...] <i>All four escaped SCP-939 (4) specimens have been captured and recontained successfully. Three (3) of them made quite a mess at Storage Area 6. A cleaning team has been dispatched.</i> \""},
        {"death_012", "<i>Subject D-9341 found in a pool of blood next to SCP-012. Subject seems to have ripped open his wrists and written three extra lines to the composition before dying of blood loss.</i>" },


        {"death_intro",  "\"[...] <i>What an annoying piece of shit </i>\""},

        {"death_gas", "<i>Subject D-9341 found dead in [DATA REDACTED]. Cause of death: suffocation due to decontamination gas.</i>" },
        {"death_tesla", "Subject D-9341 killed by the Tesla Gate at [REDACTED]" },


        {"death_mtf", "Subject: D-9341. \nTerminated by Nine-Tailed Fox." },

    };
}