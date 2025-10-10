## 🧠 Enemy FSM Description

### 🔹 Overview
The enemy AI uses a Finite State Machine (FSM) to manage its behavior based on a **Suspicion** value ranging from 0 to 1.  
- When the enemy detects the agent or the drone, the suspicion level increases toward 1.  
- When the target is lost, the suspicion level gradually decreases back toward 0.  
The FSM transitions between Patrol, Alert, and Chase states depending on this suspicion level.  
Pathfinding is handled via Unity’s **NavMeshAgent**, allowing dynamic movement between waypoints and towards the player’s position.

---

### 🔹 States

| **State** | **Description** |
|------------|----------------|
| **IdleState** | The enemy stands still within its patrol routine, scanning the surroundings. Suspicion remains low. If the patrol timer expires, the enemy begins to move to the next waypoint. |
| **MoveState** | The enemy walks between predefined patrol points using `NavMeshAgent`. If no anomalies are detected, it loops between waypoints. If suspicion rises above 0.5 (hearing or partial sight of player/drone), it transitions to the AlertState. |
| **AlertState** | The enemy becomes partially aware of the player or drone (Suspicion between 0.5 and 1). It pauses and **observes its surroundings**, rotating toward potential noise or visual sources. If the player is fully detected (Suspicion == 1), it transitions to ChaseState. If the suspicion decreases below 0.5, it returns to PatrolState. |
| **ChaseState** | The enemy has fully detected the player or drone (Suspicion == 1). It actively pursues the target using `NavMeshAgent`, updating its destination every frame to match the player’s position. If it loses sight of the target and suspicion decreases, it transitions back to AlertState or PatrolState. |
| **PatrolState (Parent)** | A higher-level composite state that encapsulates both IdleState and MoveState, allowing the enemy to alternate between waiting and patrolling. This state is re-entered when suspicion falls below the alert threshold. |

---

### 🔹 Transitions

| **From** | **To** | **Condition / Trigger** |
|-----------|---------|-------------------------|
| **IdleState** | **MoveState** | Patrol timer expires (`timer > idleDuration`) |
| **MoveState** | **IdleState** | Destination reached or waypoint delay triggered |
| **PatrolState (Idle/Move)** | **AlertState** | `0.5 <= Suspicion < 1` (partial detection of player or drone) |
| **AlertState** | **ChaseState** | `Suspicion == 1` (target fully detected) |
| **ChaseState** | **AlertState** | `0.5 <= Suspicion < 1` (target partially visible or lost briefly) |
| **AlertState / ChaseState** | **PatrolState** | `Suspicion < 0.5` (target lost and environment calm) |

---

### 🔹 Implementation Notes

- FSM implemented in **Unity C#** using a state pattern (`BaseState` + `EnemyController` with current state reference).  
- **Suspicion value** is updated continuously based on vision/hearing triggers:  
  - Increases when the target (agent or drone) enters the enemy’s field of view or emits noise.  
  - Decreases gradually over time when no target is detected.  
- **NavMeshAgent** handles patrol and chase pathfinding. Patrol waypoints are stored in an array and updated cyclically.  
- **Animator** parameters (`speed`, `isAlert`, `isChasing`) are updated on state changes for smooth transitions.  
- A debug overlay displays the current state and suspicion value above the enemy’s head for testing and grading.

---

### 🔹 Visual Summary

PatrolState
├── IdleState <--> MoveState (timer / waypoint)
│
└──→ AlertState (0.5 ≤ Suspicion < 1)
↓
ChaseState (Suspicion == 1)
↑
(Suspicion decreasing)

---

### 🔹 Design Notes
This FSM design enables scalable AI behavior:
- The **PatrolState hierarchy** keeps the base movement modular.  
- The **Suspicion-based system** allows smooth transitions instead of binary detection.  
- Easy to extend with future states (e.g., *SearchState*, *AttackState*, or *ReturnToBase*).

---

### 🔹 Video link
https://youtu.be/FAdAFl0p4ac

---