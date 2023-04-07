## Kagami 

[![C#](https://img.shields.io/badge/C%23-latest-green)](#)
[![License](https://img.shields.io/static/v1?label=LICENSE&message=GNU%20GPLv3&color=lightrey)](./blob/main/LICENSE)  
<img width="20" src="https://github.com/KonataDev/Konata.Core/raw/main/Resources/konata_icon_512_round64.png">
<img width="20" src="https://user-images.githubusercontent.com/17957399/157422004-2a367049-3243-4206-90f4-ecb3f033c5ab.png">
<img width="20" src="https://user-images.githubusercontent.com/17957399/155513020-dd912c37-a86f-4d67-b707-566418cbc152.png">
<img width="20" src="https://user-images.githubusercontent.com/17957399/157422071-0faf24e0-46c6-4617-8dc0-ba6eab193237.png">

A demo bot that can lead you to create your  
own bot with [Konata.Core](https://github.com/KonataDev/Konata.Core) quickly.

### Commands

| Command                                 | Description                                                |
|-----------------------------------------|------------------------------------------------------------|
| /status                                 | Print the status such as build, version, memory usage..etc |
| /dbg \<code\>                           | Enable the debug output of your REPL expression            |
| any user-defined command start with '/' | /                                                          |

### Triggers

| Trigger | Example | Description |
| ------- | ------- | ----------- |
| Github  | https://github.com/KonataDev/Kagami | Parse the GitHub repo image. |

### REPL Feature
Kagami offers a modern C# (Currently C# 11) interactive terminal,  
type any valid C# expressions in the group then run the code.

#### Example
Benefited from the REPL feature, Kagami supports user-defined commands.  
You can define your command like the example below: 
```C#
// define a command named 'example'
// it has 3 argument inputs a, b and c.
var example = string (int a, bool b, string c)
    => $"a is {a}, b is {b}, c is \"{c}\"";
```
After defined the command, you can send '/example 1 false Hello' in your group,    
then Kagami will prints a "a is 1, b is False, c is Hello" string. 

#### Script Presets
The script presets are stored in 'scripts' directory,  
write down your code and save with '.cs' suffix name here and restart Kagami to take effect. 

The normal output while Kagami starting:
```
[ *** ] REPL compiling script => scripts/ic.cs
[ *** ] REPL compiling script => scripts/help.cs
[ *** ] REPL compiling script => scripts/echo.cs
[ *** ] REPL compiling script => scripts/ping.cs
[ *** ] REPL compiling script => scripts/mute.cs
[ *** ] REPL scripts load finished.
```

See more examples in [ScriptExample](./ScriptExample).

#### Sandbox Environment

Sandbox environment are defined in [ReplEnvironment.cs](./Kagami/SandBox/ReplEnvironment.cs),  
Contains fields, classes and methods as the script runtime context.

| Name          | Description                                  |
|---------------|----------------------------------------------|
| Bot           | The Kagami bot instance, full access in REPL |
| CurrentGroup  | Current group uin                            |
| CurrentMember | Current member uin                           |
| JSON          | The dynamic JSON parser, use JSON.Parse()    |
| Print         | Print a string                               |
| CanIDo        | Bot do something with a probability          |
| Wget          | Http request                                 |
| Random        | Shared random                                |

### Known Issues

- [x] Sometimes offline.
- [x] Sometimes stuck on some internal tasks.
- [x] Stuck while the bot exit.
- [x] Bot can trigger the command which sends by itself. (LMFAO)

### License

Licensed in GNU GPLv3 with ❤.
