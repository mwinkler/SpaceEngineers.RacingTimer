SpaceEngineers.RacingTimer
==========================

This script/mod for SpaceEngineers calculate lap times for every kind of race tracks. 

To use, place a sensor and name it "race.start". 
Every time you pass the sensor, you get your laptime. 

To add checkpoints on your track, place a sensor and name it "race.checkpoint.[YourCheckpointName]" 

Add a panel and name it "race.control" and you can: 
Button 1: Switch between Race/Hotlap mode 
Button 2: Reinit track if you add/remove checkpoints 
Button 3: Increase total race laps 
Button 4: Decrease total race laps 

If you pass a checkpoint or finish a lap, you get split and lap times. 
The color means: 
Blue: You mark a new overall best time 
Green: You mark a new personal best time 
White: No new best time 