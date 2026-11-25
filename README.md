# CassieFix

An EXILED plugin for SCP: Secret Laboratory servers. It works around the reported CASSIE failure for announcements shorter than 1024 characters by padding the text sent to clients with trailing spaces. This covers built-in announcements, Remote Admin, and other plugins without changing the server's playback schedule or subtitle data.

The client-side cause has not been confirmed from the dedicated server assemblies alone. Test audible playback with a connected game client before relying on this workaround in production.

## Requirements

- SCP: Secret Laboratory Dedicated Server with the `CassieTtsPayload.WriteSelf` serializer used by the November 2026 build.
- EXILED 9.15.3 and its Harmony 2.2.2 dependency.
- .NET Framework 4.8 build tools.

## Build

Set these environment variables to your local DLLs, then build the solution:

```powershell
$env:ScpslManagedDir = 'D:\SteamLibrary\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed'
$env:ExiledApiPath = 'C:\path\to\Exiled.API.dll'
$env:HarmonyPath = 'C:\path\to\0Harmony.dll'
dotnet build CassieFix.sln -c Release
```

The plugin DLL will be in `src/CassieFix/bin/Release/net48/CassieFix.dll`. The game and EXILED DLLs are build references and are not included in this repository or the plugin distribution.

To run the serializer checks on Windows:

```powershell
dotnet build tests/CassieFix.SerializationTests/CassieFix.SerializationTests.csproj -c Release
& 'tests/CassieFix.SerializationTests/bin/Release/net48/CassieFix.SerializationTests.exe'
```

## Install and verify

Copy only `CassieFix.dll` to `%AppData%\EXILED\Plugins` and start or reload the server. Check the server log for any patch error. With a game client connected, compare a short Remote Admin CASSIE announcement and a normal in-game announcement. Their speech and subtitles should remain unchanged. Remove the DLL to disable the workaround.

The patch checks the game's serializer shape during loading and fails with an explicit error if a later game update changes that method. Empty announcements and messages already at least 1024 characters long are left alone.

## License

MIT; see [LICENSE](LICENSE).
