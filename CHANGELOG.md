# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
- Check out the [Trello](https://trello.com/b/YxTjwqkQ) page for the most current development info. 

## [0.0.1] - 2020-08-15
### Added
- Minimized AnimationClip sample rate based on Aseprite frame timing (per tag). 
- AnimationClip keyframe spacing derives from Aseprite. 
- Importer generates readonly AnimationClip subassets for each Aseprite tag. 
- SpriteAtlas: Register a folder containing any Aseprite files to include their Sprite subassets in the SpriteAtlas.
- Sprite pivot position determined by the center of the canvas in Aseprite (atlas is tightly packed).
- Aseprite files backed by a generated atlas containing the sprites for each frame. 