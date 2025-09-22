# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Additional sub-second rates for sending packets
- `Shift+Enter` hotkey for sending packets in the send window
- `InputDescription` for `ServerShowDialogMessage` packet
- Redesigned settings window
- `CanMove` property for `ServerUserIdMessage` packet
- Autosave for traces on application exit

### Changed

- Rename `Aisling` to `Reactor` for dialog entity type
- Rename `TextInput` to `Arguments` for `ClientDialogMenuChoiceMessage` message
- Increase command filter box width slightly
- Fixed `InterfacePane` enum values
- Display `Name` for entities in Inspector
- Remove most transition animations in the UI (need to re-standardize them)
- Selected tab background color
- Fix spacing in search and filter bars
- Fix disable style for `ComboBox` dropdown button
- Reduce corner radius on input controls
- Fixed local port not working if other than default
- Swap order of scroll to end and clear in `Console` view for consistency

## [0.9.4b] - 2025-09-20

### Changed

- `ClientExitRequestMessage` now has optional `Reason` field (it is not always provided)
- Clear text field button size
- Copying a decrypted packet now includes `<` or `>` direction for `Send` window convenience
- Send syntax allows spacing between `>`/`<` and command code

## [0.9.4] - 2025-09-20

### Added

- CRC16 and CRC32 checksum algorithms
- Encrypt algorithm for server packets
- Encrypt algorithm for client packets (auto-insert `0x00` byte)
- Dialog encryption algorithm for client packets
- Unit tests for network and encryption algorithms
- `PacketException` event invoked when the client tries to send `0x42` exception packet
- Sending of raw packets to client/server
- Transition animations to various UI components
- `PacketQueued` event to `ProxyConnection` and `ProxyServer` classes

### Changed

- Client name filter now matches exactly (unless `*` or `?` are used)
- Renamed a few enum types
- Renamed `MenuChoice` to `DialogMenuChoice` for clarity
- Renamed `ClientMenuChoiceMessage` to `ClientDialogMenuChoiceMessage` for consistency
- Renamed `ShowMenu` to `ShowDialogMenu` for consistency
- Renamed `ServerShowMenuMessage` to `ServerShowDialogMenuMessage` for consistency
- Renamed `MenuChoice` to `Slot` for `ClientDialogMenuChoiceMessage` message
- Decrypting client packets will now remove the trailing padding `0x00` byte
- `NetworkEncryptionParameters` are now read-only for thread safety
- Blocking of outgoing `0x42` client exception packets to the server (still logged/traced locally)
- Refactored a lot of internal systems for clarity and performance
- Improve performance of console log and counting of log entries
- Rename `Console Log` to just `Console` in the tab view
- Fix decimal mode not being set by default in the Hex view when empty
- Fix resize grip hit test area
- Adjust combo box dropdown button size

## [0.9.3] - 2025-09-18

### Added

- `Copy Selected` context menu for trace packets
- `Save Selected` context menu for trace packets
- `Delete Selected` context menu for trace packets
- Double-click on client in list to bring game window to foreground (Windows only)
- Set game client window title to include client name on login (cleared on logout)

### Changed

- Allow multiple selection of trace packets (only first item is inspected)
- Fixed `Shift` modifier getting stuck when loading traces in append mode
- Trace packet list item hit test areas
- Context menu visual design improvements
- Disable `Launch Client` button for non-Windows operating systems

## [0.9.2] - 2025-09-18

### Added

- Severity level labels in console messages
- Console message right-click context menu for copy to clipboard
- Remember last size and position when opening application
- Text view of `ServerLegendMark` listing on `ServerUserProfileMessage` and `ServerSelfProfileMessage` inpsector views
- Text view of `ServerWorldListUser` listing on `ServerWorldListMessage` inspector view
- Copy to JSON representation from inspector
- Tooltips for inspector fields
- Find packet by command with next/prev navigation (`Ctrl+[` and `Ctrl+]` hotkeys)

### Changed

- Console exception messages are now one text run for easy text selection
- Reduced initial window size and layout splits
- Adjusted trace view toolbar layout for resizing
- Hide filter bar by default on trace
- Collapse `Users` on `ServerWorldListMessage` by default in inspector
- Renamed `Type` to `AbilityType` in `ServerCooldownMessage` message
- Renamed `PreviousX` and `PreviousY` to `OriginX` and `OriginY` on `ServerEntityWalkMessage` message
- Renamed `ShowPlayer` to `ShowUser` for consistency across wording
- Renamed `MessageType` to `ResultType` for `ServerLoginResultMessage` message
- Renamed `HasGroupInvite` to `IsRecruiting` for `ServerSelfProfileMessage` message
- Renamed `DialogId` to `StepId` and `SourceId` to `EntityId` for `ServerShowDialogMessage` message
- Renamed `SourceId` to `EntityId` and `Args` to `Prompt` for `ServerShowMenuMessage` message
- Renamed `IsTransparent` to `IsTranslucent` for `ServerShowUserMessage` message
- Renamed `HasUnreadParcels` to `HasAvailableParcels` in `ServerUpdateStatsMessage` message
- Display name overrides for nested types
- Made most nested types collapsed by default in inspector
- Allow bit flags to display on 16-bit and 32-bit values
- Change filter hotkey to `Ctrl+G` (in preparation for `Ctrl+F` for find)
- Adjust hotkey gesture text rendering

## [0.9.1] - 2025-09-17

### Added

- Exception view for inspector view errors
- Improved client view with map location and stats
- `ClientLoggedIn` event for `ProxyServer`, invoked when server sends `0x05` packet
- `ClientLoggedOut` event for `ProxyServer`, invoked when server sends `0x4C` packet

### Changed

- Renamed `DyeColor` to `Color`
- `ClientAuthenticated` event now invoked when client sends `0x10` packet
- Clear login state when client explicitly exits
- Remove `Unknown` from `ServerRequestPortraitMessage` packet/mapping
- Fixed `Name` and `GroupBox` properties for `ServerShowPlayerMessage` message when in monster form

## [0.9.0] - 2025-09-16

### Added

- Initial preview release
- Launch game client support
- Network packet tracing with decryption
- Strong-typed packet inspection
- Raw packet hex view
- Load/save packet traces
- Console logging
- Simple user settings