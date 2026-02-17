# PolyLauncher - Changelog

All notable changes to this project will be documented in this file.

## [1.0.0] - Initial Release

### Added
- ? **Protocol Handler**: Register as `polytoria://` handler in Windows
- ? **Auto-Update System**: Download and install client updates automatically
- ? **Version Management**: Track installed versions per release (Stable/Beta)
- ? **Customizable UI**: Colors, text, icons, and duration
- ? **Configuration Window**: Full GUI for settings management
- ? **First-Run Experience**: Guided setup on initial launch
- ? **Countdown Timer**: Configurable 1-60 seconds with interrupt
- ? **Progress Reporting**: Real-time download progress
- ? **Modding Framework**: Placeholder for HWID spoofing and executor
- ? **Process Monitoring**: Monitor game process for modding integration

### Features
- Multi-release support (client, clientbeta, test, testbeta)
- Command-line interface (--configure, --register-protocol, --reset-settings)
- JSON-based settings persistence
- MVVM architecture for maintainability
- Modern, transparent UI design
- Maintenance mode detection
- Error handling and user feedback

### Technical
- Built with .NET 8.0 and WPF
- Uses CommunityToolkit.Mvvm for MVVM pattern
- Newtonsoft.Json for settings serialization
- Full async/await pattern for responsive UI
- Service-based architecture

### Documentation
- README.md - User documentation
- DEVELOPER_GUIDE.md - Technical documentation
- USAGE_EXAMPLES.md - How-to guides
- PROJECT_SUMMARY.md - Comprehensive overview

---

## [Unreleased] - Future Planned Features

### To Be Implemented
- ?? **Self-Update**: Update launcher without external updater
- ?? **HWID Spoofing**: Hardware ID spoofing implementation
- ?? **Executor Integration**: DLL injection and script execution
- ?? **Multi-Account**: Support for multiple Polytoria accounts
- ?? **Launch History**: Track and display game launch history
- ?? **Version Rollback**: Easily switch between client versions
- ?? **Crash Reporting**: Automatic crash detection and reporting
- ?? **Themes**: Pre-made color themes
- ?? **Plugins**: Plugin system for extensions
- ?? **Advanced Logging**: Detailed logging system

### Potential Enhancements
- Cross-platform support (Avalonia UI)
- Discord Rich Presence integration
- Game time tracking
- Performance monitoring
- Custom fonts support
- Background images/videos
- Animated loading screens
- Sound effects
- Theme marketplace

---

## Version History Template

### [Version Number] - YYYY-MM-DD

#### Added
- New features that have been added

#### Changed
- Changes in existing functionality

#### Deprecated
- Features that will be removed in upcoming releases

#### Removed
- Features that have been removed

#### Fixed
- Bug fixes

#### Security
- Security improvements

---

## How to Contribute Updates

When making changes:
1. Update this CHANGELOG.md
2. Increment version in PolyLauncher.csproj
3. Tag release in git (if using version control)
4. Build and test thoroughly
5. Document breaking changes clearly

---

## Versioning

This project follows [Semantic Versioning](https://semver.org/):
- **MAJOR** version: Incompatible API changes
- **MINOR** version: Add functionality (backwards-compatible)
- **PATCH** version: Bug fixes (backwards-compatible)

Example: `1.2.3`
- 1 = Major version
- 2 = Minor version  
- 3 = Patch version

---

## Release Checklist

Before releasing a new version:
- [ ] Update version number in csproj
- [ ] Update CHANGELOG.md
- [ ] Update README.md if needed
- [ ] Run full test suite
- [ ] Test on clean install
- [ ] Build release configuration
- [ ] Create release notes
- [ ] Tag in version control
- [ ] Archive previous version

---

**Current Version**: 1.0.0  
**Status**: Stable  
**Last Updated**: 2024
