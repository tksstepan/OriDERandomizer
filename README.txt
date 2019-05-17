This repository contains the client code (distributed as a modified Assembly-CSharp.dll) for the OriDE randomizer. The website, seed generator, bingo and other web tools are hosted at https://github.com/turntekGodhead/ori_rando_server

The Ori Randomizer is built on top of sigmasin's incredible work.

Setup (more detailed instructions available at https://orirando.com/faq):

Put Assembly-CSharp.dll in Steam/steamapps/common/Ori DE/oriDE_Data/Managed
Use https://orirando.com to generate seeds.
Generated seeds should be placed in Steam/steamapps/common/Ori DE and named "randomizer.dat"

Information on how to set up the randomizer can be found at https://orirando.com/faq
COMMANDS (can be customized using RandomizerRebinding.txt):

Alt+T               Replay last pickup message
Alt+R:              Teleport Ori to the start of the game
                    Warning: using this in locked rooms can potentially softlock
Alt+L:              Reload the seed file (use to change seeds without restarting the game)
Alt+P:              Display current skill tree and shard progress
Alt+C:              Toggle color shifting
Alt+K:              Toggle Chaos Mode
Alt+V:              Within Chaos Mode, toggle messages
Alt+F:              Within Chaos Mode, force an effect to spawn
Grenade:            Double bash

The double bash bind exists to create parity between playing randomizer on controller and keyboard+mouse. If any of the binds specified are held when a bash ends, a double bash will automatically occur. To also make any of the binds specified end a bash on their own, add "Tap" as a bind for the double bash function.

RandomizerSettings.txt can be used to set the bash deadzone on controller. Valid inputs are any number between 0 (no deadzone) and 1 (full deadzone).

If you get stuck, use Alt+R to send yourself back to glades. Wall jump, double jump, and post ginso escape are all possible sticking points (you will be able to get into swamp without doing the ginso escape, at some point). Don't use Alt+R while you're in a locked room (sein fronkey fight, ginso miniboss, etc.) or you will probably be softlocked.

Shards, limitkeys, and clues all simplify the hunt for dungeon keys in various ways. Limitkeys forces all dungeon keys to appear at skill trees or world events (getting any of the dungeon keys, finishing the ginso escape, or starting the forlorn escape). Shards places 5 fragments of each key around the world -- upon finding 3, you are able to enter the corresponding dungeon. Clues mode provides general locations for a new key for each 3 trees visited, viewable with Alt+P. For new players, Clues is recommended.

An (outdated) guide on randomizer terminology and routing is available here: https://docs.google.com/document/d/1g7mSMY5IzORo7mrkmfb0cxLmiw2FD8rfkplIJUDLy-4/edit#heading=h.kiiklir95dur

Difficulty Guide

Not sure what to play? Here is a description of some common presets:

Casual: For people who have not speedrun the game. The only glitch that may be required is breaking charge flame walls with stomp. Damage boosting may be required, but the maximum damage taken will be 2.

Standard: For people who have run All Skills. It is assumed that all tricks used in the All Skills run are known, with the exception of sorrow bash and swamp entry. Damage boosting may be required, but the maximum damage taken will be 2. Double bashing is not required. Deep swimming without clean water is not required.

Expert: For people who have played a fair amount of the randomizer. Tricks involving out-of-the-box uses of abilities may be required. Damage boosting may be required, but every spike patch can be crossed with only a single instance of damage. Double bashing is not required.

Master: For people who want to struggle. Everything except going OOB or using TA is fair game. Paths requiring triple jump may be required. Extended damage boosting with ultra defense may be required.



CHAOS MODE?

I recommend just trying it out once before reading this section. Alt+K toggles it on and off -- when toggling off, all effects will be removed immediately.

Chaos mode spawns random effects at intervals of 5-15 seconds. The duration varies by effect, but is in the range of 5-60 seconds (thus, multiple effects can occur simultaneously). Possible effects include:

Fast movement speed
Slow movement speed
Ice physics on the ground
Max speed increase
Randomized acceleration/max speed values between ground/air
Gravity increase/decrease
Gravity direction change
Gravity well (within 20 units of Ori)
Short range teleportation
Long range teleportation
Camera zoom in/out
Temporary poison
Invisibility
Damage vulnerability/resistance
Random velocity vectors

The randomness of chaos mode is based on the system clock instead of a set seed, thus will vary between players playing the same seed and between plays of a seed. This mode is intended for fun and to feel the joy of getting teleported out of bounds while you're stuck in a cutscene. I recommend saving a lot.
