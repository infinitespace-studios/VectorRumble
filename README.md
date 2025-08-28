﻿# VectorRumble
MonoGame port of the XNA VectorRumble Sample, but enhanced to be a more complete publishable game sample.

## Enhancement over standard XNA Sample
* Extensibility
   * Custom ship loading
   * Custom level loading
* Custom font to suit Look & Feel
* Multi-language support
   * English
   * German
   * Spanish
   * French
   * Chinese (Simplified)
   * Japanese

## Build Status
[![.NET](https://github.com/infinitespace-studios/VectorRumble/actions/workflows/build.yml/badge.svg)](https://github.com/infinitespace-studios/VectorRumble/actions/workflows/build.yml)

## Released Platforms
* Desktop PCs
   * [Itch.IO](https://infinitespace-studios.itch.io/vector-rumble) (Linux, MacOS and Windows binaries)

## Contributions Opportunities

We have quite a few issues that are ripe for community contributions. This project accepts PRs :)

* A community written cross-platform [vector point editor](https://github.com/infinitespace-studios/VectorRumble/issues/15)
   * Maybe using Avalonia, built into the game itself or webbased?
   * Supports Level loading and editing
   * Support Ship loading and editing
* Extensible [weapons system](https://github.com/infinitespace-studios/VectorRumble/issues/16)
* Custom level, ship and weapon [repository]() (we'll try to host the best ones on this repo)
* There are several issues open to support more languages, take a look at the list and see if you can help translate
* [Network multi-player](https://github.com/infinitespace-studios/VectorRumble/issues/18)
* [AI Player](https://github.com/infinitespace-studios/VectorRumble/issues/17)

If you think you have found a bug or have a feature request, use our [issue tracker](https://github.com/infinitespace-studios/VectorRumble/issues). 
Before opening a new issue, please search to see if your problem has already been reported.  Try to be as detailed as possible in your issue reports.


### Subscription

If you'd like to help the MonoGame project by supporting us financially, consider supporting us via a subscription for the price of a monthly coffee.

Money goes towards hosting, new hardware and if enough people subscribe a dedicated developer.

There are several options on the MonoGame [Donation Page](http://www.monogame.net/donate/).


## Source Code

The full VectorRumble source code is available here from GitHub:
* Clone the source: `git clone https://github.com/infinitespace-studios/VectorRumble.git`

### Building with Effect Compilation

VectorRumble includes shader effects that require compilation. The project uses GitHub workflows to automatically set up the required dependencies (.NET 9 and Wine) for CoPilot environments.

```bash
dotnet build
```

### Running Tests

VectorRumble includes comprehensive unit tests to help prevent bugs. To run the test suite:

```bash
dotnet test VectorRumble.Tests/VectorRumble.Tests.csproj
```

The test suite covers:
- Utility functions (Helper class)
- Mathematical collision detection (Collision class)
- Custom collection types (CollectCollection)

All tests are isolated and don't require graphics context or audio hardware.

## Helpful Links

 * The official MonoGame website is [monogame.net](http://www.monogame.net).
 * The VectorRumble [issue tracker](https://github.com/infinitespace-studios/VectorRumble/issues) is on GitHub.
 * Follow [@InfSpaceStudios](https://twitter.com/InfSpaceStudios) on Twitter.

## License

The Vector Rumble project is under the [Microsoft Public License](https://opensource.org/licenses/MS-PL) except for a few portions of the code.  See the [LICENSE.txt](LICENSE.txt) file for more details.  Third-party libraries used by MonoGame are under their own licenses.  Please refer to those libraries for details on the license they use.
