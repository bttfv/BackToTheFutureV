class sid extends MovieClip
{
	var globalMC : MovieClip
	
	var sidBackground : MovieClip
	
	var sidLeds : Array = [10];
	
	// Constructor
	function sid(globalMovieClip)
	{
		// Invoke parent class constructor
		super();
		
		// Enable gfx extensions
		_global.gfxExtensions = true;
		
		// Save ref to global movie clip
		this.globalMC = globalMovieClip;
		
		this.sidBackground = this.globalMC.attachMovie("sidBackground", "sidBackground", this.globalMC.getNextHighestDepth());
		sidBackground.stop();
				
		for (var column = 0;column < 20;column++) 
		{
			sidLeds[column] = Array(20);
			
			for (var i=0; i < 13;i++) 
			{
				sidLeds[column][i] = this.globalMC.attachMovie("sidLedGreen", "ledCol" + column + "Row" + i, this.globalMC.getNextHighestDepth());
			
				sidLeds[column][i].stop();
				sidLeds[column][i]._x = 82 + column * 25.6;
				sidLeds[column][i]._y = 507 - i * 25.5;			
			}
		
			for (var i=13; i < 19;i++) 
			{
				sidLeds[column][i] = this.globalMC.attachMovie("sidLedYellow", "ledCol" + column + "Row" + i, this.globalMC.getNextHighestDepth());
			
				sidLeds[column][i].stop();
				sidLeds[column][i]._x = 82 + column * 25.6;
				sidLeds[column][i]._y = 507 - i * 25.5;
			}
		
			sidLeds[column][19] = this.globalMC.attachMovie("sidLedRed", "ledCol" + column + "Row" + i, this.globalMC.getNextHighestDepth());
			
			sidLeds[column][19].stop();
			sidLeds[column][19]._x = 82 + column * 25.6;
			sidLeds[column][19]._y = 507 - 19 * 25.5;	
		}
	}
	
	function setLed(column, row, _state) 
	{
		sidLeds[column][row].gotoAndStop(_state + 1);
	}
}