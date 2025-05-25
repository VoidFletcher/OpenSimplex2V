# OpenSimplex2V for Unity

**OpenSimplex2V** is a high-performance 3D noise implementation optimized for **Burst** and **Unity Jobs**. Authored by Dylan Engelbrecht (VoidFletcher), based on the OpenSimplex2 algorithm by KdotJPG.

This package is ideal for procedural generation, voxel terrain systems, simulations, and runtime content pipelines.

- Fully Burst-compatible
- Unity Jobs-friendly API
- Pure C# / NativeArray-based, GC-free
- Samples for benchmarking and integration

---

## Usage
Coming soon. See sample in the meantime.

---

## Benchmarks
Coming soon.

- ~360 Million Samples per Second | Ryzen 9 3900XT

---

## Installation

### Option 1: Install via Git URL (recommended)

In Unity:

1. Open **Window → Package Manager**
2. Click the **➕ (plus icon)** → **Add package from Git URL...**
3. Paste the following: https://github.com/VoidFletcher/OpenSimplex2V.git
4. Click **Add**.

Optional, but highly recommended:

5. In the OpenSimplex2V package within the package manager - switch to the "samples" tab and import the benchmark sample.
6. Drag the sample prefab into your scene.
8. Press Play and look at the console.

---

### Option 2: Add manually to `manifest.json`

Add this to your project's `Packages/manifest.json`:

```json
"com.voidfletcher.opensimplex2v": "https://github.com/VoidFletcher/OpenSimplex2V.git"
```

---

## Credits
Based on the Open Simplex 2 algorithm by KdotJPG - https://github.com/KdotJPG/OpenSimplex2

---

### 🙏 Attribution (Optional but Appreciated)

Attribution is not required under the MIT License, but it is **highly appreciated**.

If you use this package in a public project — especially a shipped game, tool, or asset — you're welcome (but not obligated) to credit:

> **Dylan Engelbrecht (VoidFletcher)**  
> https://github.com/VoidFletcher/OpenSimplex2V

Feel free to [open an issue](https://github.com/VoidFletcher/OpenSimplex2V/issues) or message me to be included in a credits list of projects using this package.

---

### 📜 License

This project is licensed under the MIT License.
