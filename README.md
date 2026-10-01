# Unsigned (2008)
Unsigned is a Rock Band clone built off of GarageBand (2004) eventually stemed into the 5-fret clone we know today.

# WHAT DOESN'T COMPILE
**Everything, more specifically what is compatible without doing anymore work but doesn't compile.**
- MS3DProcessor
  - REQUIRES FVProductionsUtility
- SongdataConverter
  - REQUIRES SongDataIO
- SongdataVersionChecker
  - REQUIRES SongDataIO

# WHAT DOES COMPILE
**it works :3**
- VenueCompiler
  - Built against version 'v2.0.50727' of the runtime and cannot be loaded in the 4.0 runtime (obviously lol)