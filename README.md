# 🧠 AI Simulation: FSM & A* Pathfinding

<p align="center">
  <img src="https://github.com/user-attachments/assets/6c7e95f7-cef0-4e4c-813d-c57273e8eb20" alt="Enemy Patrol State Visualization" width="49%" />
  <img src="https://github.com/user-attachments/assets/345e5fcb-b7f3-4f36-96d7-f49f3e363024" alt="Enemy Chase State Visualization" width="49%" />
</p>
<p align="center">
  <em>Left: <b>Patrol State</b> — visualizing grid-based pathfinding logic and node traversal.<br>
  Right: <b>Chase State</b> — visualizing Line-of-Sight detection rays (white lines) and logic-based targeting.</em>
</p>

## 📖 About The Project
This project is a technical showcase of artificial intelligence mechanics in game development. It focuses on custom pathfinding and decision-making logic built entirely from scratch, demonstrating an advanced understanding of algorithms and AI architecture without relying on Unity's built-in NavMesh system.

## 🛠️ Key Technical Features
* **Custom A-Star Pathfinding Algorithm:** Developed a grid-based A* pathfinding system that calculates the most efficient route to a target while accurately navigating around obstacles.
* **Finite State Machine (FSM):** Designed a modular and scalable state machine to control the AI agent's behavior, allowing smooth transitions between 4 distinct states (e.g., `Patrol`, `Chase`, `Attack`, `Search`).
* **Algorithm Visualization:** Included visual debugging tools (via Unity Gizmos) to display grid nodes, calculated paths, and AI state changes in real-time.
* **Clean Architecture:** Strictly separated the pathfinding logic from the state management components, ensuring highly reusable and optimized C# code.

## 💻 Built With
* **Game Engine:** Unity 3D
* **Language:** C#
* **Core Concepts:** Data Structures, Graph Traversal, State Machines

## 🚀 How to Run the Project
1. Clone the repository: 
   ```bash
   git clone [https://github.com/Rudnikua/FSM.git](https://github.com/Rudnikua/FSM.git)
2. Open the project folder via Unity Hub (Recommended version: 6000.2.6f2).
3. Navigate to the `Scenes` folder and open the main scene.
4. Press the **Play** button in the Unity Editor to start the game.
