#  Unity Slot Machine Game

##  Overview
This is a slot machine game built in Unity to demonstrate core game development concepts such as animation systems, RNG, UI handling, and clean architecture.

The game simulates a classic 3-reel slot machine with smooth reel animations, weighted randomness, and engaging bonus mechanics.

---

##  Features

### Core Features
- Smooth reel spinning with acceleration and deceleration  
- Accurate stop alignment with snap-to-center correction  
- Weighted random number generation using ScriptableObjects  
- Dynamic betting system (increase/decrease bet)  
- Win evaluation logic with wild symbol support  

### Bonus Systems
- Win streak multiplier system  
- Free spins triggered by special combinations  
- Progressive jackpot system  

### UX & Polish
- Delayed win popup for improved game feel  
- Input locked during spin and result phase  
- Visual highlight of winning symbols  
- Event-driven UI updates  

---

##  Architecture / Approach

The project follows a modular, event-driven architecture:

- **GameManager** → Handles balance, betting, and overall game flow  
- **SlotMachine** → Core spin logic and result evaluation  
- **ReelController** → Reel animation and symbol positioning  
- **SymbolData (ScriptableObject)** → Defines symbol weights and multipliers  
- **UIManager** → UI updates and player interaction  
- **PopupManager** → Win popup timing and animation  

### Key Design Decisions
- Used **event-driven architecture** to decouple UI from gameplay  
- Implemented **weighted randomness** for realistic slot behavior  
- Designed smooth reel animations with easing  
- Maintained clear separation of responsibilities  

---

##  How to Run (WebGL Build)

 **Important:** WebGL builds do NOT work if you double-click `index.html`.

### Option 1 — VS Code Live Server

1. Install the Live Server extension in VS Code
2. Open the Build/WebGL folder in VS Code
3. Right-click index.html
4. Click "Open with Live Server"

### Option 2 — Run with Python 

1. Open terminal inside: Build/WebGL
2. Run: python -m http.server
3. Open in browser: http://localhost:8000