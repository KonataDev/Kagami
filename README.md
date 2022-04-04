## Kagami 

[![C#](https://img.shields.io/badge/C%23-9.0-green)](#)
[![License](https://img.shields.io/static/v1?label=LICENSE&message=GNU%20GPLv3&color=lightrey)](./blob/main/LICENSE)  
<img width="20" src="https://github.com/KonataDev/Konata.Core/raw/main/Resources/konata_icon_512_round64.png">
<img width="20" src="https://user-images.githubusercontent.com/17957399/157422004-2a367049-3243-4206-90f4-ecb3f033c5ab.png">
<img width="20" src="https://user-images.githubusercontent.com/17957399/155513020-dd912c37-a86f-4d67-b707-566418cbc152.png">
<img width="20" src="https://user-images.githubusercontent.com/17957399/157422071-0faf24e0-46c6-4617-8dc0-ba6eab193237.png">

A demo bot that can lead you to create your  
own bot with [Konata.Core](https://github.com/KonataDev/Konata.Core) quickly.

### Commands

| Command             | Description                                                  |
|---------------------|--------------------------------------------------------------|
| /help               | Print the help messages.                                     |
| /eval               | Print the raw messages that after '/eval'                    |
| /echo               | Print the raw messages that after '/echo' (safer than /eval) |
| /status             | Print the status such as build, version, memory usage..etc   |
| /mute \<at\> [time] | Mute the member.                                             |
| /member \<at\>      | Inspect member information.                                  |
| /poke [at]          | Send poke to the member or send poke to the sender.          | 

### Triggers

| Trigger              | Example                             | Description                        |
|----------------------|-------------------------------------|------------------------------------|
| BV                   | BV1Qh411i7ic                        | Parse the BiliBili video code(BV). |
| Github               | https://github.com/KonataDev/Kagami | Parse the GitHub repo image.       |
| Friend Add Request   | /                                   | Auto Approve                       |
| Group Invite Request | /                                   | Auto Approve                       |
| /                    | /                                   | Repeater function.                 | 

### Known issues

- [ ] Sometimes offline.
- [x] Sometimes stuck on some internal tasks.
- [x] Stuck while the bot exit.
- [x] Bot can trigger the command which sends by itself. (LMFAO)

### License

Licensed in GNU GPLv3 with ❤.
