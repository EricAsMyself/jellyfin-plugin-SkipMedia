# Jellyfin EDL Skipper Plugin

A Jellyfin plugin that automatically skips specified portions of videos using `.edl` (Edit Decision List) files.

## Features

- Automatically seeks over time ranges defined in a `.edl` file during video playback.
- Supports multiple skip segments per video.
- Runs server-side and applies to all sessions.
- Fast skip execution using a background timer.
- EDL files are plain text files stored alongside media.

> ❗ This plugin **only supports skipping (EDL type 3)**. Audio will continue during skips (no muting).  
> ❗ It does **not** support pausing, muting, or other EDL types (1 or 2).

---

## How It Works

For each video being played, the plugin checks if there’s a `.edl` file with the same name. If so, it will parse the skip ranges and automatically seek past those segments during playback.

For example, if you're watching:

```
/media/movies/MyMovie.mkv
```

You must also have an EDL file at:

```
/media/movies/MyMovie.edl
````

### Example `.edl` File

```text
0.00 1200.00 3
3600.00 5400.00 3
````

This file means:

* Skip from **0:00** to **20:00** (1200 seconds)
* Skip from **1:00:00** to **1:30:00** (3600 to 5400 seconds)

The third column (`3`) means **"cut/skip"**, which is the only supported action.

---

## Installation

1. Clone this repository or download the release.
2. Build the plugin and place the `.dll` in your Jellyfin `plugins` folder.
3. Restart Jellyfin.
4. Place `.edl` files next to your videos with the same name and `.edl` extension.

---

## Compatibility

* ✅ Tested on Jellyfin 10.10.7
* ⚠️ Does not support mute or pause actions.

---

## Known Limitations

* Cannot mute audio — only seeks forward.
* Does not validate overlapping segments.
* No web UI — configuration is entirely file-based.
* This doesn't seem to work with non web based playback.

---

## Contributing

PRs are welcome! If you'd like to add support for other EDL types, client muting, or GUI configuration, feel free to fork and contribute.

---

## Some more comments

This is my first plugin for Jellyfin. I'm happy to add other features, some just couldn't be added because of plugin supports.


---

```

Let me know if you'd like me to also include a section for building instructions or contributing guidelines.
```
