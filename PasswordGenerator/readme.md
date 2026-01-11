# Password Generator

A deterministic password generator built with .NET MAUI that creates unique, strong passwords based on a master password
and seed value.

## Features

- **Deterministic Generation**: Same master password + seed = same output password (always)
- **Cross-Platform**: Runs on Windows, Android, iOS, macOS, and (soon-to-come) Linux
- **No Storage**: Nothing is saved - all passwords are generated on-the-fly
- **Secure**: Uses Argon2id hashing algorithm (winner of Password Hashing Competition 2015)
- **Customizable**: Adjustable password length (8-256 characters) and optional special characters
- **Offline**: Works without internet connection
- **Visual Validation**: Clear error messages for missing inputs

## How It Works

The app uses the Argon2id cryptographic hashing algorithm to generate passwords:

```
Master Password + Seed → Argon2id → Deterministic Password
```

### Example Usage

```
Master Password: "MyStrongPassword123!"
Seed: "facebook"  → Generated: "Qh6w+ZJFim!1A1!"
Seed: "gmail"     → Generated: "X2k9-PLnr@8Bv3#"
Seed: "netflix"   → Generated: "M4t!Zx7+Wd5Yn2@"
```

The same inputs will **always** produce the same output, allowing you to regenerate passwords whenever needed.

## Getting Started

### Prerequisites

- .NET 10 SDK or higher
- Platform-specific requirements:
    - **Windows**: Windows 10 version 1809 or higher
    - **Android**: Android 5.0 (API 21) or higher
    - **iOS**: iOS 15.0 or higher
    - **macOS**: macOS 12 or higher

### Installation

1. Clone the repository:
```bash
git clone https://github.com/yourusername/PasswordGenerator.git
cd PasswordGenerator
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Build the project:
```bash
dotnet build
```

### Running the Application

#### Windows
```bash
dotnet run --framework net10.0-windows10.0.19041.0
```

#### Android
```bash
dotnet build -t:Run -f net10.0-android
```

#### iOS (requires Mac)
```bash
dotnet build -t:Run -f net10.0-ios
```

## Usage Guide

1. **Enter Master Password**: Your main password that you'll remember
2. **Enter Seed**: A unique identifier for each service (e.g., "facebook", "gmail")
3. **Set Length**: Choose password length between 8-256 characters
4. **Special Characters**: Toggle whether to include special characters (!@#$%&*-_+=)
5. **Generate**: Click to create your password
6. **Copy**: Click anywhere in the result box or use the copy button

### Best Practices

- **Master Password**: Choose a strong, memorable master password
- **Seed Strategy**: Use consistent naming (e.g., service names or domains)
- **Keep a Seed List**: Maintain a list of your seeds (safe even if exposed - useless without master password)
- **Password Rotation**: Add version numbers to seeds if you need to change passwords (e.g., "facebook-v2")

### Example Seed List
```
facebook
gmail
netflix
bankid
work-email
github
```

## Technology Stack

- **Framework**: .NET MAUI (.NET 10)
- **Language**: C# 14
- **Cryptography**: Argon2id via Konscious.Security.Cryptography.Argon2
- **UI Toolkit**: CommunityToolkit.Maui
- **Architecture**: MVVM-lite with Dependency Injection
- **Fonts**: Font Awesome 7 Free-Solid-900 and Electrolize-Regular

## Security

### Argon2id Configuration

```csharp
DegreeOfParallelism: 4    // 4 parallel threads
MemorySize: 131072        // 128 MB of memory
Iterations: 4             // 4 iterations
```

These settings provide strong protection against:
- Brute-force attacks
- Rainbow table attacks
- GPU/ASIC-based attacks

### Security Considerations

 **Strengths:**
- No password storage (nothing to breach)
- Memory-hard algorithm (expensive to attack)
- Deterministic (reliable regeneration)
- Open-source (auditable code)

 **Limitations:**
- If master password is compromised, all passwords are at risk
- Forgotten seeds cannot be recovered
- Requires trust in the implementation
- A safe and secure Password Manager is the safest bet for storing passwords

## Testing

To run unit tests (if implemented):
```bash
dotnet test
```


## Acknowledgments

- [Argon2](https://github.com/P-H-C/phc-winner-argon2) - Password hashing algorithm
- [.NET MAUI Community Toolkit](https://github.com/CommunityToolkit/Maui) - UI components
- [Konscious.Security.Cryptography](https://github.com/kmaragon/Konscious.Security.Cryptography) - Argon2 implementation

##  Contact and Support
Fredrik Magee - fredrikmagee@gmail.com

Project Link: [https://github.com/dadrikthedad/PasswordGenerator](https://github.com/yourusername/PasswordGenerator)

---

**Note**: This is a deterministic password generator. Always keep your master password secure and maintain a backup of your seed list!