# Cyber-Hunter

Cyber-Hunter is an Augmented Reality (AR) game where players embark on a thrilling adventure to find hidden artifacts in the game environment. The score increases for each artifact found, offering an engaging and immersive experience for users.

---

## Table of Contents

- [Features](#features)
- [Prerequisites](#prerequisites)
- [Setup Instructions](#setup-instructions)
- [Running the Project](#running-the-project)
- [Contributing](#contributing)
- [License](#license)
- [Contact](#contact)

---

## Features

- **AR Gameplay**: Cyber-Hunter uses cutting-edge AR technology to provide an interactive gaming experience.
- **Artifact Detection**: Players search for and collect artifacts to earn points.
- **Cross-Platform Support**: Compatible with both Android and iOS devices.
- **Built with Unity**: Developed using Unity, utilizing C#, ShaderLab, Mathematica, and HLSL.

---

## Prerequisites

Before setting up the project, ensure you have the following:

1. **Unity Editor**:
   - Install Unity Hub: [Unity Hub](https://unity.com/download).
   - Install the Unity Editor version specified in `ProjectVersion.txt` in the project root directory.

2. **Git**:
   - Clone the repository using Git: [Download Git](https://git-scm.com/).

3. **AR Development Environment**:
   - Install AR Foundation and ARKit (for iOS) or ARCore (for Android) packages in Unity.

4. **Supported Platforms**:
   - Android
   - iOS

---

## Setup Instructions

Follow these steps to set up the project:

1. **Clone the Repository**:
   Clone the repository using the following command in your terminal or command prompt:
   ```bash
   git clone https://github.com/Youssef-Ashraf2099/Cyber-Hunter.git
   ```
   2. **Open the Project in Unity**:
   - Open Unity Hub.
   - Click the "Open" button.
   - Navigate to the folder where the repository was cloned and select it.

3. **Install Required Packages**:
   - Open Unity.
   - Go to `Window > Package Manager`.
   - Install the following packages if not already installed:
     - AR Foundation
     - ARKit XR Plugin (iOS)
     - ARCore XR Plugin (Android)

4. **Set Up Build Platform**:
   - Go to `File > Build Settings`.
   - Choose your target platform (iOS or Android).
   - Click "Switch Platform" to apply changes.

---

## Running the Project

1. **Connect Your Device**:
   - For iOS: Connect your iPhone/iPad and ensure Xcode is installed on your Mac.
   - For Android: Connect your Android device with USB debugging enabled.

2. **Build the Project**:
   - Go to `File > Build Settings`.
   - Select the appropriate platform.
   - Click "Build and Run."

3. **Play the Game**:
   - Once the build is complete, the game will be installed on your device.
   - Launch the app and start searching for artifacts to increment your score!

---

## Contributing

We welcome contributions to make Cyber-Hunter even better! Here's how you can contribute:

1. **Fork the Repository**:
   - Click the "Fork" button on the top-right corner of this repository.

2. **Clone Your Fork**:
   ```bash
   git clone https://github.com/<your-username>/Cyber-Hunter.git
