# Memory VR Game (Restorer)
**Open-Source Reconstruction of a Cognitive Memory Task for Eye-Tracking–Based Cognitive Load Assessment**

<p align="center">
  <img src="images/gameplay.gif" width="700" height="700">
</p>

## Short Description
**Memory VR Game (Restorer)** is an open-source Unity3D reconstruction of the *Restorer* memory task from the [Neuronation](https://www.neuronation.com/) cognitive training app, adapted for immersive virtual reality.    

The goal of this reconstruction is to provide a controlled, open-source, research-grade memory task suitable for cognitive science, HCI, and VR experimentation. This VR version was developed as part of a research study examining **intrinsic cognitive load** using **non-intrusive eye-tracking indicators**.   

The game presents participants with a grid of objects during a 8-second memorization phase. After this phase, some or all objects disappear, and the participant must recall and fill the empty positions by selecting the correct objects from a set of visual options. Across 26 levels of increasing complexity, the game induces varying degrees of intrinsic cognitive load by increasing memory and spatial recall demands.   

 To isolate intrinsic load and minimize extraneous cognitive effort, the VR environment was intentionally designed to be minimalistic and distraction-free: a plain room, neutral lighting, and a virtual screen directly in front of the participant. Participants interacted using VR controllers, and all gameplay was preceded by gaze calibration and task familiarization.

<p align="center">
  <img src="images/env.png" width="500" height="500">
</p>

During gameplay, two main data streams were recorded in real time:

- **Eye-tracking metrics**:  
  Gaze origin, gaze direction, gaze validity, blink status, gaze hit point in 3D space, and the currently viewed virtual object.

- **Task performance metrics**:  
  Level number, task start and end time, duration, selected objects, and correctness.


## Tools & Technologies Used

This project was developed, tested, and deployed using the following hardware and software setup. These specifications are provided to support reproducibility and consistent VR performance across research environments.

## System Specifications (Development & Testing)

- **Windows 10 Pro 64-bit**  
- **AMD Ryzen 7 3700X**  
- **32 GB DDR4 RAM**
- **NVIDIA GeForce RTX 2080**
  
## Unity Development Environment
- **Unity Hub** :  Version Used: 3.10.0
- **Unity Editor** : Unity 2020.3.30f1 (LTS)  
 

## SDKs & VR Tools
- **Tobii XR SDK for Unity** - TobiiXRSDK_3.0.1.179
- **HTC VIVE Headset(Tobii Eye Tracking VR Retrofit Service)**
- **Brick Project Studio** - Apartment Kit
- **SteamVR 2.14.3**
- **Vive Console**
- **SRanipal Runtime**

## How to Run the Game

### 1. Install the following tools :
- Unity Hub , Unity Editor , SteamVR , Vive Console , SRanipal Runtime
### 2. Download **Tobii XR SDK for Unity** (version `3.0.1.179`)
### 3. Clone the Repository
```bash
git clone https://github.com/TugaAhmed/MemoryGame_Restorer.git
```
### 4. Add Tobii XR SDK to the Project
Restorer/Packages/TobiiXRSDK_3.0.1.179
### 5. Open the Project in Unity Hub
- Open Unity Hub
- Click Add
- Select the cloned Restorer project folder
- Open the project
- Accept the Tobii license agreement when prompted

### 6. Open the Main Scene
In the Unity Project window, navigate to:  
**Assets/Samples/Tobii XR SDK/3.0.1/Restorer_Scene/Restorer_Scene.unity**

### 7. Verify Required Unity Packages
From the Unity menu, open:
- **Window → Package Manager**
- Confirm that the following packages are installed:
  * XR Interaction Toolkit
  * XR Plug-in Management
  * OpenXR

### 8. Verify Project Settings
- Open: **Edit → Project Settings**
- Confirm the following:
  * XR Plug-in Management
  * OpenXR is enabled for PC / Standalone
  * OpenXR → Interaction Profiles
  * HTC Vive Controller Profile enabled
  * Oculus Touch Controller Profile enabled
-Ensure SteamVR is running in the background

### 9. Run the Game
- Connect the VR headset
- Launch SRanipal Runtime
- Press Play in Unity
