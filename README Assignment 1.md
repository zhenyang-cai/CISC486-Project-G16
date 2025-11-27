# CISC486 Course Project - Group 16

> The following is sourced from the document located at `Assignments/Assignment 1 - Game Proposal.docx`

## Game Information
Working title: Echoes of Silence
### Gameplay
Our game will be a 2-player cooperative 3D stealth game centered around asymmetrical roles and coordination.

One player will take the role of an agent tasked with infiltrating enemy zones, completing objectives, and eliminating enemies, all while remaining undetected. The agent is relatively weak to enemy attacks, being killed within just a few hits, but they will have access to a small selection of weapons and items with which to eliminate enemies. The second player will operate a drone to provide detailed reconnaissance, create distractions, and manipulate mechanisms only accessible to the drone. While the drone is durable enough to handle multiple attacks from many enemies, they are not invincible, and none of their abilities will be able to eliminate enemies.

If the drone is killed during the mission, they will be able to respawn after a long cooldown, leaving the agent vulnerable. However, if the agent is killed during the mission, they will stay dead. If both players are dead at the same time, the mission is automatically failed. Progressing through the level and completing the main objectives will require careful teamwork, as the agent is vulnerable without the drone’s support, while the drone cannot complete certain objectives or take down enemies without the agent’s assistance.

Once the main objective is completed, players will be able extract via a specified extraction point, located on the outskirts of the level. During this phase, enemy They can also complete optional side objectives before extraction to receive additional rewards, in the form of new missions and additional abilities to unlock.

### Players
Our game is designed for two players to play cooperatively. It can be played locally on the same device, or on two separate devices, which can be either on the same local network or over the internet.

### AI/NPCs
Levels will feature multiple enemy NPCs controlled via finite state machines. Enemies will follow predefined patrol paths throughout the level, and if any unusual sounds or movements are made within a certain range of them, they will transition into an alert state and begin searching the nearby area. Full detection of either the drone or the agent will trigger a chase state, where enemies will actively pursue intruders and may call in reinforcements. If they lose track of the players, they will switch back into an alert state, before returning to their patrols after being unable to find the players. 

### Scripted Events
After the player completes the main objective, they will be required to extract from the level. During this extraction phase, the level will be in a heightened state of security, with additional enemies, expanded patrols, and certain pathways being sealed off. The agent and the drone must adapt to the changed environment to reach the extraction point safely. Additionally, some objectives will have scripted events that play once it is completed, altering the level and the behaviour of enemies.

### Environment & Assets
Our game will be set in a post-apocalyptic environment dominated by synthetic machines and androids, with an art style reminiscent of “Blame!” or “NieR: Automata.” Players will traverse ruined cities, abandoned factories, and secret laboratories, all patrolled by mechanical guardians. Core assets include enemy robot models of varying types (patrol guards, commanders), the agent character model, and the drone. Environmental details such as broken surveillance systems, locked terminals, and interactive panels will also be placed throughout to provide opportunities for strategic gameplay. During initial development, most assets will be placeholder development assets, to let us focus on development of the main gameplay. Once the game is near completion, we will begin to replace those assets.
Additionally, we plan to use third-party frameworks to optimize the Unity development experience, specifically QFramework. The event-driven and data-driven features of QFramework make it easier for us to handle global events and data interactions between objects.

## Group Information
Group #16
### Roles
- William Ban: 3D modelling, animations, programming
- Zhenyang Cai: Level design, UI, programming
- Yuda Hu: Gameplay design (abilities, enemy design, balancing), programming

These roles are tentative and will likely change over time as we develop the game.

### Rough Timeline
- Week 5: Assignment 2 - Player controllers and basic AI enemies
- Week 6: Initial set of player abilities and weapons
- Week 7: Prototype levels
- Week 8: AI enemy development
- Week 9: Assignment 3 - AI enemies with decision-making and pathfinding
- Week 10: Additional weapons and abilities, enemy types
- Week 11: Final levels, environmental art, and assets
- Week 12: Networking development
- Week 13: Assignment 4 - Networking & multiplayer
