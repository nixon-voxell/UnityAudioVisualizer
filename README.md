# Unity Audio Visualizer

![Demo](./Pictures/SmartAssistantAudioDemoHD.png)

This is a package than handles audio processing and visualization. Anyone can use this package in any way they want as long as they credit the author(s) and also respect the [license](LICENSE) agreement.

- [Unity Audio Visualizer](#unity-audio-visualizer)
    - [Audio Visualizer](#audio-visualizer)
  - [Installation](#installation)
  - [Support the project!](#support-the-project)
  - [License](#license)


### Audio Visualizer

The audio visualizer is made using custom VFX Graph and custom Shader (Wireframe Shader). It also utilizes Unity's Job System and Burst Compiler to manipulate each triangles of the mesh of the audio visualizer.

## Installation

This package only supports the Universal Render Pipeline.

External dependencies:

- VX Util ([UnityUtil](https://github.com/voxell-tech/UnityUtil))

1. Setup a project with URP as it's SRP.
2. Clone the [UnityUtil](https://github.com/voxell-tech/UnityUtil) repository into your `Packages` folder.
3. Clone this repository into your `Packages` folder.
4. And you are ready to go!

## Support the project!

<a href="https://www.patreon.com/voxelltech" target="_blank">
  <img src="https://teaprincesschronicles.files.wordpress.com/2020/03/support-me-on-patreon.png" alt="patreon" width="200px" height="55px"/>
</a>

<a href ="https://ko-fi.com/voxelltech" target="_blank">
  <img src="https://uploads-ssl.webflow.com/5c14e387dab576fe667689cf/5cbed8a4cf61eceb26012821_SupportMe_red.png" alt="kofi" width="200px" height="40px"/>
</a>

## License

This repository as a whole is licensed under the Apache License 2.0. Individual files may have a different, but compatible license.

See [license file](./LICENSE) for details.