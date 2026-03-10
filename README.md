# Stremio Magnet Grabber

A lightweight, portable Windows desktop application that extracts proper Magnet (Torrent) links from copied Stremio URLs. It automatically parses out the 40-character info-hash and copies a universal `magnet:?xt=urn:btih:hash` link directly to your clipboard.

## Features ✨
- **Zero Dependencies**: Built with native C# and Windows Forms. No Python, Node.js, or heavy frameworks required!
- **Portable**: Just download the `.exe` and run it anywhere on your Windows PC.
- **Aesthetic UI**: Custom Dark Mode, borderless window, flat design, and smooth fading animations.
- **Smart Extraction**: Automatically decodes URL-encoded Stremio strings and regex-matches torrent payloads.

## Usage 🚀
1. Grab the latest release (`MagnetGrabber.exe`) or build it from source.
2. Open the application.
3. Paste a Stremio URL or an encoded string containing an info-hash.
4. Click **"Mıknatısla! (Çıkar ve Kopyala)"**
5. Success! The pure magnet URL is instantly copied to your clipboard, ready to be pasted into your Torrent client.

## Building from source 🛠️
You do not need Visual Studio installed to compile this! The .NET Framework compiler (`csc.exe`) is already built into modern Windows versions.

1. Clone or download this repository.
2. Run the included `build.bat` script.
3. A newly compiled `MagnetGrabber.exe` will appear in the main directory.

