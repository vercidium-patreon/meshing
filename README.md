This repository is Part 1 of Vercidium's [Free Friday](https://www.patreon.com/posts/100857028) series.

You can get access to the full Vercidium Engine source code by joining [my Patreon](https://www.patreon.com/vercidium) or sponsoring me on [GitHub sponsors](https://github.com/vercidium-patreon).

---

This is a standlone voxel renderer that uses greedy meshing to generate a simplified mesh of a voxel world.

This project uses Silk.NET so it *should* be cross-platform, but I've only tested it on Windows.

Key files:
- `ChunkMeshActual.cs` contains the greedy meshing algorithm
- `Program.cs` contains the main entry point
- `Client.cs` contains the render loop

![image](https://github.com/vercidium-patreon/meshing/assets/12014138/125ca9d5-14bf-4e8c-9fbd-76f4ef6d64b9)
