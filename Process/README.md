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

[Play the game here!](https://zettamarge.github.io/cart-315/Projects/1_TinyGame/POTTERY_GOBLIN.html)

## Week 2: Intro to Unity (01-23-25 to 01-29-25)
ahhhh the basics. rly takes me back to my cegep days. still getting enumerator/able mixed up oops

### TODO
Experiment w/ movement, colliders, physics materials, etc. Could just do a mini platformer thing

### Dev Notes
- Going w/ a Jetpac (1983) recreation instead. Having some issue making the raycast + lineRenderer work (nts: check values during play mode to see how to fix + keep script in PlayerControl)
- Got it to work w/ a certain number of lasers at all times (hooray!!). However, raycast collision detection is too precise & doesnt work well. Research indicates to use CircleCast as replacement. Also, it looks like theres a glitchy one near the starting position when I fire repeatedly, idk if it's functional or how to remove it, but no biggie.
- CircleCast addendum: the raycast would collide w/ the player, changed it to CircleCastAll and iterated to find a hit w/ an enemy.
- Player death & respawn is functional, added a score and an extra platform. Satisfactory for this experiment, all thats missing for a full Jetpac recreation is:
    - lives + game over
    - building + fueling the rocket, lvl transitions
    - bonus point items upon collection
    - sounds and actual sprites, better laser animation
    - awarding the rareware coin upon getting 5000 points **:p**

Project found under Projects/_experiments, asset folder P1 (nts: do folder hierarchy per experiment)

![Jetpac Prototype](./Media/P1-Jetpac.gif)

## Week 3: Pong (01-30-25 to 02-05-25)
Proto-Types:
- Look/Feel (no focus on function necessarily)
- Role (story, flowchart)
- Implementation (actual function, no focus on look necessarily)
- Integration (all of the above)

tennis. tennis. tennis. tennis. tennis.

### TODO
pong variation. or not idk.

### Dev Notes
- pawng -> prawn -> shrimp play with bouncy ball. mantis shrimp can punch ball real good (more velocity). segmented shrimp (gameObject w/ multiple circles, see trail renderer or Vector2.MoveTowards, individual colliders might work best) w/ tank controls? (LR: rotate, UD: move) for a more swim-feel
- bit annoying that dictionaries cant be edited from the inspector, but easy workaround using a serializable custom class
- smooth snake movement is hard, delay rotation? yes, also add more delay per subsequent body part for maximum effect. might keep completing the rotation even if doesnt move bc otherwise its veeeeeeery tricky to manage.
- except i actually got something that looks half decent???? just gotta keep the parts from going too far from each other now tho, but shrimp move good rn
- just had to lower the maxDistanceDelta, easy lol
- shrimp wrap doesnt work the same way as Jetpac's bc of the segments but isnt much trickier
- all that left is to increase bounciness & lower the velocity back down to a threshold over time, make gravity 0 to make it floaty w some random initial force
- could do like pawng template and have 2 shrimps trying to get the ball to the opposite side (color-code shrimps) (also keep screenwrap bc its funny)
- not sure how to slow down ball while keeping its bounciness w/ the moving players, the rotation is still a bit wonky at times but mostly all good; something to figure out at a later time i guess. other potential upgrades: actually doing the mantis punch (goes w/ better physics), baskets for the ball, eating plankton to grow bigger/longer (ie dynamically add and manage rotation of more segments)

Project found under Projects/_experiments, asset folder P2

![Prawng Prototype](./Media/P2-Prawng.gif)

## Week 4: Breakout (02-06-2025 to 02-12-2025)
singletons my beloved. and beloathed. unless u love load-bearing coconuts (we love load-bearing coconuts)

### TODO
prof suggestion: prefab variation (diff colours, values, etc)

breakout pinball? piano breakout? dk64randomizer dot com piano game? paper mario battle test?

### Dev Notes
- going with paper mario battle test; goal is hammer attack, should go as follows:
    - select enemy (input check), auto move player towards enemy (no input check)
    - when in position -> spawn hammer, start attack timer (input check)
    - if timer = 0 && no input, small bonk (1/2 damage)
    - if input -> rotate hammer back, change to input timer (with leeway for correct timing)
    - if early release -> small bonk (1/2 damage)
    - if late release -> miss (0 damage)
    - when input release/timer end -> rotate hammer, deal damage, despawn hammer, move back to start position (no input check)
    - if enemy has no hp -> kill
    - when in position, if no enemy -> spawn new enemy
    - restart
- enemies have random HPs and colour
- possible miss "animations": yeet or hammer fall back
- small bonk slower rotation?
- works just as planned, EZ (just gotta tweak the speeds a bit but shhh, and fix the rotation back on subsequent hits)
- for some reason sfx pitch remains at 1, even when changed in editor (NOTE its bc i added an AudioSource component directly to the gameobjects and not creating one from the dropdown menu, thats so dumb smh) (NOTE #2 ITS NOT EVEN THAT ITS JUST THAT ITS RESET TOO FAST, GOD)

Project found under Projects/_experiments, asset folder P3

### CREDITS
- Bonk sfx downloaded from [here](https://www.myinstants.com/en/instant/doge-bonk-84044/)
- other sfx generated with [Jsfx](https://sfxr.me/)

![Hammer Prototype](./Media/P3-Hammer.gif)