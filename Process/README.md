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
- potential upgrades: abstract AttackManager down to fit w different types of attacks (inheritance) + attack selection menu, add enemy retaliation + type variation beyond just color and hp

Project found under Projects/_experiments, asset folder P3-4

### CREDITS
- Bonk sfx downloaded from [here](https://www.myinstants.com/en/instant/doge-bonk-84044/)
- other sfx generated with [Jsfx](https://sfxr.me/)

![Hammer Prototype](./Media/P3-Hammer.gif)

## Week 5: Winter Storm Alert (02-13-2025 to 02-19-2025)
no class oopsie

### TODO
prof suggestion: either build on the previous experiment or be something entirely new

### Dev Notes
- busy week assignment-wise, so in case i dont have time to implement it heres a theoretical jump attack to build off of the previous experiment:
    - add collision box over enemy (thicker) & under player (thinner)
    - select attack type
    - select enemy (input check), auto move player towards enemy (no input check) (same as hammer)
    - when in position -> auto aim to arc over & land on enemy
    - correct input timing is while OnCollisionStay2D w the 2 hitboxes, otherwise miss (1/2 damage)
    - if successful input -> arc back in front of enemy, then move back to start position (no input check)
    - otherwise -> complete arc down, then move back to start position (no input check)
    - if enemy has no hp -> kill (same as hammer)
    - when in position, if no enemy -> spawn new enemy (same as hammer)
    - restart (same as hammer)
- if not completed, insert prototype schematics here
- only thing missing code-wise rn would be the arc transition (the hecks a bezier curve), hopefully ill figure it out in time before class
- bezier quadratic curve is: p(t) = (1 − t)^2 * init + 2t(1 − t)anchor + t^2 * end
- looks clunky as hell but it works, hooray **c:** could probably just do one big bezier curve instead of the uppies and downies, might look better (NOTE: it does)
- added some delays here and there we love to see it, better feel

Project found under Projects/_experiments, asset folder P3-4

![Jump Prototype](./Media/P4-Jump.gif)

## Week 6-7: Going Into Reading Week (02-20-2025 to 03-05-2025)
oh thank god no end of semester presentation.

brainstorming: state the challenge/constraints -> criticism is ok + quantity over quality. once an idea is found, find smaller problem statements to better define it

### In-Class Brainstorming Session
Braindump: pottery idk
- making things on the wheel is called throwing
- wedge = getting the air out of the clay + making it firmer
- eat ur dirt (dont)
- fine clay particles are real bad for ur lungs
- throwing process: wet the bat, slam on the bat, stabilize, center (cone), open (make sure there is enough still for the foot), pinch the cylinder, optional but recommended to gather up excess clay from the base into the cylinder (tricky, watch out), actually make the shape from the cylinder, free up excess clay from the foot to help it dry, remove bat from wheel & let dry a bit before covering and letting the piece slowly dry
- bat (the animal)
- trimming process: center, keep in place w extra clay or batmate or whatever, finish the shape + the foot, decorate as desired, let dry
- kiln
- glaze

Hybrids:
- wheel bats -> thats just the batmobile
- dirt bats -> they sleep in caves idk
- "sandstorm" clay bats w a poison/petrification effect (bc it gets in ur lungs)

Speed Dating:
- glaze bandits : thieves running a bakery, trying to stay down-low from the law AND ur customers, heists to steal from other bakeries to make ur own better
- farming bats : either ur a fruit bat w a fruit farm, or ur farming herds of bats (via trees, u pick them like fruit)
- flying center: bird school for birds to teach them how to fly, floating islands, flying to the center of everything
- clay sorceress: tower defense game w little clay dudes/pots as ur units
- bats vs mice: bats have the top of the screen mice have the bottom, or a story about a vampire that turns into a mouse instead of a bat & gets flack for it
- sleep clay: a pottery student falls asleep while making a piece, nap in the kiln, stealth game in a mansion full of pottery (dont break them or itll wake grandma)
- light bat: escape from a mine while avoiding shining light on the bats or they get angry and attack
- thief colony: cult of the lamb style training little robbers, victorian-era aesthetic

as usual, ideas ended up being more thematic than system-based

### TODO
- extra credit journal entry: game mechanic analysis (due March 6)
- find an idea to prototype (not necessarily in unity yet, can be just on paper for now) that can be iterated upon in the coming weeks

### Dev Notes
probably the best thing to do here is build off the 2 previous experiment, gradually adding more systems to make it a proper demo. current possible iteration ideas include:
- scene transitions between overworld & battle, remembering position in overworld
- switching to Unity 3D for 2.5D perspective outside of combat
- refactor attack manager script to be more generalized and make it easier to implement other types of attacks
- enemy retaliation + modifiers (spikes mean cant jump on it, etc), defend action command
- finding new combat abilities in the overworld that are then added to ur possible actions in-battle
- if enough time, replace basic shapes with custom (still) sprites/icons
- if enough time, save data

current possible conceptual ideas to make it more distinct than just a Paper Mario clone:
- game is based on exploration, turn-based battles are how u navigate treacherous environment (high wall to climb, big chasm to jump over, etc). when battle completed successfully, u spawn past it in the overworld (ex initiate battle from one end of a pit, spawn at the other side after the battle makes u cross it safely) w a possibility when completed to a) retry the battle for more exp or b) skip it if backtracking (can just be fade to black to begin with). enemy retaliations are environmental hazards (falling spikes, rocks getting dislodged & having to keep ur grip, earthquake, etc). trail mix to restore hps

![Iteration Prototype Concept](./Media/IT1-ConceptArt.png)

we love a funny little guy **:3**

alas we got midterms to study for so this is most likely it for this week rip, still a decent plan imho

## Week 8: Iterations Intro (03-06-2025 to 03-12-2025)
important: have someone else playtest/look at prototype (in class), for feedback

every week/loop keep track of Risks (problems to solve/what question to answer), Prototypes (plan out how to answer the risks), and Results (of prototypes)

### TODO
work on Iteration #2

### Dev Notes
Iteration #2 - Perspective Shift Transition

Risk(s):
- How will the transition to/from battle work? Same scene or different scene?
    - If same scene, how do we treat running away from battle? especially since the base concept involves advancing in the hazards
    - If different scenes, do we keep a static orientation (player on the left, enemy on the right) or do we keep in mind how the battle was approached?
- Considering the battles are environmental in nature, how do we indicate what triggers a battle so it doesnt feel like surprise encounters?

Unity Project Prototype(s):
- recreate battle experiment from Experiments 3-4 in 3D project (refactoring is not the goal but not unwelcome)
- add basic overworld scene w player movement and battle triggers
- test every kind of battle transitions (same scene, diff scenes w static angle, diff scenes w dynamic angle), get feedback from some poor shmuck (brötherrrr)

Other Prototype(s):
- think of + potentially test in Unity hazard indicator concepts
- think of + potentially test in Unity same-scene-battle escape

Results:
- same-scene-battle escape ideas:
    - screen fade to black, respawn at set location (basic, best for now)
    - "animated sequence" w player jumping in hole, hole close, hole opens at set location w player jumping out, hole close (more complex but that feel tho) (gotta draw it)
- hazard/battle indicator ideas:
    - having a bunch of those hiking warning signs (rock slides, exclamation point in triangle, hazardous cliff, etc) near where the battle takes place
    - addon: some visual effect on the edges of the screen that get more pronounced as u get closer
    ![warning signs](./Media/IT2-WarningSigns.png)
- overworld scene raises concern of player movement being jittery when colliding w walls & even clipping. jitteriness fixed by replacing Update w FixedUpdate, but clipping still happens at the junction of walls
- changed back to Update, fixed clipping by using AddForce instead of changing the position. still some fixes to do on that front but Good Enough for now
- battle transition results:
    - different scene: the tried-and-true method, works well enough. but its the tried-and-true method, so it can be p basic. doesnt make much sense to change the battle orientation in this situation tho, so skipped it
    ![separate scenes test](./Media/IT2-Separate.gif)
    - same scene: works best w the concept, but can be p limiting considering the enemies are environmental elements of the overworld, ie all encounters are predefined. but its Different, and a good direction to stand out from the genre/try funky things
    ![combined scene test](./Media/IT2-Combined.gif)
        - could make it a rogue. randomize & assemble different overworld chunks together every run. tricky challenge (ex. if chunk A leads to chunk B via a wall-climb section then B needs to have a higher ground level, or making sure battle ability pickups are still reachable in the configuration if it hasnt been picked up), but would bring in variance. sounds funky enough

Whats left for later iterations from initial plan (*if time permits):
- properly refactor attack manager (abstractize + redo attack transitions to better fit concept)
- enemy attacks + types (platforms over pit, climbable wall) + modifiers
- ~~map out + build overworld scene w final goal to reach, items (abilities) to pick up~~
- implement new player attacks based on concept
- \*draw character/item sprites
- \*title/end scene
- \*implement save/load system

New potential avenues:
- fix overworld controls
- **randomize overworld generation** w logic for placing item/ability pickups

## Week 9: Iteratin' (03-13-2025 to 03-19-2025)
class feedback:
- include a menu/tutorial to see what the controls are (not everyone played paper mario & knows how to land the action commands)

### TODO
work on Iteration #3

### Dev Notes
Risk(s):
- how would one go about randomizing a 3D stylized world? a quick internet search suggests:
    - heat map, generally for more realistic styles and Unity terrain. [(src)](https://www.reddit.com/r/Unity3D/comments/2kdfno/how_to_make_3d_random_map_generation_if_you/)
    - 3D noise generation for a x,y,z grid, each block's value determinates what kind of block is at that coordinate, generally minecraft-y. [(src)](https://www.reddit.com/r/Unity3D/comments/2kdfno/how_to_make_3d_random_map_generation_if_you/)
    - Random/Drunken Walk algorithm to generate a path in a 2D grid, usually for mazes. [(src)](https://medium.com/@mihailstumkins/how-to-create-random-levels-with-unity-3d-2219c4d39ea8)

Unity Project Prototype(s):
- create a few basic chunks of varying heights to test WorldGen
- create customizable Chunk class script for storing height, orientation, etc. data
- start working on script to generate (small) 2D grid with Random/Drunken Walk algorithm, assign a world chunk to each coordinate w logic reguarding the height (ex. if a chunk has a higher ground level, make sure an adjacent coordinate has at least 1 scalable wall battle, make sure most of the base ground lvl chunks are connected, etc)

Other Prototype(s):
- start looking at free environmental assets for chunk building

Results:
- Notable asset packs:
    - [aw yeah low-poly mountains](https://assetstore.unity.com/packages/3d/environments/3d-low-poly-environment-assets-299354)
    ![low-poly mountain assets](./Media/IT3-lowPolyMtn.webp)
    - [nice low-poly trees](https://assetstore.unity.com/packages/3d/environments/landscapes/low-poly-simple-nature-pack-162153)
    ![low-poly mountain assets](./Media/IT3-lowPolyNature.webp)
    - [this one might be a bit too high-poly for my tastes but still](https://assetstore.unity.com/packages/3d/environments/lowpoly-environment-nature-free-medieval-fantasy-series-187052)
    - [good rocky ground texture](https://assetstore.unity.com/packages/2d/textures-materials/nature/ground-earth-and-rocks-free-texture-a-hand-painted-235783)
    - [more nice textures](https://assetstore.unity.com/packages/2d/textures-materials/nature/handpainted-grass-ground-textures-187634)
- generating a random tile grid is eazy, writing the logic for assigning the chunks less so. will have to figure out how to work out the pits, making sure u cant just walk around them
- best to stick to a small grid for now
- found a good script base for grouping connected tiles of the same type, will help to assign chunks
- will have to adjust camera for if theres a higher chunk between it and the player, also gotta make sure theres no encounter directly behind a higher chunk
- good progress was made on the WorldGen script, but highly likely i wont get to finish it this iteration bc its complex. might have overshot a weeeeeeee bit w this, oopsie **c:-0** but a good learning experience nonetheless, in this house we respect the KISS principle. should make at least one default world scene so i can focus on the rest of the important bits to have a decent final prototype by the end of the semester & keep hacking at it if i get the time
- all thats left is the chunk-assigning logic and making chunks. def got newfound respect for the good people of dk64randomizer dot com, coding good rando logic is rough

Whats left for later iterations (**priority**, \*if time permits, ~~def not enough time rip~~):
- **fix overworld controls + camera when high chunk between cam-player**
- add menu w control scheme + attack timing description
- refactor attack manager (~~abstractize +~~ **redo attack transitions to better fit concept**)
- **convert enemies to environment (platforms over pit, climbable wall)**, add enemy attacks + modifiers (ex. spiked platform)
- \*finish WorldGen script
- ~~implement new player attacks based on concept~~
- ~~draw character/item sprites~~
- ~~title/end scene~~
- ~~implement save/load system~~

## Week 10: Iterating Still (03-20-2025 to 03-26-2025)
What - So What - Now What

### TODO
work on Iteration #4

### Dev Notes
Risk(s):
- for the final iteration result, do i want a functional rando WorldGen or do i want a playable example of the game loop?
    - WorldGen: more about wanting a technical result, full Implementation
    - playable: more of a mix of Look/Feel and Implementation

Unity Project Prorotype(s):
- create a few basic chunks of varying heights; needed regardless of which final result im going for
- if playable, create 1 pre-built overworld scene + look into fixing the camera
- if WorldGen, keep hacking at the script and test it

Results:
- just a general observation, but i havent been thinking at all about Role implementation all throughout the semester
- going down the rando path wooo!! feels more challenging to get right and its all about prototyping anyways, trial & error.
    - **Expected final prototype: having a scene that randomizes a world every time its loaded. unplayable but can use the scene view to look around**
- chunks. thats it.

![soooo... chunks, huh](./Media/IT4-Chunks.gif)

## Week 11: Homestretch (03-27-2025 to 04-02-2025)
Pippin presentation

### TODO
work on Iteration #5

### Dev Notes
Risk(s):
- can i get that chunk-assigning done (to my liking)

Unity Project Prorotype(s):
- get that chunk-assigning done

Results:
- found a seemingly decent logic that shouldnt take too many loops
- need to think of a fix for the edgecase where the tile selected to have cliff encounter is behind the higher-up tile to prevent it from happening
- somethings up, mostly void. also a vertical hole cross was placed on the top edge of the map, which shouldnt be the case.

![mostly void, partially code fuckups](./Media/IT5-MostlyVoid.png)

- mostly void bc the walker loop only goes once bc GetNbTiles counts all tiles by default so its automatically higher than the fill%. except now instead of just 1 tile its just the top row, somehow the walker(s) are spending all their iterations there. dang y instead of z
- nts in loops when passing a tile to a function to modify it, it doesnt actually apply the change when doing ```tile.whatever = whatever```, which messes w the rest of things. do ```grid[tile.x, tile.y].whatever = whatever``` instead
- slightly better, havent gotten to fixing the hole chunk logic but how tf did a walker jump like that. answer is another sneaky y instead of z. except now it somehow fills the whole board. damn int/int not resulting in a float

![huh????](./Media/IT5-WalkerJumped.png)

- looking better, but one summit has no climbable wall to get up to it so gotta tweak that logic a bit too. some slight tweaks to the chunks' size and y-placement too. seems like the singular peaks/summits are the ones that dont get a climbable wall up to. havent gotten any hole spawned in these good ones
- nts missing some chunks where the climbable walls is north, gotta make them
- potential logic upgrade: make sure peak/summit groups have a climbable wall in adjacent ground group. also get them hole spawns fixed. and them null refs that sometimes happen

![nice](./Media/IT5-ClosestSoFar.png)
![neat](./Media/IT5-FeatFrontClimb.png)

## Week 12: The End (04-03-2025 to 04-10-2025)
- sales pitch: whats the product (econo, cultural, creative, intel) - to who (player, publisher, press, boss) - whats the narrative of ur pitch
- pyramid (topdown): game concept (5sec, constantly being refined, ex: TITLE: quick description - what u have to do - main qualifiers) -> victory condition -> mechanics -> selling points
- q/a: have predetermined questions to get feedback on instead of leaving it open, dont argue about the feedback

### TODO
finalise iteration project

### Dev Notes
Risk(s):
- bug/fix:
    - ~~too many peaks (probably an int-float thing again)~~
    - ~~neighborInGroup null thing~~
    - ~~fix hole spawn logic~~
    - ~~fix single height group climb spawn logic~~
    - ~~upgrade: make sure peak/summit groups have a climbable wall in each adjacent ground group~~
    - ~~fix stranded ground groups~~

Unity Project Prorotype(s):
- get that chunk-assigning done pt.2

Results:
- slight feedback from class: have somewhat more obvious distinction between the heights (color-code, etc) for better visualisation

![color chunks](./Media/ITF-DiffChunkColors.png)

- fixing the nb of higher tiles should be a mix of int-float thing + currently dividing by the total size of the grid and not the nb of used tiles, THEORETICALLY. somehow theres still some seeds that are mostly peak rather than ground (most likely could be bc of the 2nd pass to check if surroundings are all higher/void)
- fixed the unreachable single higher chunks by adding lists of single tiles + making sure to assign the summits before the peaks. raised another issue w the current height-assigning where a peak can be connected to no summit, and a group of lower ground can be fully stranded behind higher tiles (bc no climbable wall from the south of a chunk)

![stranded](./Media/ITF-StrandedChunks.png)

- possible fix for stranded heights: do another walker sequence instead of the current rng method. for peaks, the % of tiles should be the current peakCap + summitCap. for summits, only walk on peaks and % of tiles should be just summitCap
- damn these are like the almost-platonic ideal of worldgen its just missing some holes

![sick](./Media/ITF-AlmostPlatonicIdeal1.png)
![sick](./Media/ITF-AlmostPlatonicIdeal2.png)

- the null thing was just me not checking the right list, dang copy-paste
- now this. this is the real platonic ideal. im not even done tweaking the logic. incredible.

![The One](./Media/ITF-TheOne.png)
![another](./Media/ITF-AnotherWinner1.png)
![another](./Media/ITF-AnotherWinner2.png)

- damn look at these funky one, starting to think i dont necessarily need to tweak it any further

![wont you take me to](./Media/ITF-FunkyTown.png)
![oooooooh](./Media/ITF-ThisOneTho.png)

- still getting some seeds that get stuck loading for many minutes, so gotta fix that still. but outside reviews are in and people think the idea for the prototype and the results are real neat
- still a few edge-cases to fix also, as evidence by these worldgens where a pit prevents access to a crucial tile 

![i speak it into existence by curse of code decay](./Media/ITF-ProofOfFixStill.png)
![bad pit](./Media/ITF-BadPit1.png)
![bad pit](./Media/ITF-BadPit2.png)

- still possible for ground height to be only adjacent to summits, add more complexity to double checking heights
- put in a check for if a loop takes too long to stop play mode and getting stuck while starting play mode still happens. gonna try running the code in a coroutine see if it happens again. NOTE: yes. try killing the coroutine instead. NOTE: found a non-coroutine workaround
- first test in making sure each peak/summit groups have a climbable wall in each adjacent lower group, somehow missing one climb but overall seems to work. seems to be consistently ground chunks that dont get assigned. also, this fix triggers the auto-loop-stop more often but hey it works

![many climbs](./Media/ITF-ClimbFix1.png)
![many climbs](./Media/ITF-ClimbFix2.png)

- somehow the lack of climb doesnt happen if i dont use the function i made to limit code duplication. go figure

![idk](./Media/ITF-ClimbFix3.png)
![idk](./Media/ITF-ClimbFix4.png)

- still a few kinks to works out, evidently. also havent implemented the pit recursion, but given that its unnecessary most of the time ill just say: **Good Enough(tm)**. still very happy w what i made tho, lots to consider when doing a rando!