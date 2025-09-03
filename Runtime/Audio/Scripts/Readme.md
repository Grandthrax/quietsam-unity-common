# 🎵 Audio System

## Overview
A singleton AudioManager, ScriptableObject-based audio assets.

## System Architecture

### Core Components
- **AudioManager**: Singleton controller managing all audio playback
- **SfxEvent**: ScriptableObject for configuring sound effects
- **MusicTrack**: ScriptableObject for music configuration

## Quick Start

### 1. Setup AudioManager
1. Create an empty GameObject in your scene
2. Create an AudioMixer [Create->Audio->AudioMixer]
3. In the AudioMixer create subgroups Music, SFX, UI
4. Expose volume for each and rename exposed parameter to "MasterVol", "MusicVol", "SFXVol", "UIVol"
5. Add the "AudioManager" script component to gameobject
3. Assign the AudioMixer in the inspector
4. Set the Music Mixer Group
5. Configure the starting music track (optional)

### 2. Create Audio Assets
1. Right-click in Project window → Create → Audio → SFX Event
2. Right-click in Project window → Create → Audio → Music Track
3. Configure the assets in the inspector


### Usage

#### Playing Sound Effects
```csharp
// Simple playback
AudioManager.Instance.Play(mySfxEvent);

// 3D positioned sound
AudioManager.Instance.PlayAt(mySfxEvent, null, transform.position);

// Sound that follows an object
AudioManager.Instance.PlayAt(mySfxEvent, playerTransform, Vector3.zero);

// Stop a specific audio source
AudioManager.Instance.Stop(audioSource);
```

#### Music Management
```csharp
// Play music with crossfade
AudioManager.Instance.PlayMusic(myMusicTrack);

// Play with custom fade time
AudioManager.Instance.PlayMusic(myMusicTrack, 2.0f);

// Stop music with fade out
AudioManager.Instance.StopMusic(1.0f);

// Pause/unpause music
AudioManager.Instance.PauseMusic(true);
AudioManager.Instance.PauseMusic(false);

// Change music pitch
AudioManager.Instance.SetMusicPitch(1.2f);
```

#### Music Queue System
```csharp
// Add music to the end of the queue
AudioManager.Instance.AddToQueue(myMusicTrack);

// Add music to the front of the queue (plays next)
AudioManager.Instance.AddToQueueFront(myMusicTrack);

// Skip to the next track in the queue
AudioManager.Instance.SkipToNext();

// Clear the entire music queue
AudioManager.Instance.ClearQueue();

// Get information about the queue
int remainingTracks = AudioManager.Instance.GetQueueCount();
MusicTrack currentTrack = AudioManager.Instance.GetCurrentTrack();
bool isEmpty = AudioManager.Instance.IsQueueEmpty();
```

**Note**: The music queue automatically plays the next track. Starting tracks in the inspector is the initial queue. When the queue reaches the end it automatically loops back to the starting music.

#### Volume Control
```csharp
// Set volume programmatically (0.0 to 1.0)
AudioManager.Instance.SetVolume("MasterVol", 0.5f);
AudioManager.Instance.SetVolume("MusicVol", 0.7f);
AudioManager.Instance.SetVolume("SFXVol", 0.8f);
AudioManager.Instance.SetVolume("UIVol", 0.6f);
```

### SfxEvent Configuration

#### Basic Settings
- **Clips**: Array of AudioClips (randomly selected)
- **Volume**: Base volume (0.0 to 1.0)
- **Volume Jitter**: Random volume variation range. Normally leave as default
- **Pitch Jitter**: Random pitch variation range. Normally leave as default

#### Spatial Audio
- **Spatial Blend**: 0 = 2D, 1 = 3D
- **Rolloff Mode**: Logarithmic (realistic), Linear, or Custom. Choose Logarithmic unless you have a good reason
- **Min/Max Distance**: Closest distance if full volume. Furtherst is zero volume
- **Priority**: Audio source priority (0-256)

#### Behavior
- **Loop**: Whether the sound should loop
- **Allow Multiple**: Can multiple instances play simultaneously
- **Min Repeat Delay**: Minimum time between repeated plays
- **Respect Time**: Pause with game pause 

### MusicTrack Configuration

#### Basic Settings
- **Track**: The AudioClip to play
- **Output**: AudioMixerGroup (optional, defaults to music group). Don't change without a good reason
- **Volume**: Track volume (0.0 to 1.0)


### IMPORTANT!
Your AudioMixer must expose these parameters for volume control:
- `MasterVol`
- `MusicVol`
- `SFXVol`
- `UIVol`

### Setup Steps
1. Open your AudioMixer
2. Select Master Volume
2. In top right right-click on volume → Expose Volume
3. In the main Audio Mixer tab in the top right you can see exposed parameters
3. Rename the exposed parameter to `MasterVol`
4. Repeat for Music, SFX, and UI groups
