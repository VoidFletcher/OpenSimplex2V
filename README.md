# OpenSimplex2V for Unity

**OpenSimplex2V** is a high-performance 3D noise implementation for Unity, optimized for **Burst** and **Unity Jobs**.  
This package is ideal for procedural generation, voxel terrain systems, simulations, and runtime content pipelines.

- Fully Burst-compatible
- Unity Jobs-friendly API
- Pure C# / NativeArray-based, GC-free
- Samples for benchmarking and integration

---

## Installation

### Option 1: Install via Git URL (recommended)

In Unity:

1. Open **Window → Package Manager**
2. Click the **➕ (plus icon)** → **Add package from Git URL...**
3. Paste the following: https://github.com/VoidFletcher/OpenSimplex2V.git
4. Click **Add**.

---

### Option 2: Add manually to `manifest.json`

Add this to your project's `Packages/manifest.json`:

```json
"com.voidfletcher.opensimplex2v": "https://github.com/VoidFletcher/OpenSimplex2V.git"
```
