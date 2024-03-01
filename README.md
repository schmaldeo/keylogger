# Keylogger

This is a keylogger making big use of [Microsoft's CsWin32 NuGet package](https://github.com/microsoft/CsWin32).

## DISCLAIMER
This program was made with educational purposes in mind.
**I am not responsible for and do not condone any improper use of this software.**

## Technicalities
The program installs a low level keyboard hook and then processes the messages. They then get mapped to human-readable
strings and writes them to a file every time max buffer size is reached. The program detects when caps lock and shift
are active and alters the characters written to file accordingly, however modifier keys do not affect the chars written
and instead their `keyup` and `keydown` events are written so it's clear when they were active. You can't just map
chars + modifier keys to another char because that's dependent on the keyboard layout. That's too much effort to make
mappings for every layout, even if you only include the most popular ones. That information about the layout is however 
written to the log file, so you can actually find out what the input was.
