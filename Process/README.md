# Weekly Class Journal

## Week 1: Tiny Game (01-16-25 to 01-23-25)
### Tool Overview
- **Twine)** text-based, diverging choices; official documentation is structured w a sidebar, but kinda heavy (no examples)
- **Ink)** also text-based, diverging choices; official documentation is structured, has "code" examples (still a bit too clunky for me tho); apparently has a Unity plugin
- **Bitsy)** visual, tile-based (16x16), small sprites (8x8) and limited colors palettes (3 colors, but can change between rooms); various community tutorials; I prefer to work with visuals so going with this one for the tiny game

### Dev Notes
Saved my progress often with Bitsy, but had kind of a panic moment when I wanted to reload a previous data file via the "Load Data" button in the "Game" tab & the whole thing just reset **D:** The save files I had were fine and I just had to copy-paste them in the "Game/Data" subsection, just the "Load Data" option being buggy it seems (along with the game window after playtesting it). Refreshing the page every now & then fixes the visual bugs at least.

Also, there's no quick & easy way to reorder the rooms that I found (specifically to change which one the game starts in), gotta manually update it in the "Game/Data" subsection along with all sprite references to the rooms' indexes. Had fun figuring out how to get the change in perspective from top-down studio view to the side throwing view right tho.

Messed around with the generated HTML/css/Javascript to add a pretty start button bc why not **c:**

### Playtest Notes
- People liked how elaborate the experience was even if the game loop was extremely basic, the shift in perspective from studio to throwing, and that there were different ways to throw the clay on the wheel (lot of spritework went into this)
- Pretty much everyone always used the same wheel after (re)wedging the clay, but were pleasantly surprised when I pointed out that the game remembered which wheel was used and putting the avatar back next to the right seat
- Despite the door (intentionally) being the same color as the walls (as opposed to the interactable green sprites), someone still immediately went for it first thing into the game and got to the end screen lol

[Play the game here!](https://zettamarge.github.io/cart-315/Projects/1_TinyGame/POTTERY_GOBLIN.html){:target="_blank"}
