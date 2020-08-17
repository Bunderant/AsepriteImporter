# Unity Aseprite Importer

Work-in-progress. The short-term goal is to automatically import Aseprite files, mapping tags to AnimationClip assets. This **does** require a local [Aseprite](https://www.aseprite.org/) installation to function, since I'm currently depending on their command line interface. 

# (Very) Basic Usage

I'm not recommending this for use just yet. Unless you want to build on this project yourself, wait at least until version 0.1.0 for a stable feature set. 

I'm currently working in Unity 2019.4. First, install via the package manager (instructions [here](https://docs.unity3d.com/Manual/upm-ui-giturl.html)). Under this package's "Settings" directory, specify your Aseprite installation's global path within the settings asset. 

For Aseprite files, any tagged frames will be imported as AnimationClip assets, and the Sprite pivot points correspond to the center of your Aseprite canvas. There is no support yet for renaming or reordering tags, but that'll be in version 0.1.0 (with restrictions). I'll have a more general workflow guide up along with 0.1.0. 

Now, you should be able to drag Aseprite files into your project and use the generated (readonly) AnimationClips and Sprites as you see fit. Support for modifying the backing texture of the associated atlases is forthcoming. 

# Attribution

I would be remiss not to mention the excellent alternatives floating around. I dug into **Seanba**'s [Aseprite2Unity](https://github.com/Seanba/Aseprite2Unity) a bit, and flirted with the idea of just forking their project as something compatible with Unity's package manager. Ultimately, I decided to move in my own direction, as I have some specific needs from the importer. Maybe someday I'll reincorporate some of Seanba's project and give due credit, and in the process remove my dependency on an Aseprite installation. 

Also, a quick thank you to **thammin**, for creating a package template that follows the guidelines laid out in Unity's official documentation. It helped get this project off the ground more quickly, and it might help you with your own packages. I haven't checked compatibility with 2020.1 which has some major changes to the package manager : [https://github.com/thammin/unity-custom-package-template]
