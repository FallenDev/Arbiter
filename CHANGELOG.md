# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## Unreleased

### Added

- Severity level labels in console messages
- Console message right-click context menu for copy to clipboard
- Remember last size and position when opening application
- Text view of `ServerLegendMark` listing on `ServerUserProfileMessage` and `ServerSelfProfileMessage` inpsector views
- Text view of `ServerWorldListUser` listing on `ServerWorldListMessage` inspector view

### Changed

- Console exception messages are now one text run for easy text selection
- Reduced initial window size and layout splits
- Adjusted trace view toolbar layout for resizing
- Hide filter bar by default on trace
- Collapse `Users` on `ServerWorldListMessage` by default in inspector

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