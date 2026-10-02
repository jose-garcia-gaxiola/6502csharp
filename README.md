# olcNES c#
c# NES Emulator

This is a project based on: https://github.com/OneLoneCoder/olcNES
from c++ to c# just because... why not!?

## CPU lookup

The CPU targets the NES Ricoh 2A03/2A07 (NMOS 6502-derived), not the CMOS
65C02. The constructor initializes 256 opcode entries directly, each with an
operation, addressing mode, and base cycle count. Indexed reads can add a
page-crossing cycle; indexed stores and read-modify-write instructions use
their fixed cycle counts.

The supported unofficial NOPs consume their operands, and opcode `0xEB` uses
immediate SBC. Other unofficial instructions remain placeholders; this is
not a complete implementation of all NES CPU opcodes. BRK uses implied
addressing because its operation already advances past the padding byte.

Run the dependency-free lookup regression checks with .NET 10:

```sh
dotnet run --project tests/Components.RegressionTests.csproj
```

## CI and NuGet publishing

The [GitHub Actions workflow](.github/workflows/feat-build.yml) restores and
builds with .NET 10 in Release mode, runs the regression executable, and
uploads a NuGet package artifact for pull requests and pushes to `main`,
`feat/**`, and `Feat/**`.

To publish `6502_net_standard` to GitHub Packages, push a version tag:

```sh
git tag v0.1.1
git push origin v0.1.1
```

Tags must follow `vMAJOR.MINOR.PATCH`, optionally with a prerelease suffix
such as `v0.1.1-beta.1`. The tag supplies the NuGet package version without
editing the project file. Publishing runs only after a successful build,
test run, and pack; branches and pull requests never publish.

The publishing job uses the built-in `GITHUB_TOKEN` with `packages: write`;
no NuGet.org API key is required. Packages are published to
`https://nuget.pkg.github.com/jose-garcia-gaxiola/index.json` and linked to
this repository through its package metadata. If repository or organization
policies restrict package creation, allow the workflow to publish packages.
Downloading from this feed requires GitHub authentication with package-read
access.

# License (OLC-3)
Copyright 2018, 2019, 2020, 2021 OneLoneCoder.com

Redistribution and use in source and binary forms, with or without 
modification, are permitted provided that the following conditions 
are met:

1. Redistributions or derivations of source code must retain the above 
   copyright notice, this list of conditions and the following disclaimer.

2. Redistributions or derivative works in binary form must reproduce 
   the above copyright notice. This list of conditions and the following 
   disclaimer must be reproduced in the documentation and/or other 
   materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its 
   contributors may be used to endorse or promote products derived 
   from this software without specific prior written permission.
    
THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS 
"AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT 
LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR 
A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT 
HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT 
LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT 
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
