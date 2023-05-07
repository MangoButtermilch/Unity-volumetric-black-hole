# Unity volumetric black hole
## Compute shader approach for rendering a volumetric black hole in Unity

[![Render](https://img.youtube.com/vi/bhgIQqslpN0/0.jpg)](https://www.youtube.com/watch?v=bhgIQqslpN0)
Video Showcase of current version: https://www.youtube.com/watch?v=aQ-_nNzhalA

## Resources
- The original approach for rendering the black hole comes from this blog post: https://medium.com/dotcrossdot/raymarching-simulating-a-black-hole-53624a3684d3
- The original approach for rendering volumetric clouds/nebulae comes from this tutorial: https://www.youtube.com/watch?v=0G8CVQZhMXw


## Benchmark


GPU |Resolution px | March steps  | Light steps | FPS | GPU LOAD
-- | -- | ------------- | ------------- | ------- | -- |
 GTX 1070 |480 x 270 | 500  | 4 | 60 (clamped) | 64% - 66%
 RTX 3060 |480 x 270 | 500  | 4 | 150 (unclamped) | 33% - 35%
 RTX 3060 |480 x 270 | 500  | 18 | 60 (unclamped) | 100%
 RTX 3060 |850 x 490 | 500  | 4 | 55 - 60 (unclamped) | 100%

## Volume textures

The provided (pseudo) 3D textures are created with my free volume generator tool:

![Volume generator](Screenshots/volume-generator.png?raw=true "Volume generator")(https://acetix.itch.io/pseudo-volume-generator)

## Example settings

![Example settings 1](Screenshots/settings-1.png?raw=true "Example settings 1")
![Example settings 2](Screenshots/settings-2.png?raw=true "Example settings 2")
