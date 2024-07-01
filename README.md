# SR exercise

Notes:
- Assumed that score in match can decrese due to fouls, but not below 0.
- Assumed that commands are simple and all parms for `IMatchService` should not be boxed into `Command` class.
- Added time stamps to matches.
- Added delete match command that is idempotent.